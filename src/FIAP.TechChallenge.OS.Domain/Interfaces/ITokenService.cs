using FIAP.TechChallenge.OS.Domain.Entities;

namespace FIAP.TechChallenge.OS.Domain.Interfaces;

public interface ITokenService
{
    int AccessTokenLifetimeSeconds { get; }
    string GenerateToken(Usuario usuario);
}

