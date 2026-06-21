using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Domain.Tests.ValueObjects;

[TestFixture]
internal class CPFTests
{
    [Test]
    public void Create_ShouldFail_WhenInputIsNull()
    {
        var result = Cpf.Create(null);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O CPF deve ser informado."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenInputIsWhitespace()
    {
        var result = Cpf.Create("   ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O CPF deve ser informado."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenLengthIsNotEleven()
    {
        var result = Cpf.Create("1234567890");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O CPF precisa ter exatamente 11 dígitos."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenAllDigitsAreEqual()
    {
        var result = Cpf.Create("11111111111");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O CPF informado é inválido."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenFirstVerificationDigitDoesNotMatch()
    {
        var result = Cpf.Create("52998224735");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O CPF informado é inválido: o primeiro dígito verificador não confere."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenSecondVerificationDigitDoesNotMatch()
    {
        var result = Cpf.Create("52998224724");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O CPF informado é inválido: o segundo dígito verificador não confere."));
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValid()
    {
        var result = Cpf.Create("529.982.247-25");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Unformatted, Is.EqualTo("52998224725"));
            Assert.That(result.Value.Formatted, Is.EqualTo("529.982.247-25"));
            Assert.That(result.Value.ToString(), Is.EqualTo("529.982.247-25"));
        });
    }

    [Test]
    public void Unformatted_ShouldReturnNormalizedValue()
    {
        var result = Cpf.Create("529.982.247-25");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Unformatted, Is.EqualTo("52998224725"));
        });
    }

    [Test]
    public void Formatted_ShouldReturnExpectedMask()
    {
        var result = Cpf.Create("52998224725");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Formatted, Is.EqualTo("529.982.247-25"));
        });
    }

    [Test]
    public void ToString_ShouldReturnFormattedValue()
    {
        var result = Cpf.Create("52998224725");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.ToString(), Is.EqualTo(result.Value.Formatted));
        });
    }

    [Test]
    public void Equals_ShouldReturnTrue_ForSameNormalizedValue()
    {
        var left = Cpf.Create("529.982.247-25").Value;
        var right = Cpf.Create("52998224725").Value;

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
        var left = Cpf.Create("52998224725").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(left!, Is.Not.Null);
        });
    }

    [Test]
    public void ObjectEquals_ShouldReturnFalse_WhenObjectIsDifferentType()
    {
        var left = Cpf.Create("52998224725").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(left!, Is.Not.EqualTo(new object()));
        });
    }

    [Test]
    public void GetHashCode_ShouldBeEqual_ForEqualObjects()
    {
        var left = Cpf.Create("529.982.247-25").Value;
        var right = Cpf.Create("52998224725").Value;

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
        var left = Cpf.Create("529.982.247-25").Value;
        var right = Cpf.Create("52998224725").Value;

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
        Cpf? left = null;
        Cpf? right = null;

        Assert.That(left, Is.EqualTo(right));
    }

    [Test]
    public void InequalityOperator_ShouldReturnTrue_ForDifferentObjects()
    {
        var left = Cpf.Create("52998224725").Value;
        var right = Cpf.Create("12345678909").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(right, Is.Not.Null);
            Assert.That(left, Is.Not.EqualTo(right));
        });
    }
}

