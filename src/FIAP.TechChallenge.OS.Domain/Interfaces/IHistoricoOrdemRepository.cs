using FIAP.TechChallenge.OS.Domain.Entities;

namespace FIAP.TechChallenge.OS.Domain.Interfaces;

public interface IHistoricoOrdemRepository
{
    Task<IReadOnlyCollection<HistoricoOrdem>> GetByOrdemAsync(Guid ordemServicoId, CancellationToken cancellationToken = default);
}
