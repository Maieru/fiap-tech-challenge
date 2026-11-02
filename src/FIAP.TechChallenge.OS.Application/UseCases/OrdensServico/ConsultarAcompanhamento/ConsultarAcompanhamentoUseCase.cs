using FIAP.TechChallenge.OS.Application.Abstractions;
using FIAP.TechChallenge.OS.Application.Messaging;
using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Interfaces;
namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarAcompanhamento;
public sealed class ConsultarAcompanhamentoUseCase(IOrdemServicoRepository repository, IUnitOfWork unitOfWork, IAcessoClienteService acesso) : IConsultarAcompanhamentoUseCase
{
    public async Task<Result<AcompanhamentoResponse>> ExecuteAsync(ConsultarAcompanhamentoCommand command, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginAsync(cancellationToken);
        var found = await repository.GetByIdAsync(command.OrdemServicoId, cancellationToken);
        if (!found.IsSuccess || !await acesso.ValidarAsync(command.OrdemServicoId, command.Token, cancellationToken))
            return Result<AcompanhamentoResponse>.Failure(Error.Unauthorized("Acesso à ordem inválido."));
        var response = AcompanhamentoResponse.From(found.Value!);
        await transaction.CommitAsync(cancellationToken);
        return Result<AcompanhamentoResponse>.Success(response);
    }
}
