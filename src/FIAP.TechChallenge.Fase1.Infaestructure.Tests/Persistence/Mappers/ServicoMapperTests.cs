using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Mappers;

[TestFixture]
internal sealed class ServicoMapperTests
{
    [Test]
    public void ToEntity_ShouldMapAllFields_WhenServicoIsValid()
    {
        var servico = Servico.Rehydrate(Guid.NewGuid(), "Troca de pastilhas", 320m).Value!;

        var entity = ServicoMapper.ToEntity(servico);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(servico.Id));
            Assert.That(entity.Descricao, Is.EqualTo(servico.Descricao));
            Assert.That(entity.ValorUnitario, Is.EqualTo(servico.ValorUnitario));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnSuccess_WhenEntityIsValid()
    {
        var entity = new ServicoEntity
        {
            Id = Guid.NewGuid(),
            Descricao = "Troca de bateria",
            ValorUnitario = 450m
        };

        var result = ServicoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(entity.Id));
            Assert.That(result.Value.Descricao, Is.EqualTo(entity.Descricao));
            Assert.That(result.Value.ValorUnitario, Is.EqualTo(entity.ValorUnitario));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnFailure_WhenEntityIsInvalid()
    {
        var entity = new ServicoEntity
        {
            Id = Guid.Empty,
            Descricao = "Serviço inválido",
            ValorUnitario = 100m
        };

        var result = ServicoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });
    }
}
