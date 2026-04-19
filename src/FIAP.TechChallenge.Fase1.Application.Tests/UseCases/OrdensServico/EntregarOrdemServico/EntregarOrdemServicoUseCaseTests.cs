using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.EntregarOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.EntregarOrdemServico;

[TestFixture]
internal sealed class EntregarOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.")));

        var useCase = new EntregarOrdemServicoUseCase(ordemServicoRepositoryMock.Object);
        var result = await useCase.ExecuteAsync(CreateCommand());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
        });

        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIsNotFinalizada()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var ordemServico = CreateOrdemServico(finalizada: false);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        var useCase = new EntregarOrdemServicoUseCase(ordemServicoRepositoryMock.Object);
        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Does.Contain("finalizadas"));
        });

        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenOrdemServicoIsFinalizada()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var ordemServico = CreateOrdemServico(finalizada: true);
        OrdemServico? ordemServicoAtualizada = null;

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()))
            .Callback<OrdemServico, CancellationToken>((item, _) => ordemServicoAtualizada = item)
            .Returns(Task.CompletedTask);

        var useCase = new EntregarOrdemServicoUseCase(ordemServicoRepositoryMock.Object);
        var result = await useCase.ExecuteAsync(CreateCommand(ordemServico.Id));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(ordemServicoAtualizada, Is.Not.Null);
        });

        ordemServicoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(ordemServico.Id));
            Assert.That(response.Status, Is.EqualTo(StatusOrdemServico.Entregue));
            Assert.That(response.DataEntrega, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-5)));
        });
    }

    private static EntregarOrdemServicoCommand CreateCommand(Guid? ordemServicoId = null) =>
        new()
        {
            OrdemServicoId = ordemServicoId ?? Guid.NewGuid()
        };

    private static OrdemServico CreateOrdemServico(bool finalizada)
    {
        var ordemServicoResult = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Falha no sistema de injecao");

        Assert.Multiple(() =>
        {
            Assert.That(ordemServicoResult.IsSuccess, Is.True);
            Assert.That(ordemServicoResult.Value, Is.Not.Null);
        });

        var ordemServico = ordemServicoResult.Value!;

        if (finalizada)
        {
            var iniciarDiagnosticoResult = ordemServico.IniciarDiagnostico();
            var aguardarAprovacaoResult = ordemServico.AguardarAprovacao();
            var aprovarOrcamentoResult = ordemServico.AprovarOrcamento();
            var finalizarResult = ordemServico.Finalizar([]);

            Assert.Multiple(() =>
            {
                Assert.That(iniciarDiagnosticoResult.IsSuccess, Is.True);
                Assert.That(aguardarAprovacaoResult.IsSuccess, Is.True);
                Assert.That(aprovarOrcamentoResult.IsSuccess, Is.True);
                Assert.That(finalizarResult.IsSuccess, Is.True);
            });
        }

        return ordemServico;
    }
}

