using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConcluirServicoOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.ConcluirServicoOrdemServico;

[TestFixture]
internal sealed class ConcluirServicoOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenServicoDaOrdemDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();

        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServicoDaOrdemDeServico>.Failure(new Error("Serviço da ordem de serviço não encontrado.", ErrorCode.NotFound)));

        var useCase = new ConcluirServicoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Serviço da ordem de serviço não encontrado."));
        });

        ordemServicoRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        servicoDaOrdemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var servicoDaOrdem = CreateServicoDaOrdem();

        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByIdAsync(servicoDaOrdem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServicoDaOrdemDeServico>.Success(servicoDaOrdem));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(servicoDaOrdem.OrdemServicoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.", ErrorCode.NotFound)));

        var useCase = new ConcluirServicoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(servicoDaOrdem.Id, 30));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
        });

        servicoDaOrdemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIsNotEmExecucao()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var servicoDaOrdem = CreateServicoDaOrdem();
        var ordemServico = CreateOrdemServico(emExecucao: false);

        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByIdAsync(servicoDaOrdem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServicoDaOrdemDeServico>.Success(servicoDaOrdem));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(servicoDaOrdem.OrdemServicoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        var useCase = new ConcluirServicoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(servicoDaOrdem.Id, 30));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Somente ordens de serviço em execução podem concluir serviços."));
        });

        servicoDaOrdemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenTempoGastoIsInvalid()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var servicoDaOrdem = CreateServicoDaOrdem();
        var ordemServico = CreateOrdemServico(emExecucao: true);

        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByIdAsync(servicoDaOrdem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServicoDaOrdemDeServico>.Success(servicoDaOrdem));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(servicoDaOrdem.OrdemServicoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        var useCase = new ConcluirServicoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(servicoDaOrdem.Id, 0));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O tempo gasto do serviço da ordem de serviço deve ser maior que zero."));
        });

        servicoDaOrdemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenServicoDaOrdemAlreadyConcluded()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var servicoDaOrdem = CreateServicoDaOrdem(concluido: true, tempoGastoMinutos: 20);
        var ordemServico = CreateOrdemServico(emExecucao: true);

        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByIdAsync(servicoDaOrdem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServicoDaOrdemDeServico>.Success(servicoDaOrdem));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(servicoDaOrdem.OrdemServicoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        var useCase = new ConcluirServicoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(servicoDaOrdem.Id, 15));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O serviço da ordem de serviço já foi concluído."));
        });

        servicoDaOrdemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var servicoDaOrdemRepositoryMock = new Mock<IServicoDaOrdemDeServicoRepository>();
        var servicoDaOrdem = CreateServicoDaOrdem();
        var ordemServico = CreateOrdemServico(emExecucao: true);
        ServicoDaOrdemDeServico? servicoAtualizado = null;

        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.GetByIdAsync(servicoDaOrdem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ServicoDaOrdemDeServico>.Success(servicoDaOrdem));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(servicoDaOrdem.OrdemServicoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = servicoDaOrdemRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()))
            .Callback<ServicoDaOrdemDeServico, CancellationToken>((item, _) => servicoAtualizado = item)
            .Returns(Task.CompletedTask);

        var useCase = new ConcluirServicoOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            servicoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(servicoDaOrdem.Id, 55));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(servicoAtualizado, Is.Not.Null);
        });

        servicoDaOrdemRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ServicoDaOrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(servicoDaOrdem.Id));
            Assert.That(response.OrdemServicoId, Is.EqualTo(servicoDaOrdem.OrdemServicoId));
            Assert.That(response.ServicoId, Is.EqualTo(servicoDaOrdem.ServicoId));
            Assert.That(response.TempoGastoMinutos, Is.EqualTo(55));
            Assert.That(response.Concluido, Is.True);
        });
    }

    private static ConcluirServicoOrdemServicoCommand CreateCommand(Guid? servicoDaOrdemDeServicoId = null, int tempoGastoMinutos = 20) =>
        new()
        {
            ServicoDaOrdemDeServicoId = servicoDaOrdemDeServicoId ?? Guid.NewGuid(),
            TempoGastoMinutos = tempoGastoMinutos
        };

    private static OrdemServico CreateOrdemServico(bool emExecucao)
    {
        var ordemServicoResult = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Falha no sistema de injecao");

        Assert.Multiple(() =>
        {
            Assert.That(ordemServicoResult.IsSuccess, Is.True);
            Assert.That(ordemServicoResult.Value, Is.Not.Null);
        });

        var ordemServico = ordemServicoResult.Value!;

        if (emExecucao)
        {
            var iniciarDiagnosticoResult = ordemServico.IniciarDiagnostico();
            var aguardarAprovacaoResult = ordemServico.AguardarAprovacao();
            var aprovarOrcamentoResult = ordemServico.AprovarOrcamento(ordemServico.CodigoAprovacao);

            Assert.Multiple(() =>
            {
                Assert.That(iniciarDiagnosticoResult.IsSuccess, Is.True);
                Assert.That(aguardarAprovacaoResult.IsSuccess, Is.True);
                Assert.That(aprovarOrcamentoResult.IsSuccess, Is.True);
            });
        }

        return ordemServico;
    }

    private static ServicoDaOrdemDeServico CreateServicoDaOrdem(bool concluido = false, int? tempoGastoMinutos = null)
    {
        var servicoResult = Servico.Create("Troca de oleo", 120m);

        Assert.Multiple(() =>
        {
            Assert.That(servicoResult.IsSuccess, Is.True);
            Assert.That(servicoResult.Value, Is.Not.Null);
        });

        var servicoDaOrdemResult = concluido
            ? ServicoDaOrdemDeServico.Rehydrate(
                new ServicoDaOrdemDeServicoSnapshot(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    servicoResult.Value!.Id,
                    servicoResult.Value.Descricao,
                    servicoResult.Value.ValorUnitario,
                    1,
                    tempoGastoMinutos,
                    true))
            : ServicoDaOrdemDeServico.Create(Guid.NewGuid(), servicoResult.Value!, 1);

        Assert.Multiple(() =>
        {
            Assert.That(servicoDaOrdemResult.IsSuccess, Is.True);
            Assert.That(servicoDaOrdemResult.Value, Is.Not.Null);
        });

        return servicoDaOrdemResult.Value!;
    }
}

