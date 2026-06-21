using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class OrdemServicoRepository(AppDbContext context) : IOrdemServicoRepository
{
    public async Task<Result<OrdemServico>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ordemServico = await context.OrdensServico.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ordemServico == null)
            return Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.", ErrorCode.NotFound));

        return OrdemServicoMapper.ToDomain(ordemServico);
    }

    public async Task<Result<(IReadOnlyCollection<OrdemServico> OrdensServico, int TotalItems)>> GetPagedAsync(
        Guid? clienteId,
        Guid? veiculoId,
        IReadOnlyCollection<StatusOrdemServico> status,
        SortDirection? statusSortDirection,
        SortDirection? dataAberturaSortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.OrdensServico.AsNoTracking();

        if (clienteId.HasValue)
            query = query.Where(x => x.ClienteId == clienteId.Value);

        if (veiculoId.HasValue)
            query = query.Where(x => x.VeiculoId == veiculoId.Value);

        if (status.Count > 0)
            query = query.Where(x => status.Contains(x.Status));

        var totalItems = await query.CountAsync(cancellationToken);

        var orderedQuery = ApplyOrdering(query, statusSortDirection, dataAberturaSortDirection);

        var ordensServicoEntity = await orderedQuery
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var ordensServicoResult = MapToDomainCollection(ordensServicoEntity);

        if (!ordensServicoResult.IsSuccess || ordensServicoResult.Value is null)
            return Result<(IReadOnlyCollection<OrdemServico> OrdensServico, int TotalItems)>.Failure(ordensServicoResult.Error);

        return Result<(IReadOnlyCollection<OrdemServico> OrdensServico, int TotalItems)>.Success((ordensServicoResult.Value, totalItems));
    }

    private static IOrderedQueryable<OrdemServicoEntity> ApplyOrdering(
        IQueryable<OrdemServicoEntity> query,
        SortDirection? statusSortDirection,
        SortDirection? dataAberturaSortDirection)
    {
        IOrderedQueryable<OrdemServicoEntity>? orderedQuery = null;

        if (statusSortDirection.HasValue)
        {
            orderedQuery = statusSortDirection.Value == SortDirection.Asc
                ? query.OrderBy(x => x.Status)
                : query.OrderByDescending(x => x.Status);
        }

        if (dataAberturaSortDirection.HasValue)
        {
            orderedQuery = orderedQuery is null
                ? OrderByDataAbertura(query, dataAberturaSortDirection.Value)
                : ThenByDataAbertura(orderedQuery, dataAberturaSortDirection.Value);
        }

        return orderedQuery ?? query.OrderByDescending(x => x.DataCriacao);
    }

    private static IOrderedQueryable<OrdemServicoEntity> OrderByDataAbertura(IQueryable<OrdemServicoEntity> query, SortDirection sortDirection)
    {
        return sortDirection == SortDirection.Asc
            ? query.OrderBy(x => x.DataCriacao)
            : query.OrderByDescending(x => x.DataCriacao);
    }

    private static IOrderedQueryable<OrdemServicoEntity> ThenByDataAbertura(IOrderedQueryable<OrdemServicoEntity> query, SortDirection sortDirection)
    {
        return sortDirection == SortDirection.Asc
            ? query.ThenBy(x => x.DataCriacao)
            : query.ThenByDescending(x => x.DataCriacao);
    }

    public async Task AddAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        var ordemServicoEntity = OrdemServicoMapper.ToEntity(ordemServico);
        _ = await context.OrdensServico.AddAsync(ordemServicoEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        var ordemServicoEntity = OrdemServicoMapper.ToEntity(ordemServico);
        _ = context.OrdensServico.Update(ordemServicoEntity);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        var ordemServicoEntity = await context.OrdensServico.FirstOrDefaultAsync(x => x.Id == ordemServico.Id, cancellationToken);

        if (ordemServicoEntity is null)
            return;

        ordemServicoEntity.Ativo = false;
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    private static Result<List<OrdemServico>> MapToDomainCollection(List<OrdemServicoEntity> ordensServicoEntity)
    {
        var ordensServico = new List<OrdemServico>(ordensServicoEntity.Count);

        foreach (var entity in ordensServicoEntity)
        {
            var ordemServicoResult = OrdemServicoMapper.ToDomain(entity);

            if (!ordemServicoResult.IsSuccess || ordemServicoResult.Value is null)
                return Result<List<OrdemServico>>.Failure(ordemServicoResult.Error);

            ordensServico.Add(ordemServicoResult.Value);
        }

        return Result<List<OrdemServico>>.Success(ordensServico);
    }
}

