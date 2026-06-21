using FIAP.TechChallenge.Fase1.Infrastructure.Security;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Security;

[TestFixture]
internal sealed class BCryptPasswordHasherTests
{
    [Test]
    public void HashPassword_ShouldGenerateBCryptHash()
    {
        var hasher = new BCryptPasswordHasher();

        var hash = hasher.HashPassword("SenhaForte@123");

        Assert.Multiple(() =>
        {
            Assert.That(hash, Is.Not.Null.And.Not.Empty);
            Assert.That(hash, Is.Not.EqualTo("SenhaForte@123"));
            Assert.That(hash, Does.StartWith("$2"));
        });
    }

    [Test]
    public void VerifyHashedPassword_ShouldReturnTrue_WhenPasswordMatches()
    {
        var hasher = new BCryptPasswordHasher();
        var plainPassword = "SenhaForte@123";
        var hash = hasher.HashPassword(plainPassword);

        var isValid = hasher.VerifyHashedPassword(hash, plainPassword);

        Assert.That(isValid, Is.True);
    }

    [Test]
    public void VerifyHashedPassword_ShouldReturnFalse_WhenPasswordDoesNotMatch()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = hasher.HashPassword("SenhaForte@123");

        var isValid = hasher.VerifyHashedPassword(hash, "SenhaErrada@123");

        Assert.That(isValid, Is.False);
    }
}

