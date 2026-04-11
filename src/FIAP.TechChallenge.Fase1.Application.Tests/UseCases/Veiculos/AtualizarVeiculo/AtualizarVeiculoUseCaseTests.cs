using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.AtualizarVeiculo;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Veiculos.AtualizarVeiculo;

[TestFixture]
internal sealed class AtualizarVeiculoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenVeiculoIsNotFound()
    {
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Failure(new Error("Veículo não encontrado.")));

        var useCase = new AtualizarVeiculoUseCase(veiculoRepositoryMock.Object);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Veículo não encontrado."));
        });

        veiculoRepositoryMock.Verify(x => x.ExistsByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        veiculoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenPlacaIsInvalid()
    {
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(CreateVeiculo()));

        var useCase = new AtualizarVeiculoUseCase(veiculoRepositoryMock.Object);
        var command = CreateCommand(placa: "123");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        veiculoRepositoryMock.Verify(x => x.ExistsByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        veiculoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenPlacaAlreadyExists()
    {
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var command = CreateCommand(placa: "BRA2E19");

        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(CreateVeiculo(id: command.Id, placa: "ABC1234")));
        _ = veiculoRepositoryMock
            .Setup(x => x.ExistsByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new AtualizarVeiculoUseCase(veiculoRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ja existe um veiculo cadastrado com esta placa."));
        });

        veiculoRepositoryMock.Verify(x => x.ExistsByPlacaAsync("BRA2E19", It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenMarcaIsInvalid()
    {
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(CreateVeiculo()));

        var useCase = new AtualizarVeiculoUseCase(veiculoRepositoryMock.Object);
        var command = CreateCommand(marca: " ");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description.Contains("marca", StringComparison.OrdinalIgnoreCase), Is.True);
        });

        veiculoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenAnoIsInvalid()
    {
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(CreateVeiculo()));

        var useCase = new AtualizarVeiculoUseCase(veiculoRepositoryMock.Object);
        var command = CreateCommand(ano: DateTime.UtcNow.Year + 2);

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description.Contains("ano", StringComparison.OrdinalIgnoreCase), Is.True);
        });

        veiculoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        Veiculo? updatedVeiculo = null;
        var command = CreateCommand(placa: "BRA2E19", marca: "Honda", modelo: "Civic", ano: 2024);

        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(CreateVeiculo(id: command.Id, placa: "ABC1234")));
        _ = veiculoRepositoryMock
            .Setup(x => x.ExistsByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _ = veiculoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()))
            .Callback<Veiculo, CancellationToken>((veiculo, _) => updatedVeiculo = veiculo)
            .Returns(Task.CompletedTask);

        var useCase = new AtualizarVeiculoUseCase(veiculoRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(updatedVeiculo, Is.Not.Null);
        });

        veiculoRepositoryMock.Verify(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.ExistsByPlacaAsync("BRA2E19", It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(command.Id));
            Assert.That(response.ClienteId, Is.EqualTo(updatedVeiculo!.ClienteId));
            Assert.That(response.Placa, Is.EqualTo("BRA2E19"));
            Assert.That(response.Marca, Is.EqualTo("Honda"));
            Assert.That(response.Modelo, Is.EqualTo("Civic"));
            Assert.That(response.Ano, Is.EqualTo(2024));
        });
    }

    private static AtualizarVeiculoCommand CreateCommand(
        Guid? id = null,
        string placa = "ABC1234",
        string marca = "Toyota",
        string modelo = "Corolla",
        int ano = 2023) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Placa = placa,
            Marca = marca,
            Modelo = modelo,
            Ano = ano
        };

    private static Veiculo CreateVeiculo(
        Guid? id = null,
        string placa = "ABC1234",
        string marca = "Toyota",
        string modelo = "Corolla",
        int ano = 2023)
    {
        var placaResult = Placa.Create(placa);

        Assert.Multiple(() =>
        {
            Assert.That(placaResult.IsSuccess, Is.True);
            Assert.That(placaResult.Value, Is.Not.Null);
        });

        var veiculoResult = Veiculo.Rehydrate(id ?? Guid.NewGuid(), Guid.NewGuid(), placaResult.Value!, marca, modelo, ano);

        Assert.Multiple(() =>
        {
            Assert.That(veiculoResult.IsSuccess, Is.True);
            Assert.That(veiculoResult.Value, Is.Not.Null);
        });

        return veiculoResult.Value!;
    }
}
