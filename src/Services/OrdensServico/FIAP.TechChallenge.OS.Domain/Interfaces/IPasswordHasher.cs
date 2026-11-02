namespace FIAP.TechChallenge.OS.Domain.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string plainPassword);
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}

