using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Mappers;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Mappers;

[TestFixture]
internal sealed class OrdemServicoMapperTests
{
    [Test]
    public void ToEntity_ShouldMapAllFields_WhenOrdemServicoIsValid()
    {
        var dataCriacao = new DateTime(2026, 04, 11, 10, 0, 0, DateTimeKind.Utc);
        var dataInicioDiagnostico = dataCriacao.AddHours(1);
        var dataEnvioAprovacao = dataCriacao.AddHours(2);
        var dataInicioExecucao = dataCriacao.AddHours(3);
        var dataFinalizacao = dataCriacao.AddHours(4);
        var dataEntrega = dataCriacao.AddHours(5);
        var snapshot = new OrdemServicoSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Ruido metalico no motor",
            StatusOrdemServico.Entregue,
            dataCriacao,
            dataInicioDiagnostico,
            dataEnvioAprovacao,
            dataInicioExecucao,
            dataFinalizacao,
            dataEntrega);
        var ordemServico = OrdemServico.Rehydrate(snapshot).Value!;

        var entity = OrdemServicoMapper.ToEntity(ordemServico);

        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(snapshot.Id));
            Assert.That(entity.ClienteId, Is.EqualTo(snapshot.ClienteId));
            Assert.That(entity.VeiculoId, Is.EqualTo(snapshot.VeiculoId));
            Assert.That(entity.DescricaoProblema, Is.EqualTo(snapshot.DescricaoProblema));
            Assert.That(entity.Status, Is.EqualTo(snapshot.Status));
            Assert.That(entity.DataCriacao, Is.EqualTo(snapshot.DataCriacao));
            Assert.That(entity.DataInicioDiagnostico, Is.EqualTo(snapshot.DataInicioDiagnostico));
            Assert.That(entity.DataEnvioAprovacao, Is.EqualTo(snapshot.DataEnvioAprovacao));
            Assert.That(entity.DataInicioExecucao, Is.EqualTo(snapshot.DataInicioExecucao));
            Assert.That(entity.DataFinalizacao, Is.EqualTo(snapshot.DataFinalizacao));
            Assert.That(entity.DataEntrega, Is.EqualTo(snapshot.DataEntrega));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnSuccess_WhenEntityIsValid()
    {
        var entity = new OrdemServicoEntity
        {
            Id = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Falha eletrica intermitente",
            Status = StatusOrdemServico.Recebida,
            DataCriacao = new DateTime(2026, 04, 11, 12, 0, 0, DateTimeKind.Utc)
        };

        var result = OrdemServicoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(entity.Id));
            Assert.That(result.Value.ClienteId, Is.EqualTo(entity.ClienteId));
            Assert.That(result.Value.VeiculoId, Is.EqualTo(entity.VeiculoId));
            Assert.That(result.Value.DescricaoProblema, Is.EqualTo(entity.DescricaoProblema));
            Assert.That(result.Value.Status, Is.EqualTo(entity.Status));
            Assert.That(result.Value.DataCriacao, Is.EqualTo(entity.DataCriacao));
        });
    }

    [Test]
    public void ToDomain_ShouldReturnFailure_WhenEntityStatusFlowIsInconsistent()
    {
        var entity = new OrdemServicoEntity
        {
            Id = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Troca de embreagem",
            Status = StatusOrdemServico.EmDiagnostico,
            DataCriacao = new DateTime(2026, 04, 11, 12, 0, 0, DateTimeKind.Utc),
            DataInicioDiagnostico = null
        };

        var result = OrdemServicoMapper.ToDomain(entity);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });
    }
}

