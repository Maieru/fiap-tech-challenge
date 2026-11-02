using FIAP.TechChallenge.OS.Application.Abstractions;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Interfaces;
namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.EmitirAcessoCliente;
public sealed class EmitirAcessoClienteUseCase(IOrdemServicoRepository repository, IUnitOfWork unitOfWork, IAcessoClienteService acesso) : IEmitirAcessoClienteUseCase
{
    public async Task<Result<EmitirAcessoClienteResponse>> ExecuteAsync(EmitirAcessoClienteCommand command, CancellationToken cancellationToken = default)
    {
        if (command.OrdemServicoId == Guid.Empty) return Result<EmitirAcessoClienteResponse>.Failure(Error.InvalidId("Ordem"));
        await using var transaction = await unitOfWork.BeginAsync(cancellationToken);
        var found = await repository.GetByIdAsync(command.OrdemServicoId, cancellationToken);
        if (!found.IsSuccess) return Result<EmitirAcessoClienteResponse>.Failure(found.Error);
        var token = await acesso.EmitirAsync(command.OrdemServicoId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result<EmitirAcessoClienteResponse>.Success(new(command.OrdemServicoId, token, $"/api/os/v1/ordens/acompanhamento/{command.OrdemServicoId:D}?token={token}"));
    }
}
