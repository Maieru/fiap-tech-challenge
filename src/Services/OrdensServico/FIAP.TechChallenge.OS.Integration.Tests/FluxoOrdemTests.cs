using FIAP.TechChallenge.OS.Infrastructure.Persistence.Repositories;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FIAP.TechChallenge.OS.Infrastructure.Messaging;
using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;
using Maieru.FiapTechChallenge.Chassis;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.OS.Integration.Tests;

[TestFixture, NonParallelizable]
public sealed class FluxoOrdemTests
{
    private OsTestHost _host = null!;
    [OneTimeSetUp] public async Task Start() { _host = new() { ManualAuthorizationEnabled = true }; await _host.StartAsync(); }
    [OneTimeTearDown] public async Task Stop() { if (_host is not null) await _host.DisposeAsync(); }
    [SetUp] public Task Reset() => _host.ResetAsync();

    private async Task<Guid> PrepareAsync(bool awaiting = true)
    {
        var customer = await _host.CreateClientAsync(); var customerId = customer.GetProperty("id").GetGuid();
        var vehicle = await _host.CreateVehicleAsync(customerId); var order = await _host.CreateOrderAsync(customerId, vehicle.GetProperty("id").GetGuid());
        var id = order.GetProperty("id").GetGuid();
        if (awaiting) { await ApplyAsync(id, "DiagnosticoIniciado", 1); await ApplyAsync(id, "DiagnosticoConcluido", 2, snapshot: JsonSerializer.SerializeToElement(new { ValorTotal = 125.50m })); }
        return id;
    }
    private async Task<MessageEnvelope<FlowPayload>> ApplyAsync(Guid id, string type, long sequence, Guid? requestId = null, JsonElement? snapshot = null,
        MessageEnvelope<FlowPayload>? message = null)
    {
        message ??= new MessageEnvelope<FlowPayload>(Guid.NewGuid(), type, 1, DateTimeOffset.UtcNow, "flow-test", null, null, null,
            new FlowPayload(id, sequence, SolicitacaoId: requestId, Snapshot: snapshot));
        await using var db = _host.CreateDbContext();
        await new FluxoOrdemRepository(db, new IntegrationContext()).ApplyAsync(message, default);
        return message;
    }

