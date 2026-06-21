using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ExcluirVeiculo;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Veiculos.ExcluirVeiculo;

[TestFixture]
internal sealed class ExcluirVeiculoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldCallDelete_WhenVeiculoExists()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();
        var veiculo = CreateVeiculo();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(veiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(veiculo));

        var useCase = new ExcluirVeiculoUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ExcluirVeiculoCommand { Id = veiculo.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Id, Is.EqualTo(veiculo.Id));
        });
        repositoryMock.Verify(x => x.DeleteAsync(veiculo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenVeiculoDoesNotExist()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Failure(new Error("Veiculo nao encontrado.", ErrorCode.NotFound)));

        var useCase = new ExcluirVeiculoUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ExcluirVeiculoCommand { Id = Guid.NewGuid() });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.NotFound));
        });
        repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Veiculo CreateVeiculo()
    {
        var placa = Placa.Create("ABC1234").Value!;
        return Veiculo.Create(Guid.NewGuid(), placa, "Toyota", "Corolla", 2024).Value!;
    }
}

