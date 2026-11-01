using FIAP.TechChallenge.OS.Application.Abstractions;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;
using FIAP.TechChallenge.OS.Domain.Interfaces;
using Maieru.FiapTechChallenge.Chassis;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.AutorizarExecucao;

public sealed class AutorizarExecucaoUseCase(IOrdemServicoRepository repository, IUnitOfWork unitOfWork,
    IMessagingService messaging, MessageContext context, TimeProvider clock) : IAutorizarExecucaoUseCase
{
    public async Task<Result<DecisaoExecucaoResponse>> ExecuteAsync(AutorizarExecucaoCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrdemServicoId == Guid.Empty)
            return Result<DecisaoExecucaoResponse>.Failure(Error.InvalidId("Ordem"));

        await using var transaction = await unitOfWork.BeginAsync(cancellationToken);
        var result = await repository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!result.IsSuccess)
            return Result<DecisaoExecucaoResponse>.Failure(result.Error);

        var order = result.Value!;
        var now = clock.GetUtcNow();
        var changed = order.DecidirExecucao(DecisaoExecucao.Autorizada, now.UtcDateTime);

        if (!changed.IsSuccess)
            return Result<DecisaoExecucaoResponse>.Failure(changed.Error);

        if (changed.Value)
        {
            await repository.UpdateAsync(order, new HistoricoOrdem(Guid.NewGuid(), order.Id, "ExecucaoAutorizada", now.UtcDateTime, context.CorrelationId), cancellationToken);
            await messaging.PublishAsync(OSMessaging.Topic, order.Id.ToString("D"),
                context.Create(ExecucaoAutorizada.Type, new ExecucaoAutorizada(order.Id), now), cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return Result<DecisaoExecucaoResponse>.Success(DecisaoExecucaoResponse.From(order));
    }
}
