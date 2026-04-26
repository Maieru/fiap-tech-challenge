using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
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
            return Result<PecaInsumo>.Failure(new Error("Peca ou insumo nao encontrado.", ErrorCode.NotFound));

        return PecaInsumoMapper.ToDomain(pecaInsumo);
    }

    public async Task<Result<PecaInsumo>> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        var pecaInsumo = await context.PecasInsumos.AsNoTracking().FirstOrDefaultAsync(x => x.Codigo == codigo, cancellationToken);

        if (pecaInsumo == null)
            return Result<PecaInsumo>.Failure(new Error("Peca ou insumo nao encontrado.", ErrorCode.NotFound));

        return PecaInsumoMapper.ToDomain(pecaInsumo);
    }

    public async Task<Result<(IReadOnlyCollection<PecaInsumo> PecasInsumos, int TotalItems)>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalItems = await context.PecasInsumos.CountAsync(cancellationToken);

        var pecasInsumosEntity = await context.PecasInsumos
            .AsNoTracking()
            .OrderBy(x => x.Codigo)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pecasInsumosResult = MapToDomainCollection(pecasInsumosEntity);

        if (!pecasInsumosResult.IsSuccess || pecasInsumosResult.Value is null)
            return Result<(IReadOnlyCollection<PecaInsumo> PecasInsumos, int TotalItems)>.Failure(pecasInsumosResult.Error);

        return Result<(IReadOnlyCollection<PecaInsumo> PecasInsumos, int TotalItems)>.Success((pecasInsumosResult.Value, totalItems));
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

    public async Task DeleteAsync(PecaInsumo pecaInsumo, CancellationToken cancellationToken = default)
    {
        var pecaInsumoEntity = await context.PecasInsumos.FirstOrDefaultAsync(x => x.Id == pecaInsumo.Id, cancellationToken);

        if (pecaInsumoEntity is null)
            return;

        pecaInsumoEntity.Ativo = false;
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    private static Result<List<PecaInsumo>> MapToDomainCollection(List<PecaInsumoEntity> pecasInsumosEntity)
    {
        var pecasInsumos = new List<PecaInsumo>(pecasInsumosEntity.Count);

        foreach (var entity in pecasInsumosEntity)
        {
            var pecaInsumoResult = PecaInsumoMapper.ToDomain(entity);

            if (!pecaInsumoResult.IsSuccess || pecaInsumoResult.Value is null)
                return Result<List<PecaInsumo>>.Failure(pecaInsumoResult.Error);

            pecasInsumos.Add(pecaInsumoResult.Value);
        }

        return Result<List<PecaInsumo>>.Success(pecasInsumos);
    }
}
