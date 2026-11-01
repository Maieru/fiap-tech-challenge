using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;

namespace FIAP.TechChallenge.OS.Domain.Interfaces;

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
    Task UpdateAsync(OrdemServico ordemServico, HistoricoOrdem historico, CancellationToken cancellationToken = default);
}
