using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;

namespace FIAP.TechChallenge.OS.Domain.Tests.Entities;

public sealed class OrdemServicoWorkflowTests
{
    private static OrdemServico Order() => OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Motor falhando").Value!;
    private static OrdemServico Awaiting()
    {
        var order = Order();
        _ = order.RegistrarDiagnosticoIniciado(DateTime.UtcNow);
        _ = order.RegistrarDiagnosticoConcluido(DateTime.UtcNow);
        return order;
    }

    [Test]
    public void Decision_ShouldRemainPendingAndRejectContradiction()
    {
        var order = Awaiting();
        var now = DateTime.UtcNow;
        var accepted = order.DecidirExecucao(DecisaoExecucao.Autorizada, now);
        var repeated = order.DecidirExecucao(DecisaoExecucao.Autorizada, now.AddSeconds(1));
        var conflicting = order.DecidirExecucao(DecisaoExecucao.CancelamentoSolicitado, now);
        Assert.Multiple(() =>
        {
            Assert.That(accepted.Value, Is.True);
            Assert.That(repeated.Value, Is.False);
            Assert.That(conflicting.IsSuccess, Is.False);
            Assert.That(order.Status, Is.EqualTo(StatusOrdemServico.AguardandoAprovacao));
            Assert.That(order.DataDecisaoUtc, Is.EqualTo(now));
        });
        _ = order.RegistrarExecucaoIniciada(now.AddSeconds(2));
        _ = order.RegistrarExecucaoConcluida(now.AddSeconds(3));
        var recovered = OrdemServico.Rehydrate(order.Snapshot()).Value!;
        Assert.Multiple(() =>
        {
            Assert.That(recovered.Status, Is.EqualTo(StatusOrdemServico.Finalizada));
            Assert.That(recovered.DataFinalizacao, Is.EqualTo(now.AddSeconds(3)));
            Assert.That(recovered.Decisao, Is.EqualTo(DecisaoExecucao.Autorizada));
        });
    }

    [Test]
    public void Cancellation_ShouldRequireDecisionAndPreserveFirstConfirmation()
    {
        var order = Awaiting();
        Assert.That(order.RegistrarExecucaoCancelada(DateTime.UtcNow).IsSuccess, Is.False);
        _ = order.DecidirExecucao(DecisaoExecucao.CancelamentoSolicitado, DateTime.UtcNow);
        var first = DateTime.UtcNow;
        _ = order.RegistrarExecucaoCancelada(first);
        var repeated = order.RegistrarExecucaoCancelada(first.AddSeconds(1));
        Assert.Multiple(() =>
        {
            Assert.That(repeated.Value, Is.False);
            Assert.That(order.Status, Is.EqualTo(StatusOrdemServico.Cancelada));
            Assert.That(order.DataCancelamento, Is.EqualTo(first));
            Assert.That(order.RegistrarExecucaoIniciada(first).IsSuccess, Is.False);
        });
    }

    [Test]
    public void Workflow_ShouldRejectInvalidOrderAndInvalidDates()
    {
        var order = Order();
        Assert.Multiple(() =>
        {
            Assert.That(order.DecidirExecucao(DecisaoExecucao.Autorizada, DateTime.UtcNow).IsSuccess, Is.False);
            Assert.That(order.DecidirExecucao(DecisaoExecucao.Nenhuma, DateTime.UtcNow).IsSuccess, Is.False);
            Assert.That(order.DecidirExecucao(DecisaoExecucao.Autorizada, DateTime.Now).IsSuccess, Is.False);
            Assert.That(order.RegistrarDiagnosticoConcluido(DateTime.UtcNow).IsSuccess, Is.False);
            Assert.That(order.RegistrarExecucaoConcluida(DateTime.UtcNow).IsSuccess, Is.False);
            Assert.That(order.RegistrarDiagnosticoIniciado(default).IsSuccess, Is.False);
        });
        var snapshot = Awaiting().Snapshot();
        Assert.Multiple(() =>
        {
            Assert.That(OrdemServico.Rehydrate(snapshot with { DataEnvioAprovacao = null }).IsSuccess, Is.False);
            Assert.That(OrdemServico.Rehydrate(snapshot with { Status = StatusOrdemServico.Entregue }).IsSuccess, Is.False);
            Assert.That(OrdemServico.Rehydrate(snapshot with { Decisao = DecisaoExecucao.Autorizada }).IsSuccess, Is.False);
            Assert.That(OrdemServico.Rehydrate(snapshot with { Version = -1 }).IsSuccess, Is.False);
        });
    }

    [Test]
    public void Projection_ShouldIgnoreCompletedTransitionsWithoutMovingStateBackwards()
    {
        var order = Awaiting();
        var started = order.DataInicioDiagnostico;
        var completed = order.DataEnvioAprovacao;
        var first = order.RegistrarDiagnosticoIniciado(DateTime.UtcNow.AddMinutes(1));
        var second = order.RegistrarDiagnosticoConcluido(DateTime.UtcNow.AddMinutes(2));
        Assert.Multiple(() =>
        {
            Assert.That(first.Value, Is.False);
            Assert.That(second.Value, Is.False);
            Assert.That(order.Status, Is.EqualTo(StatusOrdemServico.AguardandoAprovacao));
            Assert.That(order.DataInicioDiagnostico, Is.EqualTo(started));
            Assert.That(order.DataEnvioAprovacao, Is.EqualTo(completed));
        });
    }
}
