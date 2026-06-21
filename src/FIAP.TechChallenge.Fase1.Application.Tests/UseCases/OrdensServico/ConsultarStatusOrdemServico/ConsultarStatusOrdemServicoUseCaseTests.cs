using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ConsultarStatusOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.ConsultarStatusOrdemServico;

[TestFixture]
internal sealed class ConsultarStatusOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoIdIsEmpty()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var useCase = new ConsultarStatusOrdemServicoUseCase(ordemServicoRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ConsultarStatusOrdemServicoCommand { OrdemServicoId = Guid.Empty });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O identificador da ordem de servico deve ser valido."));
        });

        ordemServicoRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenOrdemServicoDoesNotExist()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var ordemServicoId = Guid.NewGuid();

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(ordemServicoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.", ErrorCode.NotFound)));

        var useCase = new ConsultarStatusOrdemServicoUseCase(ordemServicoRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ConsultarStatusOrdemServicoCommand { OrdemServicoId = ordemServicoId });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.NotFound));
        });

        ordemServicoRepositoryMock.Verify(x => x.GetByIdAsync(ordemServicoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenOrdemServicoExists()
    {
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var ordemServico = CreateOrdemServico(StatusOrdemServico.EmDiagnostico);

        _ = ordemServicoRepositoryMock
            .Setup(x => x.GetByIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        var useCase = new ConsultarStatusOrdemServicoUseCase(ordemServicoRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ConsultarStatusOrdemServicoCommand { OrdemServicoId = ordemServico.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(ordemServico.Id));
            Assert.That(result.Value.Status, Is.EqualTo(StatusOrdemServico.EmDiagnostico));
        });

        ordemServicoRepositoryMock.Verify(x => x.GetByIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static OrdemServico CreateOrdemServico(StatusOrdemServico status)
    {
        var ordemServicoResult = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Ruido no sistema de direcao");

        Assert.Multiple(() =>
        {
            Assert.That(ordemServicoResult.IsSuccess, Is.True);
            Assert.That(ordemServicoResult.Value, Is.Not.Null);
        });

        var ordemServico = ordemServicoResult.Value!;

        if (status == StatusOrdemServico.EmDiagnostico)
        {
            var iniciarDiagnosticoResult = ordemServico.IniciarDiagnostico();

            Assert.Multiple(() =>
            {
                Assert.That(iniciarDiagnosticoResult.IsSuccess, Is.True);
                Assert.That(iniciarDiagnosticoResult.Value, Is.True);
            });
        }

        return ordemServico;
    }
}

