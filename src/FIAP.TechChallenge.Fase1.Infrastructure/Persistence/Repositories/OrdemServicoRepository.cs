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
        StatusOrdemServico? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.OrdensServico.AsNoTracking();

        if (clienteId.HasValue)
            query = query.Where(x => x.ClienteId == clienteId.Value);

        if (veiculoId.HasValue)
            query = query.Where(x => x.VeiculoId == veiculoId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var totalItems = await query.CountAsync(cancellationToken);

        var ordensServicoEntity = await query
            .OrderByDescending(x => x.DataCriacao)
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var ordensServicoResult = MapToDomainCollection(ordensServicoEntity);

        if (!ordensServicoResult.IsSuccess || ordensServicoResult.Value is null)
            return Result<(IReadOnlyCollection<OrdemServico> OrdensServico, int TotalItems)>.Failure(ordensServicoResult.Error);

        return Result<(IReadOnlyCollection<OrdemServico> OrdensServico, int TotalItems)>.Success((ordensServicoResult.Value, totalItems));
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
