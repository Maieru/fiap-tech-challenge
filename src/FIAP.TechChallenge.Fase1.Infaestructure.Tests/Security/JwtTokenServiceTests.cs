using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Security;

[TestFixture]
internal sealed class JwtTokenServiceTests
{
    [Test]
    public void GenerateToken_ShouldReturnSignedJwt()
    {
        var usuarioResult = Usuario.Create("admin", "hash");
        Assert.That(usuarioResult.IsSuccess, Is.True);
        var usuario = usuarioResult.Value!;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("12345678901234567890123456789012"));
        var service = new JwtTokenService("issuer-test", "audience-test", key, 60);

        var token = service.GenerateToken(usuario);

        Assert.That(token, Is.Not.Null.And.Not.Empty);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Multiple(() =>
        {
            Assert.That(jwt.Issuer, Is.EqualTo("issuer-test"));
            Assert.That(jwt.Audiences.Single(), Is.EqualTo("audience-test"));
            Assert.That(jwt.Claims.Any(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == usuario.Id.ToString()), Is.True);
            Assert.That(jwt.Claims.Any(c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == "admin"), Is.True);
            Assert.That(service.AccessTokenLifetimeSeconds, Is.EqualTo(3600));
        });
    }
}

