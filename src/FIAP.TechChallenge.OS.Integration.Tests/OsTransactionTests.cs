using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Confluent.Kafka;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarDiagnosticoIniciado;
using FIAP.TechChallenge.OS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.TechChallenge.OS.Integration.Tests;

[NonParallelizable]
public sealed class OsTransactionTests
{
    private OsTestHost host = null!;
    [SetUp] public async Task Start() { host = new OsTestHost(); await host.StartAsync(); }
    [TearDown] public async Task Stop() => await host.DisposeAsync();

    private async Task<(Guid Customer, Guid Vehicle)> Register()
    {
        var customer = (await host.CreateClientAsync()).GetProperty("id").GetGuid();
        var vehicle = (await host.CreateVehicleAsync(customer)).GetProperty("id").GetGuid();
        return (customer, vehicle);
    }

    [Test]
    public async Task PublicationFailure_ShouldRollbackNewOrderAndAdministrativeDecision()
    {
        var (customer, vehicle) = await Register();
        var id = (await host.CreateOrderAsync(customer, vehicle)).GetProperty("id").GetGuid();
        using (var producer = new ProducerBuilder<string, string>(new ProducerConfig
            { BootstrapServers = host.BootstrapServers, EnableIdempotence = true, Acks = Acks.All }).Build())
        {
            foreach (var type in new[] { "execution.diagnostico-iniciado", "execution.diagnostico-concluido" })
            {
                var envelope = new Maieru.FiapTechChallenge.Chassis.MessageEnvelope<object>(Guid.NewGuid(), type, 1,
                    DateTimeOffset.UtcNow, "transaction-test", null, null, null,
                    new { OrdemServicoId = id, Servicos = Array.Empty<object>(), PecasInsumos = Array.Empty<object>(), ValorTotal = 0 });
                await producer.ProduceAsync("oficina.execution.events.v1", new Message<string, string>
                { Key = id.ToString("D"), Value = JsonSerializer.Serialize(envelope, new JsonSerializerOptions(JsonSerializerDefaults.Web)) });
            }
        }
        using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            while ((await host.Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{id}", timeout.Token))
                .GetProperty("status").GetInt32() != 3) await Task.Delay(100, timeout.Token);
        await host.StopKafkaAsync();
        using var create = await host.Client.PostAsJsonAsync(OsTestHost.Root + "/ordens",
            new { ClienteId = customer, VeiculoId = vehicle, DescricaoProblema = "Falha de publicação" });
        using var approve = await host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{id}/aprovar-execucao", new { });
        using var cancel = await host.Client.PutAsJsonAsync($"{OsTestHost.Root}/ordens/{id}/cancelar", new { });
        await using var persisted = host.CreateDbContext();
        var order = await persisted.OrdensServico.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(approve.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(cancel.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(order.Decisao, Is.EqualTo(DecisaoExecucao.Nenhuma));
            Assert.That(order.DataDecisaoUtc, Is.Null);
            Assert.That(persisted.HistoricoOrdens.Count(), Is.EqualTo(3));
        });
    }

    [Test]
    public async Task CommitFailureAfterPublication_ShouldExposeAcceptedOrphanMessageLimitation()
    {
        var (customer, vehicle) = await Register();
        await FailAtCommit("OrdensServico");
        using var response = await host.Client.PostAsJsonAsync(OsTestHost.Root + "/ordens",
            new { ClienteId = customer, VeiculoId = vehicle, DescricaoProblema = "Falha no commit" });
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = host.BootstrapServers, GroupId = $"orphan-{Guid.NewGuid():N}", EnableAutoCommit = false
        }).Build();
        consumer.Assign(new TopicPartitionOffset(OSMessaging.Topic, 0, Offset.Beginning));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var message = consumer.Consume(timeout.Token);
        consumer.Close();
        using var envelope = JsonDocument.Parse(message.Message.Value);
        await using var db = host.CreateDbContext();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(envelope.RootElement.GetProperty("type").GetString(), Is.EqualTo("os.ordem-servico-criada"));
            Assert.That(db.OrdensServico.Count(), Is.Zero);
            Assert.That(db.HistoricoOrdens.Count(), Is.Zero);
        });
    }

    [Test]
    public async Task ProjectionFailure_ShouldRollbackStateHistoryAndDedup_ThenAllowReplay()
    {
        var (customer, vehicle) = await Register();
        var id = (await host.CreateOrderAsync(customer, vehicle)).GetProperty("id").GetGuid();
        var messageId = Guid.NewGuid();
        await FailAtCommit("MensagensProcessadas");
        await using (var scope = host.Services.CreateAsyncScope())
        {
            scope.ServiceProvider.GetRequiredService<MessageContext>().IncomingMessageId = messageId;
            var useCase = scope.ServiceProvider.GetRequiredService<IRegistrarDiagnosticoIniciadoUseCase>();
            Assert.ThrowsAsync<Npgsql.PostgresException>(async () => await useCase.ExecuteAsync(new(id, DateTimeOffset.UtcNow)));
        }
        await using (var db = host.CreateDbContext())
        {
            var order = await db.OrdensServico.SingleAsync();
            Assert.Multiple(() =>
            {
                Assert.That(order.Status, Is.EqualTo(StatusOrdemServico.Recebida));
                Assert.That(order.DataInicioDiagnostico, Is.Null);
                Assert.That(db.HistoricoOrdens.Count(), Is.EqualTo(1));
                Assert.That(db.MensagensProcessadas.Count(), Is.Zero);
            });
            await db.Database.ExecuteSqlRawAsync("DROP TRIGGER fail_commit ON \"MensagensProcessadas\"");
        }
        for (var repeat = 0; repeat < 2; repeat++)
        {
            await using var scope = host.Services.CreateAsyncScope();
            scope.ServiceProvider.GetRequiredService<MessageContext>().IncomingMessageId = messageId;
            var result = await scope.ServiceProvider.GetRequiredService<IRegistrarDiagnosticoIniciadoUseCase>()
                .ExecuteAsync(new(id, DateTimeOffset.UtcNow));
            Assert.That(result.IsSuccess, Is.True);
        }
        await using var persisted = host.CreateDbContext();
        Assert.Multiple(() =>
        {
            Assert.That(persisted.OrdensServico.Single().Status, Is.EqualTo(StatusOrdemServico.EmDiagnostico));
            Assert.That(persisted.HistoricoOrdens.Count(), Is.EqualTo(2));
            Assert.That(persisted.MensagensProcessadas.Count(), Is.EqualTo(1));
        });
    }

    private async Task FailAtCommit(string table)
    {
        await using var db = host.CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("""
            CREATE FUNCTION fail_commit() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN RAISE EXCEPTION 'injected deferred constraint failure'; END $$;
            """);
        var sql = table switch
        {
            "OrdensServico" => "CREATE CONSTRAINT TRIGGER fail_commit AFTER INSERT ON \"OrdensServico\" DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION fail_commit()",
            "MensagensProcessadas" => "CREATE CONSTRAINT TRIGGER fail_commit AFTER INSERT ON \"MensagensProcessadas\" DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION fail_commit()",
            _ => throw new ArgumentException("Unsupported test table.", nameof(table))
        };
        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
