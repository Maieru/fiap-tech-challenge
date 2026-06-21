using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Mappers;

[TestFixture]
internal sealed class VeiculoMapperTests
{
    [Test]
    public void ToEntity_ShouldMapAllFields_WhenVeiculoIsValid()
    {
        var veiculo = CreateVeiculo();

        var entity = VeiculoMapper.ToEntity(veiculo);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(veiculo.Id));
            Assert.That(entity.ClienteId, Is.EqualTo(veiculo.ClienteId));
            Assert.That(entity.Placa, Is.EqualTo(veiculo.Placa.Unformatted));
            Assert.That(entity.Marca, Is.EqualTo(veiculo.Marca));
            Assert.That(entity.Modelo, Is.EqualTo(veiculo.Modelo));
            Assert.That(entity.Ano, Is.EqualTo(veiculo.Ano));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnSuccess_WhenEntityIsValid()
    {
        var entity = new VeiculoEntity
        {
            Id = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            Placa = "BRA2E19",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023
        };

        var result = VeiculoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(entity.Id));
            Assert.That(result.Value.ClienteId, Is.EqualTo(entity.ClienteId));
            Assert.That(result.Value.Placa.Unformatted, Is.EqualTo(entity.Placa));
            Assert.That(result.Value.Marca, Is.EqualTo(entity.Marca));
            Assert.That(result.Value.Modelo, Is.EqualTo(entity.Modelo));
            Assert.That(result.Value.Ano, Is.EqualTo(entity.Ano));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnFailure_WhenPlacaIsInvalid()
    {
        var entity = new VeiculoEntity
        {
            Id = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            Placa = "123",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023
        };

        var result = VeiculoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });
    }

    private static Veiculo CreateVeiculo()
    {
        var clienteId = Guid.NewGuid();
        var placa = Placa.Create("BRA2E19").Value!;

        return Veiculo.Create(clienteId, placa, "Toyota", "Corolla", 2023).Value!;
    }
}
