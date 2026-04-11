using FIAP.TechChallenge.Fase1.Domain.Abstractions;
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

    public async Task<Result<Veiculo>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var veiculo = await context.Veiculos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (veiculo == null)
            return Result<Veiculo>.Failure(new Error("Veículo não encontrado."));

        return VeiculoMapper.ToDomain(veiculo);
    }

    public async Task AddAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        var veiculoEntity = VeiculoMapper.ToEntity(veiculo);
        _ = await context.Veiculos.AddAsync(veiculoEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Veiculo veiculo, CancellationToken cancellationToken = default)
    {
        var veiculoEntity = VeiculoMapper.ToEntity(veiculo);
        _ = context.Veiculos.Update(veiculoEntity);
        _ = await context.SaveChangesAsync(cancellationToken);
    }
}
