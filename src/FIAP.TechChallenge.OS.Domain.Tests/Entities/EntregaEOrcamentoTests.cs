using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;
namespace FIAP.TechChallenge.OS.Domain.Tests.Entities;

public sealed class EntregaEOrcamentoTests
{
    [Test]
    public void Delivery_ShouldRequireCompletion_RestoreSnapshot_AndIgnoreLateEvents()
    {
        var order = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Motor falhando").Value!;
        Assert.That(order.Entregar(DateTime.UtcNow).IsSuccess, Is.False);
        order.RegistrarDiagnosticoIniciado(DateTime.UtcNow);
        order.RegistrarDiagnosticoConcluido(DateTime.UtcNow, Guid.NewGuid(), 0, new([], []));
        order.DecidirExecucao(DecisaoExecucao.Autorizada, DateTime.UtcNow);
        Assert.That(order.Entregar(DateTime.UtcNow).IsSuccess, Is.False);
        order.RegistrarPagamento(order.OrcamentoId!.Value, Guid.NewGuid(), 0, "BRL", true, "mock");
        order.RegistrarExecucaoIniciada(DateTime.UtcNow); order.RegistrarExecucaoConcluida(DateTime.UtcNow);
        Assert.Multiple(() =>
        {
            Assert.That(order.Entregar(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)).IsSuccess, Is.False);
            Assert.That(order.Entregar(order.DataFinalizacao!.Value.AddSeconds(-1)).IsSuccess, Is.False);
        });
        var now = DateTime.UtcNow;
        Assert.That(order.Entregar(now).Value, Is.True);
        var restored = OrdemServico.Rehydrate(order.Snapshot()).Value!;
        Assert.Multiple(() =>
        {
            Assert.That(restored.Entregar(now.AddMinutes(1)).Value, Is.False);
            Assert.That(restored.RegistrarExecucaoConcluida(DateTime.UtcNow).Value, Is.False);
            Assert.That(restored.RegistrarExecucaoIniciada(DateTime.UtcNow).Value, Is.False);
            Assert.That(restored.Status, Is.EqualTo(StatusOrdemServico.Entregue));
            Assert.That(restored.DataEntrega, Is.EqualTo(now));
            Assert.That(OrdemServico.Rehydrate(order.Snapshot() with { DataEntrega = null }).IsSuccess, Is.False);
        });
    }
    [Test]
    public void Budget_ShouldRejectInconsistentItems_Overflow_AndChangedSnapshots()
    {
        var item = new ItemOrcamento(Guid.NewGuid(), "Revisão", 2, 10.125m, 20.25m);
        var budget = new OrcamentoOrdem([item], []);
        Assert.Multiple(() =>
        {
            Assert.That(budget.Valido(20.25m), Is.True);
            Assert.That(new OrcamentoOrdem([], []).Valido(0), Is.True);
            Assert.That(budget.Valido(20.26m), Is.False);
            Assert.That(new OrcamentoOrdem([item, item], []).Valido(40.5m), Is.False);
            Assert.That(new OrcamentoOrdem([item with { ValorUnitario = decimal.MaxValue }], []).Valido(20.25m), Is.False);
        });
        var order = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Motor falhando").Value!;
        order.RegistrarDiagnosticoIniciado(DateTime.UtcNow);
        Assert.That(order.RegistrarDiagnosticoConcluido(DateTime.UtcNow, Guid.NewGuid(), 20.25m, budget).Value, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(order.RegistrarDiagnosticoConcluido(DateTime.UtcNow, Guid.NewGuid(), 20.25m, new([item], [])).Value, Is.False);
            Assert.That(order.RegistrarDiagnosticoConcluido(DateTime.UtcNow, Guid.NewGuid(), 20.25m, new([item with { Descricao = "Outro serviço" }], [])).IsSuccess, Is.False);
        });
    }
}
