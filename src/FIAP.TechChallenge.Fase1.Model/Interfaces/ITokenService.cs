using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Interfaces;

public interface ITokenService
{
    int AccessTokenLifetimeSeconds { get; }
    string GenerateToken(Usuario usuario);
}
