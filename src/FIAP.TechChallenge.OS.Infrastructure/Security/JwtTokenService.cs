using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FIAP.TechChallenge.OS.Infrastructure.Security;

public sealed class JwtTokenService(string issuer, string audience, SymmetricSecurityKey signingKey, int accessTokenMinutes, TimeProvider clock) : ITokenService

{
    private readonly string _issuer = issuer;
    private readonly string _audience = audience;
    private readonly SymmetricSecurityKey _signingKey = signingKey;
    private readonly int _accessTokenMinutes = accessTokenMinutes;

    public int AccessTokenLifetimeSeconds => _accessTokenMinutes * 60;

    public string GenerateToken(Usuario usuario)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, usuario.Login),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_accessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

