using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IPecaInsumoRepository
{
    Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<Result<PecaInsumo>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(PecaInsumo pecaInsumo, CancellationToken cancellationToken = default);
}
