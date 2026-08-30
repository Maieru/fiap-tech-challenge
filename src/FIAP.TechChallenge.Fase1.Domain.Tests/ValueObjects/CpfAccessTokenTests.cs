using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Domain.Tests.ValueObjects;

[TestFixture]
public sealed class CpfAccessTokenTests
{
    [Test]
    public void Create_ShouldReturnExpectedSha256Token()
    {
        var cpf = Cpf.Create("529.982.247-25").Value!;
        var codigoAprovacao = Guid.Parse("79ee2120-ab05-4cba-a57d-011d654248dd");

        var token = CpfAccessToken.Create(cpf, codigoAprovacao);

        Assert.That(token, Is.EqualTo("d80e8c958d265c7455badf562f4cbd7914a38bda2698edb1cff74357e9bbc2d6"));
    }
}
