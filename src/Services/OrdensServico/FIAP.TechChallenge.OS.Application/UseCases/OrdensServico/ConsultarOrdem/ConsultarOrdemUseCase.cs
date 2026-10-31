using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Interfaces;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarOrdem;

public interface IConsultarOrdemUseCase
{
    Task<Result<OrdemServico>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class ConsultarOrdemUseCase(IOrdemServicoRepository repository) : IConsultarOrdemUseCase
{
    public Task<Result<OrdemServico>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default) =>
        id == Guid.Empty
            ? Task.FromResult(Result<OrdemServico>.Failure(new Error("O identificador da ordem de servico deve ser valido.")))
            : repository.GetByIdAsync(id, cancellationToken);
}
