using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class VeiculoRepository(AppDbContext context) : IVeiculoRepository
{
    public async Task<bool> ExistsByPlacaAsync(string placa, CancellationToken cancellationToken = default)
    {
        return await context.Veiculos.AnyAsync(x => x.Placa == placa, cancellationToken);
    }

    public async Task AddAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        var veiculoEntity = VeiculoMapper.ToEntity(veiculo);
        _ = await context.Veiculos.AddAsync(veiculoEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }
}
