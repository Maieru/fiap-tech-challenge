using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using FIAP.TechChallenge.Execution.Application.Messaging;
using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using Maieru.FiapTechChallenge.Chassis;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.OS.Integration.Tests;

[NonParallelizable]
public sealed class OsMessagingTests
{
    private OsTestHost host = null!;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    [OneTimeSetUp] public async Task Start() { host = new OsTestHost(); await host.StartAsync(); }
    [SetUp] public Task Reset() => host.ResetAsync();
    [OneTimeTearDown] public async Task Stop() => await host.DisposeAsync();

    private async Task<Guid> Order()
    {
        var customer = await host.CreateClientAsync();
        var vehicle = await host.CreateVehicleAsync(customer.GetProperty("id").GetGuid());
        var order = await host.CreateOrderAsync(customer.GetProperty("id").GetGuid(), vehicle.GetProperty("id").GetGuid());
        return order.GetProperty("id").GetGuid();
    }
    private async Task Send<T>(Guid id, string type, T payload, Guid? messageId = null)
    {
        using var producer = new ProducerBuilder<string, string>(new ProducerConfig
        { BootstrapServers = host.BootstrapServers, Acks = Acks.All, EnableIdempotence = true }).Build();
        var envelope = new MessageEnvelope<T>(messageId ?? Guid.NewGuid(), type, 1, DateTimeOffset.UtcNow, "execution-confirmation", null,
            "00-12345678901234567890123456789012-1234567890123456-01", null, payload);
        await producer.ProduceAsync(ExecutionMessaging.Topic, new Message<string, string> { Key = id.ToString("D"), Value = JsonSerializer.Serialize(envelope, Json) });
    }
    private async Task<JsonElement> Wait(Guid id, int status)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (true)
        {
            var order = await host.Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{id}", timeout.Token);
            if (order.GetProperty("status").GetInt32() == status) return order;
            await Task.Delay(100, timeout.Token);
        }
    }
    private async Task Awaiting(Guid id)
    {
        await Send(id, DiagnosticoIniciado.Type, new DiagnosticoIniciado(id));
        await Wait(id, 2);
        await Send(id, DiagnosticoConcluido.Type, new DiagnosticoConcluido(id, [], [], 0));
        await Wait(id, 3);
    }
    private IReadOnlyList<MessageEnvelope<JsonElement>> Outgoing(Guid id)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        { BootstrapServers = host.BootstrapServers, GroupId = $"inspect-{Guid.NewGuid():N}", EnableAutoCommit = false }).Build();
        var partition = new TopicPartition("oficina.os.events.v1", 0);
        var high = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(10)).High.Value;
        consumer.Assign(new TopicPartitionOffset(partition, Offset.Beginning));
        var messages = new List<MessageEnvelope<JsonElement>>();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try
        {
            while (high > 0)
            {
                var record = consumer.Consume(timeout.Token);
                if (record.Message.Key == id.ToString("D"))
                    messages.Add(JsonSerializer.Deserialize<MessageEnvelope<JsonElement>>(record.Message.Value, Json)!);
                if (record.Offset.Value >= high - 1) break;
            }
        }
        finally { consumer.Close(); }
        return messages;
    }

    [Test]
    public async Task Creation_ShouldPublishNormalizedContractWithCorrelation()
    {
        host.Client.DefaultRequestHeaders.Add("X-Correlation-ID", "os-created");
        var id = await Order();
        var envelope = Outgoing(id).Single();
        Assert.Multiple(() =>
        {
            Assert.That(envelope.Type, Is.EqualTo("os.ordem-servico-criada"));
            Assert.That(envelope.Version, Is.EqualTo(1));
            Assert.That(envelope.CorrelationId, Is.EqualTo("os-created"));
            Assert.That(envelope.Payload.GetProperty("ordemServicoId").GetGuid(), Is.EqualTo(id));
            Assert.That(envelope.Payload.GetProperty("descricaoProblema").GetString(), Is.EqualTo("Motor com falha"));
            Assert.That(envelope.TraceParent, Is.Not.Null);
        });
    }

    [Test]
    public async Task Approval_ShouldStayPendingUntilConfirmation_AndNotRepublishRepeatedDecision()
    {
        var id = await Order();
        await Awaiting(id);
        using var accepted = await host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{id}/aprovar-execucao", new { });
        var body = await accepted.Content.ReadFromJsonAsync<JsonElement>();
        using var repeated = await host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{id}/aprovar-execucao", new { });
        using var conflict = await host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{id}/cancelar", new { });
        var pending = await Wait(id, 3);
        Assert.Multiple(() =>
        {
            Assert.That(accepted.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(repeated.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(conflict.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(body.GetProperty("pendente").GetBoolean(), Is.True);
            Assert.That(pending.GetProperty("decisao").GetInt32(), Is.EqualTo(1));
            Assert.That(Outgoing(id).Count(x => x.Type == "os.execucao-autorizada"), Is.EqualTo(1));
        });
        var messageId = Guid.NewGuid();
        await Send(id, ExecucaoIniciada.Type, new ExecucaoIniciada(id), messageId);
        await Wait(id, 4);
        await Send(id, ExecucaoConcluida.Type, new ExecucaoConcluida(id));
        var finished = await Wait(id, 5);
        await Send(id, ExecucaoIniciada.Type, new ExecucaoIniciada(id), messageId);
        await Send(id, ExecucaoConcluida.Type, new ExecucaoConcluida(id));
        // A second order's first transition is a barrier on the same input partition.
        var original = await host.Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{id}");
        var barrier = (await host.CreateOrderAsync(original.GetProperty("clienteId").GetGuid(),
            original.GetProperty("veiculoId").GetGuid())).GetProperty("id").GetGuid();
        await Send(barrier, DiagnosticoIniciado.Type, new DiagnosticoIniciado(barrier));
        await Wait(barrier, 2);
        var history = await host.Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{id}/historico");
        Assert.Multiple(() =>
        {
            Assert.That(finished.GetProperty("dataFinalizacao").ValueKind, Is.EqualTo(JsonValueKind.String));
            Assert.That(history.GetArrayLength(), Is.EqualTo(6));
            Assert.That(history.EnumerateArray().Count(x => x.GetProperty("tipo").GetString() == "ExecucaoIniciada"), Is.EqualTo(1));
            Assert.That(history.EnumerateArray().Where(x => x.GetProperty("tipo").GetString() == "ExecucaoConcluida")
                .Single().GetProperty("correlationId").GetString(), Is.EqualTo("execution-confirmation"));
        });
    }

    [Test]
    public async Task Cancellation_ShouldOnlyMarkCancelledAfterExecutionConfirmation()
    {
        var id = await Order();
        await Awaiting(id);
        using var accepted = await host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{id}/cancelar", new { });
        using var repeated = await host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{id}/cancelar", new { });
        var pending = await Wait(id, 3);
        Assert.Multiple(() =>
        {
            Assert.That(accepted.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(repeated.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            Assert.That(pending.GetProperty("decisao").GetInt32(), Is.EqualTo(2));
            Assert.That(Outgoing(id).Count(x => x.Type == "os.cancelamento-solicitado"), Is.EqualTo(1));
        });
        await Send(id, ExecucaoCancelada.Type, new ExecucaoCancelada(id));
        var cancelled = await Wait(id, 7);
        Assert.That(cancelled.GetProperty("dataCancelamento").ValueKind, Is.EqualTo(JsonValueKind.String));
    }

    [Test]
    public async Task Decisions_ShouldRejectInvalidStateAndRequireRealAuthentication()
    {
        var id = await Order();
        using var invalid = await host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{id}/aprovar-execucao", new { });
        using var missing = await host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{Guid.NewGuid()}/cancelar", new { });
        host.Client.DefaultRequestHeaders.Authorization = null;
        using var unauthorized = await host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{id}/cancelar", new { });
        Assert.Multiple(() =>
        {
            Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(unauthorized.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    public async Task Decisions_ShouldSerializeConcurrentContradictoryRequests()
    {
        var id = await Order();
        await Awaiting(id);
        var responses = await Task.WhenAll(
            host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{id}/aprovar-execucao", new { }),
            host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{id}/cancelar", new { }));
        try
        {
            var messages = Outgoing(id);
            Assert.Multiple(() =>
            {
                Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.Accepted), Is.EqualTo(1));
                Assert.That(responses.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1));
                Assert.That(messages.Count(x => x.Type is "os.execucao-autorizada" or "os.cancelamento-solicitado"), Is.EqualTo(1));
            });
        }
        finally { foreach (var response in responses) response.Dispose(); }
    }
}
