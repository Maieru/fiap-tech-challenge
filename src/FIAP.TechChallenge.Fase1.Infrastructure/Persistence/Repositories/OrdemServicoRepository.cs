using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class OrdemServicoRepository(AppDbContext context) : IOrdemServicoRepository
{
    public async Task<Result<OrdemServico>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ordemServico = await context.OrdensServico.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ordemServico == null)
            return Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada."));

        return OrdemServicoMapper.ToDomain(ordemServico);
    }

    public async Task AddAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        var ordemServicoEntity = OrdemServicoMapper.ToEntity(ordemServico);
        _ = await context.OrdensServico.AddAsync(ordemServicoEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }
}
