using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Domain.Tests.ValueObjects;

[TestFixture]
internal class PlacaTests
{
    [Test]
    public void Create_ShouldFail_WhenInputIsNull()
    {
        var result = Placa.Create(null);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A placa deve ser informada."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenInputIsWhitespace()
    {
        var result = Placa.Create("   ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A placa deve ser informada."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenInputHasNoAlphanumericCharacters()
    {
        var result = Placa.Create("---");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A placa deve ser informada."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenLengthIsInvalid()
    {
        var result = Placa.Create("ABC123");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A placa deve ter exatamente 7 caracteres alfanuméricos."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenPatternIsInvalid()
    {
        var result = Placa.Create("AB12345");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A placa informada é inválida. Use o padrão antigo (AAA1234) ou Mercosul (AAA1A23)."));
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValidOldPattern()
    {
        var result = Placa.Create("abc-1234");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Unformatted, Is.EqualTo("ABC1234"));
            Assert.That(result.Value.IsPadraoAntigo, Is.True);
            Assert.That(result.Value.IsMercosul, Is.False);
            Assert.That(result.Value.ToString(), Is.EqualTo("ABC1234"));
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValidMercosulPattern()
    {
        var result = Placa.Create("abc1d23");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Unformatted, Is.EqualTo("ABC1D23"));
            Assert.That(result.Value.IsPadraoAntigo, Is.False);
            Assert.That(result.Value.IsMercosul, Is.True);
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputNeedsNormalizationBeforeValidation_ForOldPattern()
    {
        var result = Placa.Create(" a-bc 12-34 ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Unformatted, Is.EqualTo("ABC1234"));
            Assert.That(result.Value.IsPadraoAntigo, Is.True);
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputNeedsNormalizationBeforeValidation_ForMercosulPattern()
    {
        var result = Placa.Create(" a-bc 1 d-23 ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Unformatted, Is.EqualTo("ABC1D23"));
            Assert.That(result.Value.IsMercosul, Is.True);
        });
    }

    [Test]
    public void Equals_ShouldReturnTrue_ForSameNormalizedValue()
    {
        var left = Placa.Create("abc-1234").Value;
        var right = Placa.Create("ABC1234").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(right, Is.Not.Null);
            Assert.That(left!, Is.EqualTo(right));
            Assert.That(left!.GetHashCode(), Is.EqualTo(right!.GetHashCode()));
        });
    }

    [Test]
    public void InequalityOperator_ShouldReturnTrue_ForDifferentObjects()
    {
        var left = Placa.Create("ABC1234").Value;
        var right = Placa.Create("ABC1D23").Value;

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.Not.Null);
            Assert.That(right, Is.Not.Null);
            Assert.That(left, Is.Not.EqualTo(right));
        });
    }
}
