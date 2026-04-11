using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;

namespace FIAP.TechChallenge.Fase1.Domain.Tests.Entities;

[TestFixture]
internal class OrdemServicoTests
{
    [Test]
    public void Create_ShouldFail_WhenClienteIdIsEmpty()
    {
        var result = OrdemServico.Create(Guid.Empty, Guid.NewGuid(), "Problema no motor");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A ordem de serviço deve estar associada a um cliente válido."));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenDataCriacaoIsDefault()
    {
        var snapshot = new OrdemServicoSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Problema no ar-condicionado",
            StatusOrdemServico.Recebida,
            default(DateTime),
            null,
            null,
            null,
            null,
            null);

        var result = OrdemServico.Rehydrate(snapshot);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A data de criação da ordem de serviço é obrigatória."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenVeiculoIdIsEmpty()
    {
        var result = OrdemServico.Create(Guid.NewGuid(), Guid.Empty, "Problema no motor");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A ordem de serviço deve estar associada a um veículo válido."));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenStatusIsAguardandoAprovacaoAndDataEnvioAprovacaoIsNull()
    {
        var snapshot = new OrdemServicoSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Problema no ar-condicionado",
            StatusOrdemServico.AguardandoAprovacao,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null,
            null,
            null);

        var result = OrdemServico.Rehydrate(snapshot);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A ordem de serviço aguardando aprovação ou posterior deve possuir data de envio para aprovação."));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenStatusIsEmExecucaoAndDataInicioExecucaoIsNull()
    {
        var snapshot = new OrdemServicoSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Problema no ar-condicionado",
            StatusOrdemServico.EmExecucao,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null,
            null);

        var result = OrdemServico.Rehydrate(snapshot);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A ordem de serviço em execução ou posterior deve possuir data de início da execução."));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenStatusIsFinalizadaAndDataFinalizacaoIsNull()
    {
        var snapshot = new OrdemServicoSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Problema no ar-condicionado",
            StatusOrdemServico.Finalizada,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null);

        var result = OrdemServico.Rehydrate(snapshot);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A ordem de serviço finalizada ou entregue deve possuir data de finalização."));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenStatusIsEntregueAndDataEntregaIsNull()
    {
        var snapshot = new OrdemServicoSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Problema no ar-condicionado",
            StatusOrdemServico.Entregue,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null);

        var result = OrdemServico.Rehydrate(snapshot);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A ordem de serviço entregue deve possuir data de entrega."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenDescricaoProblemaIsWhitespace()
    {
        var result = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "   ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A descrição do problema é obrigatória."));
        });
    }

    [Test]
    public void AguardarAprovacao_ShouldFail_WhenStatusIsNotEmDiagnostico()
    {
        var ordemServico = CreateOrdemServicoValida();

        var result = ordemServico.AguardarAprovacao();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("Somente ordens de serviço em diagnóstico podem aguardar aprovação."));
            Assert.That(ordemServico.Status, Is.EqualTo(StatusOrdemServico.Recebida));
            Assert.That(ordemServico.DataEnvioAprovacao, Is.Null);
        });
    }

    [Test]
    public void AprovarOrcamento_ShouldFail_WhenStatusIsNotAguardandoAprovacao()
    {
        var ordemServico = CreateOrdemServicoValida();

        var result = ordemServico.AprovarOrcamento();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("Somente ordens de serviço aguardando aprovação podem ser aprovadas."));
            Assert.That(ordemServico.Status, Is.EqualTo(StatusOrdemServico.Recebida));
            Assert.That(ordemServico.DataInicioExecucao, Is.Null);
        });
    }

    [Test]
    public void Finalizar_ShouldFail_WhenStatusIsNotEmExecucao()
    {
        var ordemServico = CreateOrdemServicoValida();

        var result = ordemServico.Finalizar();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("Somente ordens de serviço em execução podem ser finalizadas."));
            Assert.That(ordemServico.Status, Is.EqualTo(StatusOrdemServico.Recebida));
            Assert.That(ordemServico.DataFinalizacao, Is.Null);
        });
    }

    [Test]
    public void Create_ShouldFail_WhenDescricaoProblemaHasLessThanThreeCharacters()
    {
        var result = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "ab");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A descrição do problema deve ter pelo menos 3 caracteres."));
        });
    }

    [Test]
    public void Create_ShouldFail_WhenDescricaoProblemaHasMoreThanOneThousandCharacters()
    {
        var result = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), new string('a', 1001));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A descrição do problema deve ter no máximo 1000 caracteres."));
        });
    }

    [Test]
    public void Create_ShouldSucceed_WhenInputIsValid()
    {
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var result = OrdemServico.Create(clienteId, veiculoId, "  Barulho no freio traseiro  ");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
        });

        var ordemServico = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(ordemServico.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(ordemServico.ClienteId, Is.EqualTo(clienteId));
            Assert.That(ordemServico.VeiculoId, Is.EqualTo(veiculoId));
            Assert.That(ordemServico.DescricaoProblema, Is.EqualTo("Barulho no freio traseiro"));
            Assert.That(ordemServico.Status, Is.EqualTo(StatusOrdemServico.Recebida));
            Assert.That(ordemServico.DataCriacao, Is.GreaterThanOrEqualTo(before));
            Assert.That(ordemServico.DataInicioDiagnostico, Is.Null);
            Assert.That(ordemServico.DataEnvioAprovacao, Is.Null);
            Assert.That(ordemServico.DataInicioExecucao, Is.Null);
            Assert.That(ordemServico.DataFinalizacao, Is.Null);
            Assert.That(ordemServico.DataEntrega, Is.Null);
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenIdIsEmpty()
    {
        var snapshot = new OrdemServicoSnapshot(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Problema no ar-condicionado",
            StatusOrdemServico.Recebida,
            DateTime.UtcNow,
            null,
            null,
            null,
            null,
            null);

        var result = OrdemServico.Rehydrate(snapshot);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O id da ordem de serviço é inválido."));
        });
    }

    [Test]
    public void Rehydrate_ShouldFail_WhenStatusIsEmDiagnosticoAndDataInicioDiagnosticoIsNull()
    {
        var snapshot = new OrdemServicoSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Problema no ar-condicionado",
            StatusOrdemServico.EmDiagnostico,
            DateTime.UtcNow,
            null,
            null,
            null,
            null,
            null);

        var result = OrdemServico.Rehydrate(snapshot);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("A ordem de serviço em diagnóstico ou posterior deve possuir data de início do diagnóstico."));
        });
    }

    [Test]
    public void Rehydrate_ShouldSucceed_WhenSnapshotIsValid()
    {
        var dataCriacao = DateTime.UtcNow.AddDays(-5);
        var dataInicioDiagnostico = dataCriacao.AddHours(2);
        var dataEnvioAprovacao = dataInicioDiagnostico.AddHours(1);
        var dataInicioExecucao = dataEnvioAprovacao.AddHours(2);
        var dataFinalizacao = dataInicioExecucao.AddDays(1);
        var dataEntrega = dataFinalizacao.AddHours(3);

        var snapshot = new OrdemServicoSnapshot(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Troca de pastilhas de freio",
            StatusOrdemServico.Entregue,
            dataCriacao,
            dataInicioDiagnostico,
            dataEnvioAprovacao,
            dataInicioExecucao,
            dataFinalizacao,
            dataEntrega);

        var result = OrdemServico.Rehydrate(snapshot);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
        });

        var ordemServico = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(ordemServico.Id, Is.EqualTo(snapshot.Id));
            Assert.That(ordemServico.ClienteId, Is.EqualTo(snapshot.ClienteId));
            Assert.That(ordemServico.VeiculoId, Is.EqualTo(snapshot.VeiculoId));
            Assert.That(ordemServico.DescricaoProblema, Is.EqualTo(snapshot.DescricaoProblema));
            Assert.That(ordemServico.Status, Is.EqualTo(StatusOrdemServico.Entregue));
            Assert.That(ordemServico.DataCriacao, Is.EqualTo(dataCriacao));
            Assert.That(ordemServico.DataInicioDiagnostico, Is.EqualTo(dataInicioDiagnostico));
            Assert.That(ordemServico.DataEnvioAprovacao, Is.EqualTo(dataEnvioAprovacao));
            Assert.That(ordemServico.DataInicioExecucao, Is.EqualTo(dataInicioExecucao));
            Assert.That(ordemServico.DataFinalizacao, Is.EqualTo(dataFinalizacao));
            Assert.That(ordemServico.DataEntrega, Is.EqualTo(dataEntrega));
        });
    }

    [Test]
    public void IniciarDiagnostico_ShouldFail_WhenStatusIsNotRecebida()
    {
        var ordemServico = CreateOrdemServicoValida();
        _ = ordemServico.IniciarDiagnostico();

        var result = ordemServico.IniciarDiagnostico();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("Somente ordens de serviço recebidas podem iniciar diagnóstico."));
        });
    }

    [Test]
    public void FluxoCompleto_ShouldSucceed_WhenTransitionsAreValid()
    {
        var ordemServico = CreateOrdemServicoValida();

        var iniciarDiagnosticoResult = ordemServico.IniciarDiagnostico();
        var aguardarAprovacaoResult = ordemServico.AguardarAprovacao();
        var aprovarOrcamentoResult = ordemServico.AprovarOrcamento();
        var finalizarResult = ordemServico.Finalizar();
        var entregarResult = ordemServico.Entregar();

        Assert.Multiple(() =>
        {
            Assert.That(iniciarDiagnosticoResult.IsSuccess, Is.True);
            Assert.That(aguardarAprovacaoResult.IsSuccess, Is.True);
            Assert.That(aprovarOrcamentoResult.IsSuccess, Is.True);
            Assert.That(finalizarResult.IsSuccess, Is.True);
            Assert.That(entregarResult.IsSuccess, Is.True);
            Assert.That(ordemServico.Status, Is.EqualTo(StatusOrdemServico.Entregue));
            Assert.That(ordemServico.DataInicioDiagnostico, Is.Not.Null);
            Assert.That(ordemServico.DataEnvioAprovacao, Is.Not.Null);
            Assert.That(ordemServico.DataInicioExecucao, Is.Not.Null);
            Assert.That(ordemServico.DataFinalizacao, Is.Not.Null);
            Assert.That(ordemServico.DataEntrega, Is.Not.Null);
        });
    }

    [Test]
    public void Entregar_ShouldFail_WhenStatusIsNotFinalizada()
    {
        var ordemServico = CreateOrdemServicoValida();

        var result = ordemServico.Entregar();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.False);
            Assert.That(result.Error.Description, Is.EqualTo("Somente ordens de serviço finalizadas podem ser entregues."));
            Assert.That(ordemServico.Status, Is.EqualTo(StatusOrdemServico.Recebida));
            Assert.That(ordemServico.DataEntrega, Is.Null);
        });
    }

    private static OrdemServico CreateOrdemServicoValida()
    {
        var result = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Falha ao ligar o veículo");
        return result.Value!;
    }
}
