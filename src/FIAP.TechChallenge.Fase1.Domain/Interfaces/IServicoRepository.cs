using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IServicoRepository
{
    Task<Result<Servico>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<(IReadOnlyCollection<Servico> Servicos, int TotalItems)>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Servico servico, CancellationToken cancellationToken = default);
    Task UpdateAsync(Servico servico, CancellationToken cancellationToken = default);
    Task DeleteAsync(Servico servico, CancellationToken cancellationToken = default);
}
