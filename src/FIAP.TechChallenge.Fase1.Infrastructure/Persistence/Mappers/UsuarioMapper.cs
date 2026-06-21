using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

public static class UsuarioMapper
{
    public static UsuarioEntity ToEntity(Usuario usuario)
    {
        return new UsuarioEntity
        {
            Id = usuario.Id,
            Login = usuario.Login,
            Senha = usuario.Senha
        };
    }

    public static Result<Usuario> ToDomain(UsuarioEntity entity)
    {
        return Usuario.Rehydrate(entity.Id, entity.Login, entity.Senha);
    }
}

