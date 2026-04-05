using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Domain.Tests.ValueObjects;

[TestFixture]
internal class TelefoneTests
{
    [Test]
    public void Create_ShouldFail_WhenInputIsNull()
    {
        var result = Telefone.Create(null);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O telefone deve ser informado."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenInputIsWhitespace()
    {
        var result = Telefone.Create("   ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O telefone deve ser informado."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenInputHasNoDigits()
    {
        var result = Telefone.Create("(xx) xxxx-xxxx");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O telefone deve ser informado."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenLengthIsInvalid()
    {
        var result = Telefone.Create("119876543");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O telefone deve ter 10 dígitos para fixo ou 11 dígitos para móvel."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenDddIsInvalid()
    {
        var result = Telefone.Create("01987654321");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O telefone informado possui um DDD inválido."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenMobileNumberDoesNotStartWithNineAfterDdd()
    {
        var result = Telefone.Create("11887654321");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O telefone móvel informado é inválido: ele deve começar com 9 após o DDD."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenLandlineNumberStartsWithNineAfterDdd()
    {
        var result = Telefone.Create("1198765432");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O telefone fixo informado é inválido."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenAllDigitsAreEqual_ForMobile()
    {
        var result = Telefone.Create("99999999999");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O telefone informado é inválido."));
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValidMobile()
    {
        var result = Telefone.Create("(11) 98765-4321");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Unformatted, Is.EqualTo("11987654321"));
            Assert.That(result.Value.Formatted, Is.EqualTo("(11) 98765-4321"));
            Assert.That(result.Value.Tipo, Is.EqualTo(TipoTelefone.Movel));
            Assert.That(result.Value.IsMobile, Is.True);
            Assert.That(result.Value.IsLandline, Is.False);
            Assert.That(result.Value.ToString(), Is.EqualTo("(11) 98765-4321"));
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValidLandline()
    {
        var result = Telefone.Create("11 3265-4321");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Unformatted, Is.EqualTo("1132654321"));
            Assert.That(result.Value.Formatted, Is.EqualTo("(11) 3265-4321"));
            Assert.That(result.Value.Tipo, Is.EqualTo(TipoTelefone.Fixo));
            Assert.That(result.Value.IsMobile, Is.False);
            Assert.That(result.Value.IsLandline, Is.True);
        });
    }

    [Test]
    public void Equals_ShouldReturnTrue_ForSameNormalizedValue()
    {
        var left = Telefone.Create("(11) 98765-4321").Value;
        var right = Telefone.Create("11987654321").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(right, Is.Not.Null);
            Assert.That(left!, Is.EqualTo(right));
        });
    }

    [Test]
    public void Equals_ShouldReturnFalse_WhenOtherIsNull()
    {
        var left = Telefone.Create("11987654321").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(left!, Is.Not.Null);
        });
    }

    [Test]
    public void ObjectEquals_ShouldReturnFalse_WhenObjectIsDifferentType()
    {
        var left = Telefone.Create("11987654321").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(left!, Is.Not.EqualTo(new object()));
        });
    }

    [Test]
    public void GetHashCode_ShouldBeEqual_ForEqualObjects()
    {
        var left = Telefone.Create("(11) 98765-4321").Value;
        var right = Telefone.Create("11987654321").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(right, Is.Not.Null);
            Assert.That(left!.GetHashCode(), Is.EqualTo(right!.GetHashCode()));
        });
    }

    [Test]
    public void EqualityOperator_ShouldReturnTrue_ForEqualObjects()
    {
        var left = Telefone.Create("(11) 98765-4321").Value;
        var right = Telefone.Create("11987654321").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(right, Is.Not.Null);
            Assert.That(left, Is.EqualTo(right));
        });
    }

    [Test]
    public void EqualityOperator_ShouldReturnTrue_WhenBothAreNull()
    {
        Telefone? left = null;
        Telefone? right = null;

        Assert.That(left, Is.EqualTo(right));
    }

    [Test]
    public void InequalityOperator_ShouldReturnTrue_ForDifferentObjects()
    {
        var left = Telefone.Create("11987654321").Value;
        var right = Telefone.Create("1132654321").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(right, Is.Not.Null);
            Assert.That(left, Is.Not.EqualTo(right));
        });
    }
}