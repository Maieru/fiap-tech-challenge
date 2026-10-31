using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;

namespace FIAP.TechChallenge.OS.Domain.Tests.Entities;

public sealed class FluxoOrdemTests
{
    private static OrdemServico Create() => OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Motor com falha").Value!;

    [Test]
    public void Flow_ShouldPersistProgressAndClearPendingAuthorization()
    {
        var value = Create();
        var now = DateTime.UtcNow;
        Assert.That(value.AplicarEvento("DiagnosticoIniciado", now, null, null).IsSuccess, Is.True);
        Assert.That(value.AplicarEvento("DiagnosticoConcluido", now, null, "{\"total\":125}").IsSuccess, Is.True);
        var request = value.Solicitar("Autorizacao");
        Assert.Multiple(() => { Assert.That(request.IsSuccess, Is.True); Assert.That(value.AcaoPendente, Is.EqualTo("Autorizacao")); });
        Assert.That(value.AplicarEvento("ExecucaoIniciada", now, request.Value, null).IsSuccess, Is.True);
        Assert.That(value.AplicarEvento("ExecucaoFinalizada", now, null, null).IsSuccess, Is.True);
        var restored = OrdemServico.Rehydrate(new OrdemServicoSnapshot(value.Id, value.ClienteId, value.VeiculoId, value.DescricaoProblema,
            value.DataCriacao, value.Status, value.DataInicioDiagnostico, value.DataEnvioAprovacao, value.DataInicioExecucao,
            value.DataFinalizacao, value.AcaoPendente, value.SolicitacaoPendenteId, value.DiagnosticoJson)).Value!;
        Assert.Multiple(() =>
        {
            Assert.That(restored.Status, Is.EqualTo(StatusOrdemServico.Finalizada));
            Assert.That(restored.DataFinalizacao, Is.EqualTo(now));
            Assert.That(restored.AcaoPendente, Is.Null);
            Assert.That(restored.SolicitacaoPendenteId, Is.Null);
            Assert.That(restored.Diagnostico!.Value.GetProperty("total").GetInt32(), Is.EqualTo(125));
        });
    }

    [TestCase("Autorizacao")]
    [TestCase("Cancelamento")]
    public void Request_ShouldRequireAwaitingApprovalAndSerializeCompetingDecisions(string action)
    {
        var value = Create();
        Assert.That(value.Solicitar(action).IsSuccess, Is.False);
        _ = value.AplicarEvento("DiagnosticoIniciado", DateTime.UtcNow, null, null);
        _ = value.AplicarEvento("DiagnosticoConcluido", DateTime.UtcNow, null, "{}");
        Assert.That(value.Solicitar(action).IsSuccess, Is.True);
        Assert.That(value.Solicitar(action == "Autorizacao" ? "Cancelamento" : "Autorizacao").IsSuccess, Is.False);
    }

    [Test]
    public void Events_ShouldRejectWrongStateRequestsAndUnexpectedTypes()
    {
        var value = Create();
        Assert.Multiple(() =>
        {
            Assert.That(value.AplicarEvento("ExecucaoFinalizada", DateTime.UtcNow, null, null).IsSuccess, Is.False);
            Assert.That(value.AplicarEvento("Unknown", DateTime.UtcNow, null, null).IsSuccess, Is.False);
        });
        _ = value.AplicarEvento("DiagnosticoIniciado", DateTime.UtcNow, null, null);
        Assert.That(value.AplicarEvento("DiagnosticoConcluido", DateTime.UtcNow, null, null).IsSuccess, Is.False);
        _ = value.AplicarEvento("DiagnosticoConcluido", DateTime.UtcNow, null, "{}");
        Assert.That(value.Solicitar("Unknown").IsSuccess, Is.False);
        var request = value.Solicitar("Cancelamento").Value;
        Assert.Multiple(() =>
        {
            Assert.That(value.AplicarEvento("ExecucaoIniciada", DateTime.UtcNow, request, null).IsSuccess, Is.False);
            Assert.That(value.AplicarEvento("ExecucaoCancelada", DateTime.UtcNow, Guid.NewGuid(), null).IsSuccess, Is.False);
        });
        Assert.That(value.AplicarEvento("ExecucaoCancelada", DateTime.UtcNow, request, null).IsSuccess, Is.True);
        Assert.That(value.Status, Is.EqualTo(StatusOrdemServico.Cancelada));
    }
    [Test]
    public void RejectedRequest_ShouldLeaveBusinessStateAndAllowAnotherDecision()
    {
        var value = Create();
        _ = value.AplicarEvento("DiagnosticoIniciado", DateTime.UtcNow, null, null);
        _ = value.AplicarEvento("DiagnosticoConcluido", DateTime.UtcNow, null, "{}");
        var request = value.Solicitar("Autorizacao").Value;
        Assert.That(value.AplicarEvento("SolicitacaoRecusada", DateTime.UtcNow, request, null).IsSuccess, Is.True);
        Assert.Multiple(() => { Assert.That(value.Status, Is.EqualTo(StatusOrdemServico.AguardandoAprovacao)); Assert.That(value.AcaoPendente, Is.Null); });
        Assert.That(value.Solicitar("Cancelamento").IsSuccess, Is.True);
    }
    [Test]
    public void Rehydrate_ShouldRejectInvalidPersistentState()
    {
        var value = Create();
        var snapshot = new OrdemServicoSnapshot(value.Id, value.ClienteId, value.VeiculoId, value.DescricaoProblema, value.DataCriacao,
            (StatusOrdemServico)100, null, null, null, null, null, null, null);
        Assert.That(OrdemServico.Rehydrate(snapshot).IsSuccess, Is.False);
    }
}
