using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IVeiculoRepository
{
    Task<bool> ExistsByPlacaAsync(string placa, CancellationToken cancellationToken = default);
    Task AddAsync(Veiculo veiculo, CancellationToken cancellationToken = default);
}
