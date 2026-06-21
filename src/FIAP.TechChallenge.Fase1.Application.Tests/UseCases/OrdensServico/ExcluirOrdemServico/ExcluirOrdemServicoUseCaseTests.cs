using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.ExcluirOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.ExcluirOrdemServico;

[TestFixture]
internal sealed class ExcluirOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldCallDelete_WhenOrdemServicoExists()
    {
        var repositoryMock = new Mock<IOrdemServicoRepository>();
        var ordemServico = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Falha no motor").Value!;

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(ordemServico.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Success(ordemServico));

        var useCase = new ExcluirOrdemServicoUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ExcluirOrdemServicoCommand { Id = ordemServico.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Id, Is.EqualTo(ordemServico.Id));
        });
        repositoryMock.Verify(x => x.DeleteAsync(ordemServico, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenOrdemServicoDoesNotExist()
    {
        var repositoryMock = new Mock<IOrdemServicoRepository>();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.", ErrorCode.NotFound)));

        var useCase = new ExcluirOrdemServicoUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ExcluirOrdemServicoCommand { Id = Guid.NewGuid() });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.NotFound));
        });
        repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

