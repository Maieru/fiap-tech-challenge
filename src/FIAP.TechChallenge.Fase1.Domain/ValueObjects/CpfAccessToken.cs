using System.Security.Cryptography;
using System.Text;

namespace FIAP.TechChallenge.Fase1.Domain.ValueObjects;

public static class CpfAccessToken
{
    private const int Sha256HexLength = 64;

    public static string Create(Cpf cpf, Guid codigoAprovacao)
    {
        var value = $"{cpf.Unformatted}{codigoAprovacao:D}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));

        return Convert.ToHexStringLower(hash);
    }

    public static bool Matches(Cpf cpf, Guid codigoAprovacao, string? token)
    {
        if (token is not { Length: Sha256HexLength } || !token.All(Uri.IsHexDigit))
            return false;

        var expected = Convert.FromHexString(Create(cpf, codigoAprovacao));
        var provided = Convert.FromHexString(token);

        return CryptographicOperations.FixedTimeEquals(expected, provided);
    }
}
