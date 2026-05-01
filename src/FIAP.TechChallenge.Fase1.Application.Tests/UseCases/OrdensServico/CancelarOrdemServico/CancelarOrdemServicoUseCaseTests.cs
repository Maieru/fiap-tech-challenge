using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CancelarOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.CancelarOrdemServico;

[TestFixture]
internal sealed class CancelarOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var pecaInsumoRepositoryMock = new Mock<IPecaInsumoRepository>();
        var pecaOuInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.")));

        var useCase = new CancelarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            pecaInsumoRepositoryMock.Object,
            pecaOuInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
        });

        pecaOuInsumoDaOrdemRepositoryMock.Verify(x => x.GetByOrdemServicoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        pecaInsumoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()), Times.Never);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIsNotAguardandoAprovacao()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var pecaInsumoRepositoryMock = new Mock<IPecaInsumoRepository>();
        var pecaOuInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico(aguardandoAprovacao: false);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        var useCase = new CancelarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            pecaInsumoRepositoryMock.Object,
            pecaOuInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Does.Contain("aguardando aprovação"));
            Assert.That(ordemServico.Status, Is.EqualTo(StatusOrdemServico.Recebida));
        });

        pecaOuInsumoDaOrdemRepositoryMock.Verify(x => x.GetByOrdemServicoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        pecaInsumoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()), Times.Never);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_AndReturnReservedStock_WhenOrdemServicoIsAguardandoAprovacao()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var pecaInsumoRepositoryMock = new Mock<IPecaInsumoRepository>();
        var pecaOuInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico(aguardandoAprovacao: true);
        var pecaInsumo = CreatePecaInsumo(quantidadeEstoque: 7);
        var pecaOuInsumoDaOrdem = CreatePecaOuInsumoDaOrdem(ordemServico.Id, pecaInsumo, quantidade: 3);
        OrdemServico? ordemServicoAtualizada = null;
        PecaInsumo? pecaInsumoAtualizada = null;

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = pecaOuInsumoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>>.Success([pecaOuInsumoDaOrdem]));
        _ = pecaInsumoRepositoryMock
            .Setup(x => x.GetByIdAsync(pecaInsumo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PecaInsumo>.Success(pecaInsumo));
        _ = pecaInsumoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()))
            .Callback<PecaInsumo, CancellationToken>((item, _) => pecaInsumoAtualizada = item)
            .Returns(Task.CompletedTask);
        _ = ordemServicoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()))
            .Callback<OrdemServico, CancellationToken>((item, _) => ordemServicoAtualizada = item)
            .Returns(Task.CompletedTask);

        var useCase = new CancelarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            pecaInsumoRepositoryMock.Object,
            pecaOuInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(ordemServicoAtualizada, Is.Not.Null);
            Assert.That(pecaInsumoAtualizada, Is.Not.Null);
            Assert.That(pecaInsumoAtualizada!.QuantidadeEstoque, Is.EqualTo(10));
        });

        pecaInsumoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()), Times.Once);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(ordemServico.Id));
            Assert.That(response.Status, Is.EqualTo(StatusOrdemServico.Cancelada));
            Assert.That(ordemServico.Status, Is.EqualTo(StatusOrdemServico.Cancelada));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenReservedPecaInsumoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var pecaInsumoRepositoryMock = new Mock<IPecaInsumoRepository>();
        var pecaOuInsumoDaOrdemRepositoryMock = new Mock<IPecaOuInsumoDaOrdemDeServicoRepository>();
        var ordemServico = CreateOrdemServico(aguardandoAprovacao: true);
        var pecaInsumo = CreatePecaInsumo();
        var pecaOuInsumoDaOrdem = CreatePecaOuInsumoDaOrdem(ordemServico.Id, pecaInsumo);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = pecaOuInsumoDaOrdemRepositoryMock
            .Setup(x => x.GetByOrdemServicoIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyCollection<PecaOuInsumoDaOrdemDeServico>>.Success([pecaOuInsumoDaOrdem]));
        _ = pecaInsumoRepositoryMock
            .Setup(x => x.GetByIdAsync(pecaInsumo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PecaInsumo>.Failure(new Error("Peca ou insumo nao encontrado.")));

        var useCase = new CancelarOrdemServicoUseCase(
            ordemServicoRepositoryMock.Object,
            pecaInsumoRepositoryMock.Object,
            pecaOuInsumoDaOrdemRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Peca ou insumo nao encontrado."));
        });

        pecaInsumoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PecaInsumo>(), It.IsAny<CancellationToken>()), Times.Never);
        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static CancelarOrdemServicoCommand CreateCommand(Guid? ordemServicoId = null) =>
        new()
        {
            OrdemServicoId = ordemServicoId ?? Guid.NewGuid()
        };

    private static OrdemServico CreateOrdemServico(bool aguardandoAprovacao)
    {
        var ordemServicoResult = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Falha no sistema de injecao");

        Assert.Multiple(() =>
        {
            Assert.That(ordemServicoResult.IsSuccess, Is.True);
            Assert.That(ordemServicoResult.Value, Is.Not.Null);
        });

        var ordemServico = ordemServicoResult.Value!;

        if (aguardandoAprovacao)
        {
            var iniciarDiagnosticoResult = ordemServico.IniciarDiagnostico();
            var aguardarAprovacaoResult = ordemServico.AguardarAprovacao();

            Assert.Multiple(() =>
            {
                Assert.That(iniciarDiagnosticoResult.IsSuccess, Is.True);
                Assert.That(aguardarAprovacaoResult.IsSuccess, Is.True);
            });
        }

        return ordemServico;
    }

    private static PecaInsumo CreatePecaInsumo(int quantidadeEstoque = 30)
    {
        var pecaInsumoResult = PecaInsumo.Create("Pastilha de freio", "PST-001", "Pastilha dianteira", 180m, quantidadeEstoque);

        Assert.Multiple(() =>
        {
            Assert.That(pecaInsumoResult.IsSuccess, Is.True);
            Assert.That(pecaInsumoResult.Value, Is.Not.Null);
        });

        return pecaInsumoResult.Value!;
    }

    private static PecaOuInsumoDaOrdemDeServico CreatePecaOuInsumoDaOrdem(Guid ordemServicoId, PecaInsumo pecaInsumo, int quantidade = 1)
    {
        var pecaOuInsumoDaOrdemResult = PecaOuInsumoDaOrdemDeServico.Create(ordemServicoId, pecaInsumo, quantidade);

        Assert.Multiple(() =>
        {
            Assert.That(pecaOuInsumoDaOrdemResult.IsSuccess, Is.True);
            Assert.That(pecaOuInsumoDaOrdemResult.Value, Is.Not.Null);
        });

        return pecaOuInsumoDaOrdemResult.Value!;
    }
}
