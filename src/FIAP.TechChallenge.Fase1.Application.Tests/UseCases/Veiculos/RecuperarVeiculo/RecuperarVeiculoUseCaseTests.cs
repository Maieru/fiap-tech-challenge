using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.RecuperarVeiculo;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Veiculos.RecuperarVeiculo;

[TestFixture]
internal sealed class RecuperarVeiculoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenVeiculoIdIsEmpty()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();
        var useCase = new RecuperarVeiculoUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new RecuperarVeiculoCommand { VeiculoId = Guid.Empty });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O identificador do veiculo deve ser valido."));
        });

        repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenVeiculoDoesNotExist()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Failure(new Error("Veiculo não encontrado.")));

        var useCase = new RecuperarVeiculoUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new RecuperarVeiculoCommand { VeiculoId = Guid.NewGuid() });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Veiculo não encontrado."));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenVeiculoExists()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();
        var veiculo = CreateVeiculo();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(veiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(veiculo));

        var useCase = new RecuperarVeiculoUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new RecuperarVeiculoCommand { VeiculoId = veiculo.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(veiculo.Id));
            Assert.That(result.Value.ClienteId, Is.EqualTo(veiculo.ClienteId));
            Assert.That(result.Value.Placa, Is.EqualTo("ABC1234"));
            Assert.That(result.Value.Marca, Is.EqualTo("Toyota"));
            Assert.That(result.Value.Modelo, Is.EqualTo("Corolla"));
            Assert.That(result.Value.Ano, Is.EqualTo(2023));
        });
    }

    private static Veiculo CreateVeiculo()
    {
        var placaResult = Placa.Create("ABC1234");

        Assert.Multiple(() =>
        {
            Assert.That(placaResult.IsSuccess, Is.True);
            Assert.That(placaResult.Value, Is.Not.Null);
        });

        var veiculoResult = Veiculo.Rehydrate(Guid.NewGuid(), Guid.NewGuid(), placaResult.Value!, "Toyota", "Corolla", 2023);

        Assert.Multiple(() =>
        {
            Assert.That(veiculoResult.IsSuccess, Is.True);
            Assert.That(veiculoResult.Value, Is.Not.Null);
        });

        return veiculoResult.Value!;
    }
}

