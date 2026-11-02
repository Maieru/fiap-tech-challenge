using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;

namespace FIAP.TechChallenge.OS.Application.UseCases.OrdensServico.ConsultarOrdem;

public interface IConsultarOrdemUseCase
{
    Task<Result<OrdemServico>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}
