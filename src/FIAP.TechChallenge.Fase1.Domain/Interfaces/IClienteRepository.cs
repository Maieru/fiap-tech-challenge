using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IClienteRepository
{
    Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCnpjAsync(string cnpj, CancellationToken cancellationToken = default);
    Task<Result<Cliente>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<Cliente>> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<Result<Cliente>> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default);
    Task<Result<(IReadOnlyCollection<Cliente> Clientes, int TotalItems)>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task DeleteAsync(Cliente cliente, CancellationToken cancellationToken = default);
}