    [Test]
    public async Task Authorization_ShouldAuditOperatorAndWaitForConfirmation()
    {
        var id = await PrepareAsync();
        var response = await _host.Client.PostAsync($"{OsTestHost.Root}/ordens/{id}/autorizar-execucao", null);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(); var request = body.GetProperty("solicitacaoId").GetGuid();
        var pending = await _host.Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{id}");
        await using (var db = _host.CreateDbContext())
        {
            var outbox = await db.Outbox.SingleAsync(x => x.OrdemServicoId == id && x.Sequence == 2);
            var message = JsonSerializer.Deserialize<MessageEnvelope<FlowPayload>>(outbox.Body, IntegrationContext.Json)!;
            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted)); Assert.That(response.Headers.Location, Is.Not.Null);
                Assert.That(pending.GetProperty("status").GetInt32(), Is.EqualTo(3)); Assert.That(pending.GetProperty("acaoPendente").GetString(), Is.EqualTo("Autorizacao"));
                Assert.That(message.Payload.OperadorId, Is.EqualTo("operator")); Assert.That(message.Payload.Origem, Is.EqualTo("AdministrativaTemporaria"));
            });
        }
        await ApplyAsync(id, "ExecucaoIniciada", 3, request);
        await ApplyAsync(id, "ExecucaoFinalizada", 4);
        var completed = await _host.Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{id}");
        var history = await _host.Client.GetFromJsonAsync<JsonElement>($"{OsTestHost.Root}/ordens/{id}/historico");
        Assert.Multiple(() =>
        {
            Assert.That(completed.GetProperty("status").GetInt32(), Is.EqualTo(5)); Assert.That(completed.GetProperty("dataFinalizacao").ValueKind, Is.Not.EqualTo(JsonValueKind.Null));
            Assert.That(completed.GetProperty("acaoPendente").ValueKind, Is.EqualTo(JsonValueKind.Null)); Assert.That(history.GetArrayLength(), Is.EqualTo(6));
        });
    }

    [Test]
    public async Task Decisions_ShouldAllowOnlyOneConcurrentPendingAction()
    {
        var id = await PrepareAsync();
        var results = await Task.WhenAll(_host.Client.PostAsync($"{OsTestHost.Root}/ordens/{id}/autorizar-execucao", null),
            _host.Client.PostAsync($"{OsTestHost.Root}/ordens/{id}/cancelar", null));
        await using var db = _host.CreateDbContext();
        Assert.Multiple(() =>
        {
            Assert.That(results.Count(x => x.StatusCode == HttpStatusCode.Accepted), Is.EqualTo(1));
            Assert.That(results.Count(x => x.StatusCode == HttpStatusCode.Conflict), Is.EqualTo(1));
        });
        Assert.That(await db.Outbox.CountAsync(x => x.OrdemServicoId == id && x.Sequence == 2), Is.EqualTo(1));
    }

    [Test]
    public async Task Cancellation_ShouldConfirmOnceAndIgnoreDuplicateMessages()
    {
        var id = await PrepareAsync(); var response = await _host.Client.PostAsync($"{OsTestHost.Root}/ordens/{id}/cancelar", null);
        var request = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("solicitacaoId").GetGuid();
        var message = await ApplyAsync(id, "ExecucaoCancelada", 3, request);
        await ApplyAsync(id, "ExecucaoCancelada", 3, request, message: message);
        await ApplyAsync(id, "ExecucaoCancelada", 3, request, message: message with { MessageId = Guid.NewGuid() });
        await using var db = _host.CreateDbContext();
        var row = await db.OrdensServico.SingleAsync(x => x.Id == id);
        Assert.Multiple(() => { Assert.That(row.Status, Is.EqualTo(Domain.Enums.StatusOrdemServico.Cancelada)); Assert.That(row.AcaoPendente, Is.Null); });
        Assert.That(await db.HistoricoOrdens.CountAsync(x => x.OrdemServicoId == id && x.Tipo == "ExecucaoCancelada"), Is.EqualTo(1));
    }

    [Test]
    public async Task Refusal_ShouldRecordReasonAndClearPendingAction()
    {
        var id = await PrepareAsync(); var response = await _host.Client.PostAsync($"{OsTestHost.Root}/ordens/{id}/cancelar", null);
        var request = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("solicitacaoId").GetGuid();
        var message = new MessageEnvelope<FlowPayload>(Guid.NewGuid(), "SolicitacaoRecusada", 1, DateTimeOffset.UtcNow, "refusal-test", null, null, null,
            new FlowPayload(id, 3, SolicitacaoId: request, Motivo: "Estado operacional alterado."));
        await ApplyAsync(id, message.Type, 3, message: message);
        await using var db = _host.CreateDbContext(); var row = await db.OrdensServico.SingleAsync(x => x.Id == id);
        var history = await db.HistoricoOrdens.SingleAsync(x => x.OrdemServicoId == id && x.Tipo == "SolicitacaoRecusada");
        Assert.Multiple(() => { Assert.That(row.AcaoPendente, Is.Null); Assert.That((int)row.Status, Is.EqualTo(3)); Assert.That(history.Detalhes, Does.Contain("Estado operacional alterado")); });
    }

    [Test]
    public async Task Backfill_ShouldCreateOnlyMissingOpeningEventsForReceivedOrders()
    {
        var id = await PrepareAsync(false);
        await using var db = _host.CreateDbContext();
        await db.Outbox.Where(x => x.OrdemServicoId == id).ExecuteDeleteAsync();
        await db.Database.ExecuteSqlRawAsync(OutboxBackfill.Sql); await db.Database.ExecuteSqlRawAsync(OutboxBackfill.Sql);
        var message = await db.Outbox.SingleAsync(x => x.OrdemServicoId == id);
        var parsed = JsonSerializer.Deserialize<MessageEnvelope<FlowPayload>>(message.Body, IntegrationContext.Json)!;
        Assert.Multiple(() =>
        {
            Assert.That(parsed.Type, Is.EqualTo("OrdemCriada")); Assert.That(parsed.Payload.OrdemServicoId, Is.EqualTo(id)); Assert.That(parsed.Payload.Sequence, Is.EqualTo(1));
        });
        Assert.That(await db.HistoricoOrdens.CountAsync(x => x.OrdemServicoId == id), Is.EqualTo(1));
    }

    [Test]
    public async Task IncomingEvent_ShouldRollbackHistoryAndInboxAndRejectSequenceGaps()
    {
        var id = await PrepareAsync(false);
        Assert.ThrowsAsync<InvalidOperationException>(async () => await ApplyAsync(id, "DiagnosticoConcluido", 2, snapshot: JsonSerializer.SerializeToElement(new { })));
        await using var db = _host.CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"HistoricoOrdens\" ADD CONSTRAINT \"FlowRollbackProbe\" CHECK (\"Tipo\" <> 'DiagnosticoIniciado')");
        try { Assert.ThrowsAsync<DbUpdateException>(async () => await ApplyAsync(id, "DiagnosticoIniciado", 1)); }
        finally { await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"HistoricoOrdens\" DROP CONSTRAINT \"FlowRollbackProbe\""); }
        var row = await db.OrdensServico.SingleAsync(x => x.Id == id);
        Assert.Multiple(() => { Assert.That((int)row.Status, Is.EqualTo(1)); Assert.That(row.InputSequence, Is.Zero); });
        Assert.That(await db.Inbox.CountAsync(), Is.Zero);
    }

    [TestCase("autorizar-execucao")][TestCase("cancelar")]
    public async Task NewOperations_ShouldValidateAuthenticationMissingOrdersAndState(string action)
    {
        var id = await PrepareAsync(false);
        var wrongState = await _host.Client.PostAsync($"{OsTestHost.Root}/ordens/{id}/{action}", null);
        var missing = await _host.Client.PostAsync($"{OsTestHost.Root}/ordens/{Guid.NewGuid()}/{action}", null);
        _host.Client.DefaultRequestHeaders.Authorization = null;
        var anonymous = await _host.Client.PostAsync($"{OsTestHost.Root}/ordens/{id}/{action}", null);
        Assert.Multiple(() =>
        {
            Assert.That(wrongState.StatusCode, Is.EqualTo(HttpStatusCode.Conflict)); Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(anonymous.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }
}

[TestFixture, NonParallelizable]
public sealed class ManualAuthorizationDisabledTests
{
    [TestCase("Testing", false)][TestCase("Production", true)]
    public async Task Authorization_ShouldBeUnavailableWhenFlagOrEnvironmentDisallowsIt(string environment, bool enabled)
    {
        await using var host = new OsTestHost { EnvironmentName = environment, ManualAuthorizationEnabled = enabled }; await host.StartAsync();
        var response = await host.Client.PostAsync($"{OsTestHost.Root}/ordens/{Guid.NewGuid()}/autorizar-execucao", null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
