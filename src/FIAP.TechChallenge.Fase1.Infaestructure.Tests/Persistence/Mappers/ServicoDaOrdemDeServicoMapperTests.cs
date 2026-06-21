using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Mappers;

[TestFixture]
internal sealed class ServicoDaOrdemDeServicoMapperTests
{
    [Test]
    public void ToEntity_ShouldMapAllFields_WhenDomainIsValid()
    {
        var domain = ServicoDaOrdemDeServico.Rehydrate(
            new ServicoDaOrdemDeServicoSnapshot(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Balanceamento",
                89.9m,
                2,
                45,
                true)).Value!;

        var entity = ServicoDaOrdemDeServicoMapper.ToEntity(domain);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(domain.Id));
            Assert.That(entity.OrdemServicoId, Is.EqualTo(domain.OrdemServicoId));
            Assert.That(entity.ServicoId, Is.EqualTo(domain.ServicoId));
            Assert.That(entity.Descricao, Is.EqualTo(domain.Descricao));
            Assert.That(entity.ValorUnitario, Is.EqualTo(domain.ValorUnitario));
            Assert.That(entity.Quantidade, Is.EqualTo(domain.Quantidade));
            Assert.That(entity.TempoGastoMinutos, Is.EqualTo(domain.TempoGastoMinutos));
            Assert.That(entity.Concluido, Is.EqualTo(domain.Concluido));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnSuccess_WhenEntityIsValid()
    {
        var entity = new ServicoDaOrdemDeServicoEntity
        {
            Id = Guid.NewGuid(),
            OrdemServicoId = Guid.NewGuid(),
            ServicoId = Guid.NewGuid(),
            Descricao = "Troca de oleo",
            ValorUnitario = 150m,
            Quantidade = 1,
            TempoGastoMinutos = 30,
            Concluido = true
        };

        var result = ServicoDaOrdemDeServicoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(entity.Id));
            Assert.That(result.Value.OrdemServicoId, Is.EqualTo(entity.OrdemServicoId));
            Assert.That(result.Value.ServicoId, Is.EqualTo(entity.ServicoId));
            Assert.That(result.Value.Descricao, Is.EqualTo(entity.Descricao));
            Assert.That(result.Value.ValorUnitario, Is.EqualTo(entity.ValorUnitario));
            Assert.That(result.Value.Quantidade, Is.EqualTo(entity.Quantidade));
            Assert.That(result.Value.TempoGastoMinutos, Is.EqualTo(entity.TempoGastoMinutos));
            Assert.That(result.Value.Concluido, Is.EqualTo(entity.Concluido));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnFailure_WhenEntityIsInvalid()
    {
        var entity = new ServicoDaOrdemDeServicoEntity
        {
            Id = Guid.Empty,
            OrdemServicoId = Guid.NewGuid(),
            ServicoId = Guid.NewGuid(),
            Descricao = "Servico invalido",
            ValorUnitario = 50m,
            Quantidade = 1,
            TempoGastoMinutos = null,
            Concluido = false
        };

        var result = ServicoDaOrdemDeServicoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });
    }
}

