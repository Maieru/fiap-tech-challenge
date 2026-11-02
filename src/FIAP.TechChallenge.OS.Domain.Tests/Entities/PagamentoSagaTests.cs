using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;
namespace FIAP.TechChallenge.OS.Domain.Tests.Entities;
public class PagamentoSagaTests
{
 [TestCase(true)] [TestCase(false)] public void ResultControlsExecutionAndCompensation(bool approved)
 {
  var order = OrdemServico.Create(Guid.NewGuid(),Guid.NewGuid(),"Diagnosticar motor").Value!;
  order.RegistrarDiagnosticoIniciado(DateTime.UtcNow); order.RegistrarDiagnosticoConcluido(DateTime.UtcNow,Guid.NewGuid(),123.456m);
  order.DecidirExecucao(DecisaoExecucao.Autorizada,DateTime.UtcNow);
  Assert.Multiple(() => { Assert.That(order.RegistrarExecucaoIniciada(DateTime.UtcNow).IsSuccess,Is.False); Assert.That(order.DecidirExecucao(DecisaoExecucao.CancelamentoSolicitado,DateTime.UtcNow).IsSuccess,Is.False); Assert.That(order.RegistrarPagamento(order.OrcamentoId!.Value,Guid.NewGuid(),1,"BRL",true,"x").IsSuccess,Is.False); });
  var charge = Guid.NewGuid(); var budget = order.OrcamentoId!.Value;
  Assert.That(order.RegistrarPagamento(budget,charge,123.456m,"BRL",approved,"result").Value,Is.True);
  Assert.That(order.RegistrarPagamento(budget,charge,123.456m,"BRL",approved,"result").Value,Is.False);
  Assert.That(order.RegistrarPagamento(budget,charge,123.456m,"BRL",!approved,"different").IsSuccess,Is.False);
  if (approved) { Assert.That(order.RegistrarExecucaoIniciada(DateTime.UtcNow).IsSuccess,Is.True); order.RegistrarExecucaoConcluida(DateTime.UtcNow); }
  else Assert.That(order.RegistrarExecucaoCancelada(DateTime.UtcNow).IsSuccess,Is.True);
  var recovered = OrdemServico.Rehydrate(order.Snapshot());
  Assert.Multiple(() => { Assert.That(recovered.IsSuccess,Is.True); Assert.That(recovered.Value!.Decisao,Is.EqualTo(DecisaoExecucao.Autorizada)); Assert.That(recovered.Value.Pagamento!.Etapa,Is.EqualTo(approved ? EtapaPagamento.Concluida : EtapaPagamento.Compensada)); });
 }
 [Test] public void LegacyBudgetCannotBeInvented()
 {
  var order = OrdemServico.Create(Guid.NewGuid(),Guid.NewGuid(),"Motor falhando").Value!;
  order.RegistrarDiagnosticoIniciado(DateTime.UtcNow);order.RegistrarDiagnosticoConcluido(DateTime.UtcNow);
  Assert.That(order.DecidirExecucao(DecisaoExecucao.Autorizada,DateTime.UtcNow).IsSuccess,Is.False);
 }
}
