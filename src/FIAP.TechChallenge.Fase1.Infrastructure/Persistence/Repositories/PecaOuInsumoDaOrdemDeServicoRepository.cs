using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class PecaOuInsumoDaOrdemDeServicoRepository(AppDbContext context) : IPecaOuInsumoDaOrdemDeServicoRepository
{
    public async Task AddAsync(PecaOuInsumoDaOrdemDeServico pecaOuInsumoDaOrdemDeServico, CancellationToken cancellationToken = default)
    {
        var pecaOuInsumoDaOrdemDeServicoEntity = PecaOuInsumoDaOrdemDeServicoMapper.ToEntity(pecaOuInsumoDaOrdemDeServico);
        _ = await context.PecaOuInsumoDaOrdemDeServico.AddAsync(pecaOuInsumoDaOrdemDeServicoEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }
}
