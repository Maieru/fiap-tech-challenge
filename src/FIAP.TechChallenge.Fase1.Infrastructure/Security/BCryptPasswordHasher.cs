using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Infrastructure.Security;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
    }
}

