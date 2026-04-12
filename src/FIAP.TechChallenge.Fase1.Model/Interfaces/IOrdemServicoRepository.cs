using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IOrdemServicoRepository
{
    Task<Result<OrdemServico>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
}
