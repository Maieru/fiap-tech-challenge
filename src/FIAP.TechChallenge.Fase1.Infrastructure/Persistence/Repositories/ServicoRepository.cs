using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class ServicoRepository(AppDbContext context) : IServicoRepository
{
    public async Task AddAsync(Servico servico, CancellationToken cancellationToken = default)
    {
        var servicoEntity = ServicoMapper.ToEntity(servico);
        _ = await context.Servicos.AddAsync(servicoEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }
}
