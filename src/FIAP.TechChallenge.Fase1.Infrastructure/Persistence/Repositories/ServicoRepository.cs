using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class ServicoRepository(AppDbContext context) : IServicoRepository
{
    public async Task<Result<Servico>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var servicoEntity = await context.Servicos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (servicoEntity == null)
            return Result<Servico>.Failure(new Error("Servico nao encontrado.", ErrorCode.NotFound));

        return ServicoMapper.ToDomain(servicoEntity);
    }

    public async Task AddAsync(Servico servico, CancellationToken cancellationToken = default)
    {
        var servicoEntity = ServicoMapper.ToEntity(servico);
        _ = await context.Servicos.AddAsync(servicoEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<(IReadOnlyCollection<Servico> Servicos, int TotalItems)>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalItems = await context.Servicos.CountAsync(cancellationToken);

        var servicosEntity = await context.Servicos
            .AsNoTracking()
            .OrderBy(x => x.Descricao)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var servicosResult = MapToDomainCollection(servicosEntity);

        if (!servicosResult.IsSuccess || servicosResult.Value is null)
            return Result<(IReadOnlyCollection<Servico> Servicos, int TotalItems)>.Failure(servicosResult.Error);

        return Result<(IReadOnlyCollection<Servico> Servicos, int TotalItems)>.Success((servicosResult.Value, totalItems));
    }

    public async Task UpdateAsync(Servico servico, CancellationToken cancellationToken = default)
    {
        var servicoEntity = ServicoMapper.ToEntity(servico);
        _ = context.Servicos.Update(servicoEntity);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Servico servico, CancellationToken cancellationToken = default)
    {
        var servicoEntity = await context.Servicos.FirstOrDefaultAsync(x => x.Id == servico.Id, cancellationToken);

        if (servicoEntity is null)
            return;

        servicoEntity.Ativo = false;
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    private static Result<List<Servico>> MapToDomainCollection(List<ServicoEntity> servicosEntity)
    {
        var servicos = new List<Servico>(servicosEntity.Count);

        foreach (var entity in servicosEntity)
        {
            var servicoResult = ServicoMapper.ToDomain(entity);

            if (!servicoResult.IsSuccess || servicoResult.Value is null)
                return Result<List<Servico>>.Failure(servicoResult.Error);

            servicos.Add(servicoResult.Value);
        }

        return Result<List<Servico>>.Success(servicos);
    }
}
