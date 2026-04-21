using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository(AppDbContext context) : IUsuarioRepository
{
    public async Task<bool> ExistsByLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        return await context.Usuarios.AnyAsync(x => x.Login == login, cancellationToken);
    }

    public async Task<Usuario?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        var usuarioEntity = await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Login == login, cancellationToken);

        if (usuarioEntity is null)
            return null;

        var usuarioResult = UsuarioMapper.ToDomain(usuarioEntity);
        return usuarioResult.IsSuccess ? usuarioResult.Value : null;
    }

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        var usuarioEntity = UsuarioMapper.ToEntity(usuario);
        _ = await context.Usuarios.AddAsync(usuarioEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);
    }
}
