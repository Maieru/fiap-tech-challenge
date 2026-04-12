using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;

namespace FIAP.TechChallenge.Fase1.Domain.Tests.Entities;

[TestFixture]
internal sealed class ServicoTests
{
    [Test]
    public void Create_ShouldFail_WhenDescricaoIsWhitespace()
    {
        var result = Servico.Create("   ", 150m);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A descrição do serviço é obrigatória."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenValorUnitarioIsNegative()
    {
        var result = Servico.Create("Troca de óleo", -1m);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O valor do serviço não pode ser negativo."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenDescricaoHasMoreThanOneThousandCharacters()
    {
        var result = Servico.Create(new string('a', 1001), 100m);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A descrição do serviço deve conter no máximo 1000 caracteres."));
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValid()
    {
        var result = Servico.Create("  Alinhamento e balanceamento  ", 250.75m);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
        });

        var servico = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(servico.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(servico.Descricao, Is.EqualTo("Alinhamento e balanceamento"));
            Assert.That(servico.ValorUnitario, Is.EqualTo(250.75m));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenIdIsEmpty()
    {
        var result = Servico.Rehydrate(Guid.Empty, "Troca de filtro", 80m);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O id do serviço é inválido."));
        });
    }

    [Test]
    public void Rehydrate_ShouldSucceed_WhenInputIsValid()
    {
        var id = Guid.NewGuid();

        var result = Servico.Rehydrate(id, "  Higienização do ar-condicionado  ", 199.9m);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
        });

        var servico = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(servico.Id, Is.EqualTo(id));
            Assert.That(servico.Descricao, Is.EqualTo("Higienização do ar-condicionado"));
            Assert.That(servico.ValorUnitario, Is.EqualTo(199.9m));
        });
    }
}
