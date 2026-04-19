using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class ServicoDaOrdemDeServicoRepository(AppDbContext context) : IServicoDaOrdemDeServicoRepository
{
    public async Task<Result<ServicoDaOrdemDeServico>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var servicoDaOrdemEntity = await context.ServicoDaOrdemDeServico
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (servicoDaOrdemEntity is null)
            return Result<ServicoDaOrdemDeServico>.Failure(new Error("Serviço da ordem de serviço não encontrado.", ErrorCode.NotFound));

        return ServicoDaOrdemDeServicoMapper.ToDomain(servicoDaOrdemEntity);
    }

    public async Task<Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>> GetByOrdemServicoIdAsync(Guid ordemServicoId, CancellationToken cancellationToken = default)
    {
        var servicosDaOrdemEntity = await context.ServicoDaOrdemDeServico
            .AsNoTracking()
            .Where(x => x.OrdemServicoId == ordemServicoId)
            .OrderBy(x => x.Descricao)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var servicosDaOrdemResult = MapToDomainCollection(servicosDaOrdemEntity);

        if (!servicosDaOrdemResult.IsSuccess || servicosDaOrdemResult.Value is null)
            return Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Failure(servicosDaOrdemResult.Error);

        return Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Success(servicosDaOrdemResult.Value);
    }

    public async Task AddAsync(ServicoDaOrdemDeServico servicoDaOrdemDeServico, CancellationToken cancellationToken = default)
    {
        var servicoDaOrdemEntity = ServicoDaOrdemDeServicoMapper.ToEntity(servicoDaOrdemDeServico);
        _ = await context.ServicoDaOrdemDeServico.AddAsync(servicoDaOrdemEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ServicoDaOrdemDeServico servicoDaOrdemDeServico, CancellationToken cancellationToken = default)
    {
        var servicoDaOrdemEntity = ServicoDaOrdemDeServicoMapper.ToEntity(servicoDaOrdemDeServico);
        _ = context.ServicoDaOrdemDeServico.Update(servicoDaOrdemEntity);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    private static Result<IReadOnlyCollection<ServicoDaOrdemDeServico>> MapToDomainCollection(List<ServicoDaOrdemDeServicoEntity> servicosDaOrdemEntity)
    {
        var servicosDaOrdem = new List<ServicoDaOrdemDeServico>(servicosDaOrdemEntity.Count);

        foreach (var entity in servicosDaOrdemEntity)
        {
            var servicoDaOrdemResult = ServicoDaOrdemDeServicoMapper.ToDomain(entity);

            if (!servicoDaOrdemResult.IsSuccess || servicoDaOrdemResult.Value is null)
                return Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Failure(servicoDaOrdemResult.Error);

            servicosDaOrdem.Add(servicoDaOrdemResult.Value);
        }

        return Result<IReadOnlyCollection<ServicoDaOrdemDeServico>>.Success(servicosDaOrdem);
    }
}
