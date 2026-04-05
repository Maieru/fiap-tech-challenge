using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Domain.Tests.ValueObjects;

[TestFixture]
internal class CNPJTests
{
    [Test]
    public void Create_ShouldFail_WhenInputIsNull()
    {
        var result = Cnpj.Create(null);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O CNPJ deve ser informado."));
    }

    [Test]
    public void Create_ShouldFail_WhenInputIsWhitespace()
    {
        var result = Cnpj.Create("   ");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O CNPJ deve ser informado."));
    }

    [Test]
    public void Create_ShouldFail_WhenLengthIsNotFourteen()
    {
        var result = Cnpj.Create("1234567890123");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O CNPJ precisa ter exatamente 14 caracteres."));
    }

    [Test]
    public void Create_ShouldFail_WhenVerificationDigitsAreNotNumeric()
    {
        var result = Cnpj.Create("ABCDEF1234567A");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("Os dígitos verificadores do CNPJ devem ser numéricos."));
    }

    [Test]
    public void Create_ShouldFail_WhenAllCharactersAreEqual()
    {
        var result = Cnpj.Create("11111111111111");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O CNPJ informado é inválido."));
    }

    [Test]
    public void Create_ShouldFail_WhenFirstVerificationDigitDoesNotMatch()
    {
        var result = Cnpj.Create("11444777000101");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O CNPJ informado é inválido: o primeiro dígito verificador não confere."));
    }

    [Test]
    public void Create_ShouldFail_WhenSecondVerificationDigitDoesNotMatch()
    {
        var result = Cnpj.Create("11444777000160");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error.Description, Is.EqualTo("O CNPJ informado é inválido: o segundo dígito verificador não confere."));
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValid()
    {
        var result = Cnpj.Create("11.444.777/0001-61");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Unformatted, Is.EqualTo("11444777000161"));
        Assert.That(result.Value.Formatted, Is.EqualTo("11.444.777/0001-61"));
        Assert.That(result.Value.ToString(), Is.EqualTo("11.444.777/0001-61"));
    }

    [Test]
    public void Unformatted_ShouldReturnNormalizedValue()
    {
        var result = Cnpj.Create("11.444.777/0001-61");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Unformatted, Is.EqualTo("11444777000161"));
    }

    [Test]
    public void Formatted_ShouldReturnExpectedMask()
    {
        var result = Cnpj.Create("11444777000161");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Formatted, Is.EqualTo("11.444.777/0001-61"));
    }

    [Test]
    public void ToString_ShouldReturnFormattedValue()
    {
        var result = Cnpj.Create("11444777000161");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.ToString(), Is.EqualTo(result.Value.Formatted));
    }

    [Test]
    public void Equals_ShouldReturnTrue_ForSameNormalizedValue()
    {
        var left = Cnpj.Create("11.444.777/0001-61").Value;
        var right = Cnpj.Create("11444777000161").Value;

        Assert.That(left, Is.Not.Null);
        Assert.That(right, Is.Not.Null);
        Assert.That(left!, Is.EqualTo(right));
    }

    [Test]
    public void Equals_ShouldReturnFalse_WhenOtherIsNull()
    {
        var left = Cnpj.Create("11444777000161").Value;

        Assert.That(left, Is.Not.Null);
        Assert.That(left!.Equals(null), Is.False);
    }

    [Test]
    public void ObjectEquals_ShouldReturnFalse_WhenObjectIsDifferentType()
    {
        var left = Cnpj.Create("11444777000161").Value;

        Assert.That(left, Is.Not.Null);
        Assert.That(left!.Equals(new object()), Is.False);
    }

    [Test]
    public void GetHashCode_ShouldBeEqual_ForEqualObjects()
    {
        var left = Cnpj.Create("11.444.777/0001-61").Value;
        var right = Cnpj.Create("11444777000161").Value;

        Assert.That(left, Is.Not.Null);
        Assert.That(right, Is.Not.Null);
        Assert.That(left!.GetHashCode(), Is.EqualTo(right!.GetHashCode()));
    }

    [Test]
    public void EqualityOperator_ShouldReturnTrue_ForEqualObjects()
    {
        var left = Cnpj.Create("11.444.777/0001-61").Value;
        var right = Cnpj.Create("11444777000161").Value;

        Assert.That(left, Is.Not.Null);
        Assert.That(right, Is.Not.Null);
        Assert.That(left == right, Is.True);
    }

    [Test]
    public void EqualityOperator_ShouldReturnTrue_WhenBothAreNull()
    {
        Cnpj? left = null;
        Cnpj? right = null;

        Assert.That(left == right, Is.True);
    }

    [Test]
    public void InequalityOperator_ShouldReturnTrue_ForDifferentObjects()
    {
        var left = Cnpj.Create("11444777000161").Value;
        var right = Cnpj.Create("04252011000110").Value;

        Assert.That(left, Is.Not.Null);
        Assert.That(right, Is.Not.Null);
        Assert.That(left != right, Is.True);
    }
}
