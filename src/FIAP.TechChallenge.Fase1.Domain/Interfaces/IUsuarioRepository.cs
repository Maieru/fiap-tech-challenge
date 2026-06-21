using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<bool> ExistsByLoginAsync(string login, CancellationToken cancellationToken = default);
    Task<Result<Usuario>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task DeleteAsync(Usuario usuario, CancellationToken cancellationToken = default);
}

