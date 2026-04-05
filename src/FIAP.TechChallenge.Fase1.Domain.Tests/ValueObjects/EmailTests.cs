using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Domain.Tests.ValueObjects;

[TestFixture]
internal class EmailTests
{
    [Test]
    public void Create_ShouldFail_WhenInputIsNull()
    {
        var result = Email.Create(null!);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O e-mail é obrigatório."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenInputIsWhitespace()
    {
        var result = Email.Create("   ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O e-mail é obrigatório."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenLengthIsGreaterThanTwoHundred()
    {
        var value = $"{new string('a', 195)}@x.com";

        var result = Email.Create(value);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O e-mail deve ter no máximo 200 caracteres."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenInputIsInvalid()
    {
        var result = Email.Create("email-invalido");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O e-mail informado é inválido."));
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValid()
    {
        var result = Email.Create("usuario@exemplo.com");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Abstractions.Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Value, Is.EqualTo("usuario@exemplo.com"));
            Assert.That(result.Value.ToString(), Is.EqualTo("usuario@exemplo.com"));
        });
    }

    [Test]
    public void Create_ShouldTrimInput_WhenInputIsValidWithSpaces()
    {
        var result = Email.Create("  usuario@exemplo.com  ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Value, Is.EqualTo("usuario@exemplo.com"));
        });
    }

    [Test]
    public void IsValid_ShouldFail_WhenInputIsNull()
    {
        var result = Email.IsValid(null!);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("O e-mail é obrigatório."));
        });
    }

    [Test]
    public void IsValid_ShouldFail_WhenInputIsWhitespace()
    {
        var result = Email.IsValid("   ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("O e-mail é obrigatório."));
        });
    }

    [Test]
    public void IsValid_ShouldFail_WhenLengthIsGreaterThanTwoHundred()
    {
        var value = $"{new string('a', 195)}@x.com";

        var result = Email.IsValid(value);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("O e-mail deve ter no máximo 200 caracteres."));
        });
    }

    [Test]
    public void IsValid_ShouldSucceed_WhenInputIsValid()
    {
        var result = Email.IsValid("usuario@exemplo.com");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Abstractions.Error.None));
            Assert.That(result.Value, Is.True);
        });
    }

    [Test]
    public void Equals_ShouldReturnTrue_ForSameValueIgnoringCase()
    {
        var left = Email.Create("usuario@exemplo.com").Value;
        var right = Email.Create("USUARIO@EXEMPLO.COM").Value;

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
        var left = Email.Create("usuario@exemplo.com").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(left!, Is.Not.Null);
        });
    }

    [Test]
    public void ObjectEquals_ShouldReturnFalse_WhenObjectIsDifferentType()
    {
        var left = Email.Create("usuario@exemplo.com").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(left!, Is.Not.EqualTo(new object()));
        });
    }

    [Test]
    public void GetHashCode_ShouldBeEqual_ForEqualObjectsIgnoringCase()
    {
        var left = Email.Create("usuario@exemplo.com").Value;
        var right = Email.Create("USUARIO@EXEMPLO.COM").Value;

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
        var left = Email.Create("usuario@exemplo.com").Value;
        var right = Email.Create("USUARIO@EXEMPLO.COM").Value;

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
        Email? left = null;
        Email? right = null;

        Assert.That(left, Is.EqualTo(right));
    }

    [Test]
    public void InequalityOperator_ShouldReturnTrue_ForDifferentObjects()
    {
        var left = Email.Create("usuario@exemplo.com").Value;
        var right = Email.Create("outro@exemplo.com").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(right, Is.Not.Null);
            Assert.That(left, Is.Not.EqualTo(right));
        });
    }
}
