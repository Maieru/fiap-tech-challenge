using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class ClienteRepository(AppDbContext context) : IClienteRepository
{
    public async Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        return await context.Clientes.AnyAsync(x => x.Cpf != null && x.Cpf == cpf, cancellationToken);
    }

    public async Task<bool> ExistsByCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        return await context.Clientes.AnyAsync(x => x.Cnpj != null && x.Cnpj == cnpj, cancellationToken);
    }

    public async Task<Result<Cliente>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await context.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (cliente == null)
            return Result<Cliente>.Failure(new Error("Cliente não encontrado."));

        return ClienteMapper.ToDomain(cliente);
    }

    public async Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        var clienteEntity = ClienteMapper.ToEntity(cliente);
        _ = await context.Clientes.AddAsync(clienteEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        var clienteEntity = ClienteMapper.ToEntity(cliente);
        _ = context.Clientes.Update(clienteEntity);
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        var clienteEntity = ClienteMapper.ToEntity(cliente);

        _ = context.Clientes.Remove(clienteEntity);
        _ = await context.SaveChangesAsync(cancellationToken);
    }
}