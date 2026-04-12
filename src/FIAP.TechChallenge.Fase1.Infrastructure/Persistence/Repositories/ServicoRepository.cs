using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class ServicoRepository(AppDbContext context) : IServicoRepository
{
    public async Task<Result<Servico>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var servicoEntity = await context.Servicos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (servicoEntity == null)
            return Result<Servico>.Failure(new Error("Servico nao encontrado."));

        return ServicoMapper.ToDomain(servicoEntity);
    }

    public async Task AddAsync(Servico servico, CancellationToken cancellationToken = default)
    {
        var servicoEntity = ServicoMapper.ToEntity(servico);
        _ = await context.Servicos.AddAsync(servicoEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Servico servico, CancellationToken cancellationToken = default)
    {
        var servicoEntity = ServicoMapper.ToEntity(servico);
        _ = context.Servicos.Update(servicoEntity);
        _ = await context.SaveChangesAsync(cancellationToken);
    }
}
