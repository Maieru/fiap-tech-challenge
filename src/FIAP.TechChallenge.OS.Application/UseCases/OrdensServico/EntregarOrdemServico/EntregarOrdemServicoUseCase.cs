using FIAP.TechChallenge.OS.Application.Abstractions;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Interfaces;
namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.EntregarOrdemServico;
public sealed class EntregarOrdemServicoUseCase(IOrdemServicoRepository repository, IUnitOfWork unitOfWork, MessageContext context, TimeProvider clock) : IEntregarOrdemServicoUseCase
{
    public async Task<Result<EntregarOrdemServicoResponse>> ExecuteAsync(EntregarOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrdemServicoId == Guid.Empty) return Result<EntregarOrdemServicoResponse>.Failure(Error.InvalidId("Ordem"));
        await using var transaction = await unitOfWork.BeginAsync(cancellationToken);
        var result = await repository.GetByIdAsync(command.OrdemServicoId, cancellationToken);
        if (!result.IsSuccess) return Result<EntregarOrdemServicoResponse>.Failure(result.Error);
        var order = result.Value!; var now = clock.GetUtcNow().UtcDateTime;
        var changed = order.Entregar(now);
        if (!changed.IsSuccess) return Result<EntregarOrdemServicoResponse>.Failure(changed.Error);
        if (changed.Value) await repository.UpdateAsync(order, new HistoricoOrdem(Guid.NewGuid(), order.Id, "OrdemEntregue", now, context.CorrelationId), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result<EntregarOrdemServicoResponse>.Success(new(order.Id, order.Status, order.DataEntrega!.Value));
    }
}
