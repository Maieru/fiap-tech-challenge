using FIAP.TechChallenge.OS.Application.Abstractions;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;
using FIAP.TechChallenge.OS.Domain.Interfaces;
using Maieru.FiapTechChallenge.Chassis;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.AutorizarExecucao;

public sealed class AutorizarExecucaoUseCase(IOrdemServicoRepository repository, IUnitOfWork unitOfWork,
    IMessagingService messaging, MessageContext context, TimeProvider clock, IAcessoClienteService acessoCliente) : IAutorizarExecucaoUseCase
{
    public async Task<Result<DecisaoExecucaoResponse>> ExecuteAsync(AutorizarExecucaoCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrdemServicoId == Guid.Empty)
            return Result<DecisaoExecucaoResponse>.Failure(Error.InvalidId("Ordem"));

        await using var transaction = await unitOfWork.BeginAsync(cancellationToken);
        var result = await repository.GetByIdAsync(command.OrdemServicoId, cancellationToken);

        if (!result.IsSuccess)
            return Result<DecisaoExecucaoResponse>.Failure(command.TokenCliente is null ? result.Error : Error.Unauthorized("Acesso à ordem inválido."));
        if (command.TokenCliente is not null && !await acessoCliente.ValidarAsync(command.OrdemServicoId, command.TokenCliente, cancellationToken))
            return Result<DecisaoExecucaoResponse>.Failure(Error.Unauthorized("Acesso à ordem inválido."));

        var order = result.Value!;
        var now = clock.GetUtcNow();
        var changed = order.DecidirExecucao(DecisaoExecucao.Autorizada, now.UtcDateTime);

        if (!changed.IsSuccess)
            return Result<DecisaoExecucaoResponse>.Failure(changed.Error);

        if (changed.Value)
        {
            await repository.UpdateAsync(order, new HistoricoOrdem(Guid.NewGuid(), order.Id, "PagamentoSolicitado", now.UtcDateTime, context.CorrelationId), cancellationToken);
            await messaging.PublishAsync(OSMessaging.BillingTopic, order.Id.ToString("D"),
                context.Create(PagamentoSolicitado.Type, new PagamentoSolicitado(order.Id, order.ClienteId, order.OrcamentoId!.Value, order.ValorOrcamento!.Value, "BRL"), now), cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return Result<DecisaoExecucaoResponse>.Success(DecisaoExecucaoResponse.From(order));
    }
}
