using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class PecaOuInsumoDaOrdemDeServicoRepository(AppDbContext context) : IPecaOuInsumoDaOrdemDeServicoRepository
{
    public async Task<Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>>> GetByOrdemServicoIdAsync(Guid ordemServicoId, CancellationToken cancellationToken = default)
    {
        var pecasOuInsumosDaOrdemEntity = await context.PecaOuInsumoDaOrdemDeServico
            .AsNoTracking()
            .Where(x => x.OrdemServicoId == ordemServicoId)
            .OrderBy(x => x.Nome)
            .ThenBy(x => x.Codigo)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var pecasOuInsumosDaOrdemResult = MapToDomainCollection(pecasOuInsumosDaOrdemEntity);

        if (!pecasOuInsumosDaOrdemResult.IsSuccess || pecasOuInsumosDaOrdemResult.Value is null)
            return Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>>.Failure(pecasOuInsumosDaOrdemResult.Error);

        return Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>>.Success(pecasOuInsumosDaOrdemResult.Value);
    }

    public async Task AddAsync(PecaOuInsumoDaOrdemDeServico pecaOuInsumoDaOrdemDeServico, CancellationToken cancellationToken = default)
    {
        var pecaOuInsumoDaOrdemDeServicoEntity = PecaOuInsumoDaOrdemDeServicoMapper.ToEntity(pecaOuInsumoDaOrdemDeServico);
        _ = await context.PecaOuInsumoDaOrdemDeServico.AddAsync(pecaOuInsumoDaOrdemDeServicoEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    private static Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>> MapToDomainCollection(List<PecaOuInsumoDaOrdemDeServicoEntity> pecasOuInsumosDaOrdemEntity)
    {
        var pecasOuInsumosDaOrdem = new List<PecaOuInsumoDaOrdemDeServico>(pecasOuInsumosDaOrdemEntity.Count);

        foreach (var entity in pecasOuInsumosDaOrdemEntity)
        {
            var pecaOuInsumoDaOrdemResult = PecaOuInsumoDaOrdemDeServicoMapper.ToDomain(entity);

            if (!pecaOuInsumoDaOrdemResult.IsSuccess || pecaOuInsumoDaOrdemResult.Value is null)
                return Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>>.Failure(pecaOuInsumoDaOrdemResult.Error);

            pecasOuInsumosDaOrdem.Add(pecaOuInsumoDaOrdemResult.Value);
        }

        return Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>>.Success(pecasOuInsumosDaOrdem);
    }
}

