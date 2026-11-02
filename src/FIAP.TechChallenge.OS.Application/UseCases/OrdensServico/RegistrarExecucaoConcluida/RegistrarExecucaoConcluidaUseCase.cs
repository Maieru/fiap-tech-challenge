using FIAP.TechChallenge.OS.Application.Abstractions;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Interfaces;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.RegistrarExecucaoConcluida;

public sealed class RegistrarExecucaoConcluidaUseCase(IOrdemServicoRepository repository, IUnitOfWork unitOfWork,
    IProcessedMessageRepository processed, MessageContext context) : IRegistrarExecucaoConcluidaUseCase
{
    public async Task<Result<bool>> ExecuteAsync(RegistrarExecucaoConcluidaCommand command, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginAsync(cancellationToken);

        if (context.IncomingMessageId is Guid messageId && await processed.ExistsAsync(messageId, cancellationToken))
            return Result<bool>.Success(true);

        var result = await repository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!result.IsSuccess)
            return Result<bool>.Failure(result.Error);

        var order = result.Value!;
        var occurredAt = command.OcorridoEmUtc.UtcDateTime;
        var changed = order.RegistrarExecucaoConcluida(occurredAt);

        if (!changed.IsSuccess)
            return Result<bool>.Failure(changed.Error);

        if (changed.Value)
            await repository.UpdateAsync(order, new HistoricoOrdem(Guid.NewGuid(), order.Id, "ExecucaoConcluida", occurredAt, context.CorrelationId), cancellationToken);

        if (context.IncomingMessageId is Guid incoming)
            await processed.AddAsync(incoming, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
