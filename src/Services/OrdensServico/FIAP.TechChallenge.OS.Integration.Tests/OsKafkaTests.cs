using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Confluent.Kafka;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FIAP.TechChallenge.OS.Infrastructure.Messaging;
using Maieru.FiapTechChallenge.Chassis;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.OS.Integration.Tests;

[TestFixture, NonParallelizable]
public sealed class OsKafkaTests
{
    private IContainer _kafka = null!;
    private OsTestHost _host = null!;
    private string _bootstrap = "";
    private IProducer<string, string> _producer = null!;

    [OneTimeSetUp]
    public async Task Start()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0); listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop();
        _bootstrap = $"localhost:{port}";
        _kafka = new ContainerBuilder("apache/kafka:4.4.0@sha256:833b867a9585a141da455dc2b57431c65cbccb27f11c91f694444cca9f777e0f")
            .WithPortBinding(port, 9092)
            .WithEnvironment("CLUSTER_ID", "MkU3OEVBNTcwNTJENDM2Qk")
            .WithEnvironment("KAFKA_NODE_ID", "1").WithEnvironment("KAFKA_PROCESS_ROLES", "broker,controller")
            .WithEnvironment("KAFKA_LISTENERS", "PLAINTEXT://:9092,CONTROLLER://:19093")
            .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://" + _bootstrap)
            .WithEnvironment("KAFKA_LISTENER_SECURITY_PROTOCOL_MAP", "PLAINTEXT:PLAINTEXT,CONTROLLER:PLAINTEXT")
            .WithEnvironment("KAFKA_INTER_BROKER_LISTENER_NAME", "PLAINTEXT").WithEnvironment("KAFKA_CONTROLLER_LISTENER_NAMES", "CONTROLLER")
            .WithEnvironment("KAFKA_CONTROLLER_QUORUM_VOTERS", "1@localhost:19093")
            .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
            .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR", "1").WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_MIN_ISR", "1")
            .WithEnvironment("KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS", "0")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Kafka Server started")).Build();
        await _kafka.StartAsync();
        _host = new() { ManualAuthorizationEnabled = true, KafkaBootstrapServers = _bootstrap };
        await _host.StartAsync();
        _producer = new ProducerBuilder<string, string>(new ProducerConfig { BootstrapServers = _bootstrap, EnableIdempotence = true }).Build();
        await WaitAsync(async () => (await _host.Client.GetAsync(OsTestHost.Root + "/health/ready")).IsSuccessStatusCode);
    }

    [OneTimeTearDown]
    public async Task Stop()
    {
        _producer?.Dispose();
        if (_host is not null) await _host.DisposeAsync();
        if (_kafka is not null) await _kafka.DisposeAsync();
    }

    private async Task<Guid> CreateAsync()
    {
        await _host.ResetAsync();
        var customer = await _host.CreateClientAsync(); var customerId = customer.GetProperty("id").GetGuid();
        var vehicle = await _host.CreateVehicleAsync(customerId);
        return (await _host.CreateOrderAsync(customerId, vehicle.GetProperty("id").GetGuid())).GetProperty("id").GetGuid();
    }
    private static MessageEnvelope<FlowPayload> Envelope(Guid id, string type, long sequence, Guid? request = null) =>
        new(Guid.NewGuid(), type, 1, DateTimeOffset.UtcNow, "os-kafka-test", null, null, null,
            new(id, sequence, SolicitacaoId: request, Snapshot: type == "DiagnosticoConcluido" ? JsonSerializer.SerializeToElement(new { ValorTotal = 125.50m }) : null));
    private Task SendAsync(MessageEnvelope<FlowPayload> message) => _producer.ProduceAsync("fiap.execution.events.v1",
        new Message<string, string> { Key = message.Payload.OrdemServicoId.ToString(), Value = IntegrationContext.Serialize(message) });

    [Test]
    public async Task Messages_ShouldPublishRequestsAndApplyConfirmedStatesOnce()
    {
        var id = await CreateAsync();
        var opening = await ReadAsync("fiap.os.events.v1", value => value.Contains(id.ToString()));
        Assert.That(opening.Message.Value, Does.Contain("OrdemCriada"));
        await SendAsync(Envelope(id, "DiagnosticoIniciado", 1));
        await SendAsync(Envelope(id, "DiagnosticoConcluido", 2));
        await StateAsync(id, 3);
        using var response = await _host.Client.PostAsync($"{OsTestHost.Root}/ordens/{id}/autorizar-execucao", null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
        var requestMessage = await ReadAsync("fiap.os.events.v1", value => value.Contains(id.ToString()) && value.Contains("AutorizacaoExecucaoSolicitada"));
        var request = JsonSerializer.Deserialize<MessageEnvelope<FlowPayload>>(requestMessage.Message.Value, IntegrationContext.Json)!;
        var started = Envelope(id, "ExecucaoIniciada", 3, request.Payload.SolicitacaoId);
        await SendAsync(started); await SendAsync(started); await SendAsync(Envelope(id, "ExecucaoFinalizada", 4));
        await StateAsync(id, 5);
        await using var db = _host.CreateDbContext();
        Assert.Multiple(() => { Assert.That(request.Payload.OperadorId, Is.EqualTo("operator")); Assert.That(request.Payload.Origem, Is.EqualTo("AdministrativaTemporaria")); });
        Assert.That(await db.HistoricoOrdens.CountAsync(x => x.OrdemServicoId == id && x.Tipo == "ExecucaoIniciada"), Is.EqualTo(1));
    }

    [Test]
    public async Task Gap_ShouldGoToDlqAndReplayAfterItsPredecessorWithoutRegression()
    {
        var id = await CreateAsync(); var gap = Envelope(id, "DiagnosticoConcluido", 2);
        await SendAsync(gap);
        var dead = await ReadAsync("fiap.execution.events.v1.dlq", value => value.Contains(gap.MessageId.ToString()));
        Assert.Multiple(() => { Assert.That(dead.Message.Value, Is.EqualTo(IntegrationContext.Serialize(gap))); Assert.That(dead.Message.Key, Is.EqualTo(id.ToString())); });
        await SendAsync(Envelope(id, "DiagnosticoIniciado", 1)); await StateAsync(id, 2);
        await SendAsync(gap); await StateAsync(id, 3);
        await SendAsync(gap with { MessageId = Guid.NewGuid() });
        await using var db = _host.CreateDbContext();
        await WaitAsync(async () => await db.Inbox.CountAsync() >= 3);
        Assert.That(await db.HistoricoOrdens.CountAsync(x => x.OrdemServicoId == id && x.Tipo == "DiagnosticoConcluido"), Is.EqualTo(1));
    }

    [Test]
    public async Task BrokerOutage_ShouldKeepTheOpeningInOutboxAndRecover()
    {
        await _kafka.StopAsync();
        try
        {
            var id = await CreateAsync();
            Assert.That((await _host.Client.GetAsync(OsTestHost.Root + "/health/live")).StatusCode, Is.EqualTo(HttpStatusCode.OK));
            await using var db = _host.CreateDbContext();
            Assert.That(await db.Outbox.AnyAsync(x => x.OrdemServicoId == id && !x.Published), Is.True);
            await _kafka.StartAsync();
            var message = await ReadAsync("fiap.os.events.v1", value => value.Contains(id.ToString()));
            Assert.That(message.Message.Value, Does.Contain("OrdemCriada"));
        }
        finally { await _kafka.StartAsync(); }
    }

    private Task StateAsync(Guid id, int state) => WaitAsync(async () =>
    {
        await using var db = _host.CreateDbContext();
        return await db.OrdensServico.AnyAsync(x => x.Id == id && (int)x.Status == state);
    });
    private async Task<ConsumeResult<string, string>> ReadAsync(string topic, Func<string, bool> matches) => await Task.Run(() =>
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig { BootstrapServers = _bootstrap,
            GroupId = "assert-" + Guid.NewGuid(), AutoOffsetReset = AutoOffsetReset.Earliest, EnableAutoCommit = false }).Build();
        consumer.Subscribe(topic); var until = DateTime.UtcNow.AddSeconds(30);
        try
        {
            while (DateTime.UtcNow < until)
            {
                var record = consumer.Consume(TimeSpan.FromMilliseconds(100));
                if (record is not null && matches(record.Message.Value)) return record;
            }
            throw new AssertionException("Mensagem esperada nao encontrada em " + topic);
        }
        finally { consumer.Close(); }
    });
    private static async Task WaitAsync(Func<Task<bool>> condition)
    {
        var until = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < until) { if (await condition()) return; await Task.Delay(100); }
        Assert.Fail("Estado esperado nao observado.");
    }
}
