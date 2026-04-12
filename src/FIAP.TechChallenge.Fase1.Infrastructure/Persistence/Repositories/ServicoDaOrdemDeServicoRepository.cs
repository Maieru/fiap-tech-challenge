using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class ServicoDaOrdemDeServicoRepository(AppDbContext context) : IServicoDaOrdemDeServicoRepository
{
    public async Task AddAsync(ServicoDaOrdemDeServico servicoDaOrdemDeServico, CancellationToken cancellationToken = default)
    {
        var servicoDaOrdemEntity = ServicoDaOrdemDeServicoMapper.ToEntity(servicoDaOrdemDeServico);
        _ = await context.ServicoDaOrdemDeServico.AddAsync(servicoDaOrdemEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }
}
