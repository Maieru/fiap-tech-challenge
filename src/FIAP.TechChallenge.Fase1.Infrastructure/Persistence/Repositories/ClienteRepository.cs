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
        var cliente = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (cliente == null)
            return Result<Cliente>.Failure(new Error("Cliente não encontrado.", ErrorCode.NotFound));

        return ClienteMapper.ToDomain(cliente);
    }

    public async Task<Result<Cliente>> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cliente = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Cpf == cpf, cancellationToken);

        if (cliente == null)
            return Result<Cliente>.Failure(new Error("Cliente não encontrado.", ErrorCode.NotFound));

        return ClienteMapper.ToDomain(cliente);
    }

    public async Task<Result<Cliente>> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        var cliente = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Cnpj == cnpj, cancellationToken);

        if (cliente == null)
            return Result<Cliente>.Failure(new Error("Cliente não encontrado.", ErrorCode.NotFound));

        return ClienteMapper.ToDomain(cliente);
    }

    public async Task<Result<(IReadOnlyCollection<Cliente> Clientes, int TotalItems)>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalItems = await context.Clientes.CountAsync(cancellationToken);

        var clientesEntity = await context.Clientes
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var clientes = new List<Cliente>(clientesEntity.Count);

        foreach (var entity in clientesEntity)
        {
            var clienteResult = ClienteMapper.ToDomain(entity);

            if (!clienteResult.IsSuccess || clienteResult.Value is null)
                return Result<(IReadOnlyCollection<Cliente> Clientes, int TotalItems)>.Failure(clienteResult.Error);

            clientes.Add(clienteResult.Value);
        }

        return Result<(IReadOnlyCollection<Cliente> Clientes, int TotalItems)>.Success((clientes, totalItems));
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
        var clienteEntity = await context.Clientes.FirstOrDefaultAsync(x => x.Id == cliente.Id, cancellationToken);

        if (clienteEntity is null)
            return;

        clienteEntity.Ativo = false;
        _ = await context.SaveChangesAsync(cancellationToken);
    }
}

