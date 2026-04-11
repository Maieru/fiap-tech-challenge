using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IVeiculoRepository
{
    Task<bool> ExistsByPlacaAsync(string placa, CancellationToken cancellationToken = default);
    Task<Result<Veiculo>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
}
