using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Mappers;

public static class UsuarioMapper
{
    public static Result<Usuario> ToDomain(UsuarioEntity entity) => Usuario.Rehydrate(entity.Id, entity.Login, entity.Senha);
    public static UsuarioEntity ToEntity(Usuario usuario) => new() { Id = usuario.Id, Login = usuario.Login, Senha = usuario.Senha };
}
