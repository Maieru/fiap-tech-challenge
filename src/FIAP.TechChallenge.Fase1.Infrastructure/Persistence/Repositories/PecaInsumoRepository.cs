using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class PecaInsumoRepository(AppDbContext context) : IPecaInsumoRepository
{
    public async Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await context.PecasInsumos.AnyAsync(x => x.Codigo == codigo, cancellationToken);
    }

    public async Task<Result<PecaInsumo>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pecaInsumo = await context.PecasInsumos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (pecaInsumo == null)
            return Result<PecaInsumo>.Failure(new Error("Peca ou insumo nao encontrado."));

        return PecaInsumoMapper.ToDomain(pecaInsumo);
    }

    public async Task AddAsync(PecaInsumo pecaInsumo, CancellationToken cancellationToken = default)
    {
        var pecaInsumoEntity = PecaInsumoMapper.ToEntity(pecaInsumo);
        _ = await context.PecasInsumos.AddAsync(pecaInsumoEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PecaInsumo pecaInsumo, CancellationToken cancellationToken = default)
    {
        var pecaInsumoEntity = PecaInsumoMapper.ToEntity(pecaInsumo);
        _ = context.PecasInsumos.Update(pecaInsumoEntity);
        _ = await context.SaveChangesAsync(cancellationToken);
    }
}
