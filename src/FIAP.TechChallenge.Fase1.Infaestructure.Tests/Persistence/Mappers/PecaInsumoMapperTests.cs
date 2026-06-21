using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Mappers;

[TestFixture]
internal sealed class PecaInsumoMapperTests
{
    [Test]
    public void ToEntity_ShouldMapAllFields_WhenPecaInsumoIsValid()
    {
        var pecaInsumo = PecaInsumo.Rehydrate(
            Guid.NewGuid(),
            "Pastilha de freio",
            "PF-123",
            "Jogo dianteiro",
            249.90m,
            12,
            false).Value!;

        var entity = PecaInsumoMapper.ToEntity(pecaInsumo);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(pecaInsumo.Id));
            Assert.That(entity.Nome, Is.EqualTo(pecaInsumo.Nome));
            Assert.That(entity.Codigo, Is.EqualTo(pecaInsumo.Codigo));
            Assert.That(entity.Descricao, Is.EqualTo(pecaInsumo.Descricao));
            Assert.That(entity.PrecoUnitario, Is.EqualTo(pecaInsumo.PrecoUnitario));
            Assert.That(entity.QuantidadeEstoque, Is.EqualTo(pecaInsumo.QuantidadeEstoque));
            Assert.That(entity.Ativo, Is.EqualTo(pecaInsumo.Ativo));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnSuccess_WhenEntityIsValid()
    {
        var entity = new PecaInsumoEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Filtro de Óleo",
            Codigo = "FO-456",
            Descricao = "Aplicação linha leve",
            PrecoUnitario = 39.5m,
            QuantidadeEstoque = 25,
            Ativo = true
        };

        var result = PecaInsumoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(entity.Id));
            Assert.That(result.Value.Nome, Is.EqualTo(entity.Nome));
            Assert.That(result.Value.Codigo, Is.EqualTo(entity.Codigo));
            Assert.That(result.Value.Descricao, Is.EqualTo(entity.Descricao));
            Assert.That(result.Value.PrecoUnitario, Is.EqualTo(entity.PrecoUnitario));
            Assert.That(result.Value.QuantidadeEstoque, Is.EqualTo(entity.QuantidadeEstoque));
            Assert.That(result.Value.Ativo, Is.EqualTo(entity.Ativo));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnFailure_WhenEntityIsInvalid()
    {
        var entity = new PecaInsumoEntity
        {
            Id = Guid.Empty,
            Nome = "Correia dentada",
            Codigo = "CD-789",
            Descricao = "",
            PrecoUnitario = 89.9m,
            QuantidadeEstoque = 8,
            Ativo = true
        };

        var result = PecaInsumoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });
    }
}

