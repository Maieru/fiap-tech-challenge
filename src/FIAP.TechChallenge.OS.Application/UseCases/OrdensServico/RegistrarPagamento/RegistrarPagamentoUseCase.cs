using FIAP.TechChallenge.OS.Application.Abstractions;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Interfaces;
using Maieru.FiapTechChallenge.Chassis;
namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarPagamento;
public sealed class RegistrarPagamentoUseCase(IOrdemServicoRepository repository, IUnitOfWork unitOfWork, IProcessedMessageRepository processed, IMessagingService messaging, MessageContext context)
{
 public async Task<Result<bool>> ExecuteAsync(Guid ordem, Guid orcamento, Guid cobranca, decimal valor, string moeda, bool confirmado, string detalhe, DateTimeOffset occurredAt, CancellationToken ct)
 {
  await using var tx = await unitOfWork.BeginAsync(ct);
  if (context.IncomingMessageId is Guid mid && await processed.ExistsAsync(mid, ct)) return Result<bool>.Success(true);
  var found = await repository.GetByIdAsync(ordem, ct); if (!found.IsSuccess) return Result<bool>.Failure(found.Error);
  var order = found.Value!; var changed = order.RegistrarPagamento(orcamento, cobranca, valor, moeda, confirmado, detalhe);
  if (!changed.IsSuccess) return changed;
  if (changed.Value)
  {
   await repository.UpdateAsync(order, new HistoricoOrdem(Guid.NewGuid(), ordem, confirmado ? "PagamentoConfirmado" : "PagamentoRecusado", occurredAt.UtcDateTime, context.CorrelationId), ct);
   if (confirmado) await messaging.PublishAsync(OSMessaging.Topic, ordem.ToString("D"), context.Create(ExecucaoAutorizada.Type, new ExecucaoAutorizada(ordem), occurredAt), ct);
   else await messaging.PublishAsync(OSMessaging.Topic, ordem.ToString("D"), context.Create(CancelamentoSolicitado.Type, new CancelamentoSolicitado(ordem), occurredAt), ct);
  }
  if (context.IncomingMessageId is Guid id) await processed.AddAsync(id, ct);
  await tx.CommitAsync(ct); return Result<bool>.Success(true);
 }
}
