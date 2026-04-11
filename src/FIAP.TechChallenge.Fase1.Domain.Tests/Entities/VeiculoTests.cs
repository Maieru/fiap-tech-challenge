using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Domain.Tests.Entities;

[TestFixture]
internal class VeiculoTests
{
    [Test]
    public void Create_ShouldFail_WhenClienteIdIsEmpty()
    {
        var result = Veiculo.Create(Guid.Empty, CreatePlacaValida(), "Ford", "Ka", 2020);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O veículo deve estar associado a um cliente válido."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenPlacaIsNull()
    {
        var result = Veiculo.Create(Guid.NewGuid(), null!, "Ford", "Ka", 2020);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A placa do veículo é obrigatória."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenMarcaIsWhitespace()
    {
        var result = Veiculo.Create(Guid.NewGuid(), CreatePlacaValida(), "   ", "Ka", 2020);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A marca do veículo é obrigatória."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenModeloIsWhitespace()
    {
        var result = Veiculo.Create(Guid.NewGuid(), CreatePlacaValida(), "Ford", "   ", 2020);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O modelo do veículo é obrigatório."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenMarcaHasLessThanTwoCharacters()
    {
        var result = Veiculo.Create(Guid.NewGuid(), CreatePlacaValida(), "F", "Ka", 2020);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A marca do veículo deve ter pelo menos 2 caracteres."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenMarcaHasMoreThanOneHundredCharacters()
    {
        var result = Veiculo.Create(Guid.NewGuid(), CreatePlacaValida(), new string('a', 101), "Ka", 2020);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A marca do veículo deve ter no máximo 100 caracteres."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenModeloHasLessThanTwoCharacters()
    {
        var result = Veiculo.Create(Guid.NewGuid(), CreatePlacaValida(), "Ford", "K", 2020);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O modelo do veículo deve ter pelo menos 2 caracteres."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenModeloHasMoreThanOneHundredCharacters()
    {
        var result = Veiculo.Create(Guid.NewGuid(), CreatePlacaValida(), "Ford", new string('a', 101), 2020);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O modelo do veículo deve ter no máximo 100 caracteres."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenAnoIsLowerThanMinimumYear()
    {
        var result = Veiculo.Create(Guid.NewGuid(), CreatePlacaValida(), "Ford", "Ka", DateTime.MinValue.Year - 1);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O ano do veículo é inválido."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenAnoIsGreaterThanNextYear()
    {
        var result = Veiculo.Create(Guid.NewGuid(), CreatePlacaValida(), "Ford", "Ka", DateTime.UtcNow.Year + 2);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O ano do veículo não pode ser maior que o próximo ano."));
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValid()
    {
        var clienteId = Guid.NewGuid();
        var placa = CreatePlacaValida();

        var result = Veiculo.Create(clienteId, placa, "  Ford  ", "  Ka  ", 2020);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(result.Value, Is.Not.Null);
        });

        var veiculo = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(veiculo.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(veiculo.ClienteId, Is.EqualTo(clienteId));
            Assert.That(veiculo.Placa, Is.EqualTo(placa));
            Assert.That(veiculo.Marca, Is.EqualTo("Ford"));
            Assert.That(veiculo.Modelo, Is.EqualTo("Ka"));
            Assert.That(veiculo.Ano, Is.EqualTo(2020));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenIdIsEmpty()
    {
        var result = Veiculo.Rehydrate(Guid.Empty, Guid.NewGuid(), CreatePlacaValida(), "Ford", "Ka", 2020);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O id do veículo é inválido."));
        });
    }

    [Test]
    public void Rehydrate_ShouldSucceed_WhenInputIsValid()
    {
        var id = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var placa = CreatePlacaValida();

        var result = Veiculo.Rehydrate(id, clienteId, placa, "  Toyota  ", "  Corolla  ", 2023);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(result.Value, Is.Not.Null);
        });

        var veiculo = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(veiculo.Id, Is.EqualTo(id));
            Assert.That(veiculo.ClienteId, Is.EqualTo(clienteId));
            Assert.That(veiculo.Placa, Is.EqualTo(placa));
            Assert.That(veiculo.Marca, Is.EqualTo("Toyota"));
            Assert.That(veiculo.Modelo, Is.EqualTo("Corolla"));
            Assert.That(veiculo.Ano, Is.EqualTo(2023));
        });
    }

    [Test]
    public void UpdateMarca_ShouldFail_WhenMarcaIsWhitespace()
    {
        var veiculo = CreateVeiculo();

        var result = veiculo.UpdateMarca("   ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("A marca do veículo é obrigatória."));
            Assert.That(veiculo.Marca, Is.EqualTo("Marca Inicial"));
        });
    }

    [Test]
    public void UpdateMarca_ShouldSucceed_WhenMarcaIsValid()
    {
        var veiculo = CreateVeiculo();

        var result = veiculo.UpdateMarca("  Nova Marca  ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.True);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(veiculo.Marca, Is.EqualTo("Nova Marca"));
        });
    }

    [Test]
    public void UpdateModelo_ShouldFail_WhenModeloIsWhitespace()
    {
        var veiculo = CreateVeiculo();

        var result = veiculo.UpdateModelo("   ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("O modelo do veículo é obrigatório."));
            Assert.That(veiculo.Modelo, Is.EqualTo("Modelo Inicial"));
        });
    }

    [Test]
    public void UpdateModelo_ShouldSucceed_WhenModeloIsValid()
    {
        var veiculo = CreateVeiculo();

        var result = veiculo.UpdateModelo("  Novo Modelo  ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.True);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(veiculo.Modelo, Is.EqualTo("Novo Modelo"));
        });
    }

    [Test]
    public void UpdateAno_ShouldFail_WhenAnoIsGreaterThanNextYear()
    {
        var veiculo = CreateVeiculo();

        var result = veiculo.UpdateAno(DateTime.UtcNow.Year + 2);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("O ano do veículo não pode ser maior que o próximo ano."));
            Assert.That(veiculo.Ano, Is.EqualTo(2020));
        });
    }

    [Test]
    public void UpdateAno_ShouldSucceed_WhenAnoIsValid()
    {
        var veiculo = CreateVeiculo();

        var result = veiculo.UpdateAno(2022);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.True);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(veiculo.Ano, Is.EqualTo(2022));
        });
    }

    [Test]
    public void UpdatePlaca_ShouldFail_WhenPlacaIsNull()
    {
        var veiculo = CreateVeiculo();
        var placaInicial = veiculo.Placa;

        var result = veiculo.UpdatePlaca(null!);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("A placa do veículo é obrigatória."));
            Assert.That(veiculo.Placa, Is.EqualTo(placaInicial));
        });
    }

    [Test]
    public void UpdatePlaca_ShouldSucceed_WhenPlacaIsValid()
    {
        var veiculo = CreateVeiculo();
        var novaPlaca = CreatePlacaValida("ABC1D23");

        var result = veiculo.UpdatePlaca(novaPlaca);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.True);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(veiculo.Placa, Is.EqualTo(novaPlaca));
        });
    }

    private static Veiculo CreateVeiculo()
    {
        var result = Veiculo.Create(Guid.NewGuid(), CreatePlacaValida(), "Marca Inicial", "Modelo Inicial", 2020);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        });

        return result.Value!;
    }

    private static Placa CreatePlacaValida(string value = "ABC1234")
    {
        var result = Placa.Create(value);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
        });

        return result.Value!;
    }
}
