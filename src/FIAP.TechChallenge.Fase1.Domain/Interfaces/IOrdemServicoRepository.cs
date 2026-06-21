using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IOrdemServicoRepository
{
    Task<Result<OrdemServico>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<(IReadOnlyCollection<OrdemServico> OrdensServico, int TotalItems)>> GetPagedAsync(
        Guid? clienteId,
        Guid? veiculoId,
        IReadOnlyCollection<StatusOrdemServico> status,
        SortDirection? statusSortDirection,
        SortDirection? dataAberturaSortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
    Task UpdateAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
    Task DeleteAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
}
