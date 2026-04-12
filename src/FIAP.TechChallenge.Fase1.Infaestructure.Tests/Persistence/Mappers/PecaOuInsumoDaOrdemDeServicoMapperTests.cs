using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Mappers;

[TestFixture]
internal sealed class PecaOuInsumoDaOrdemDeServicoMapperTests
{
    [Test]
    public void ToEntity_ShouldMapAllFields_WhenDomainIsValid()
    {
        var snapshot = new PecaOuInsumoDaOrdemDeServicoSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Pastilha de freio",
            "PF-100",
            "Jogo dianteiro",
            199.90m,
            2);
        var domain = PecaOuInsumoDaOrdemDeServico.Rehydrate(snapshot).Value!;

        var entity = PecaOuInsumoDaOrdemDeServicoMapper.ToEntity(domain);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(domain.Id));
            Assert.That(entity.OrdemServicoId, Is.EqualTo(domain.OrdemServicoId));
            Assert.That(entity.PecaInsumoId, Is.EqualTo(domain.PecaInsumoId));
            Assert.That(entity.Nome, Is.EqualTo(domain.Nome));
            Assert.That(entity.Codigo, Is.EqualTo(domain.Codigo));
            Assert.That(entity.Descricao, Is.EqualTo(domain.Descricao));
            Assert.That(entity.PrecoUnitario, Is.EqualTo(domain.PrecoUnitario));
            Assert.That(entity.Quantidade, Is.EqualTo(domain.Quantidade));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnSuccess_WhenEntityIsValid()
    {
        var entity = new PecaOuInsumoDaOrdemDeServicoEntity
        {
            Id = Guid.NewGuid(),
            OrdemServicoId = Guid.NewGuid(),
            PecaInsumoId = Guid.NewGuid(),
            Nome = "Filtro de óleo",
            Codigo = "FO-456",
            Descricao = "Aplicação linha leve",
            PrecoUnitario = 39.5m,
            Quantidade = 3
        };

        var result = PecaOuInsumoDaOrdemDeServicoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(entity.Id));
            Assert.That(result.Value.OrdemServicoId, Is.EqualTo(entity.OrdemServicoId));
            Assert.That(result.Value.PecaInsumoId, Is.EqualTo(entity.PecaInsumoId));
            Assert.That(result.Value.Nome, Is.EqualTo(entity.Nome));
            Assert.That(result.Value.Codigo, Is.EqualTo(entity.Codigo));
            Assert.That(result.Value.Descricao, Is.EqualTo(entity.Descricao));
            Assert.That(result.Value.PrecoUnitario, Is.EqualTo(entity.PrecoUnitario));
            Assert.That(result.Value.Quantidade, Is.EqualTo(entity.Quantidade));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnFailure_WhenEntityIsInvalid()
    {
        var entity = new PecaOuInsumoDaOrdemDeServicoEntity
        {
            Id = Guid.Empty,
            OrdemServicoId = Guid.NewGuid(),
            PecaInsumoId = Guid.NewGuid(),
            Nome = "Correia",
            Codigo = "CR-01",
            Descricao = "Item inválido",
            PrecoUnitario = 10m,
            Quantidade = 1
        };

        var result = PecaOuInsumoDaOrdemDeServicoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });
    }
}
