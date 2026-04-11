using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.CriarVeiculo;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Veiculos.CriarVeiculo;

[TestFixture]
internal sealed class CriarVeiculoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenPlacaIsInvalid()
    {
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var useCase = new CriarVeiculoUseCase(veiculoRepositoryMock.Object, clienteRepositoryMock.Object);
        var command = CreateCommand(placa: "123");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        veiculoRepositoryMock.Verify(x => x.ExistsByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        veiculoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenClienteDoesNotExist()
    {
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Failure(new Error("Cliente não encontrado.")));

        var useCase = new CriarVeiculoUseCase(veiculoRepositoryMock.Object, clienteRepositoryMock.Object);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description.Contains("Cliente", StringComparison.OrdinalIgnoreCase), Is.True);
            Assert.That(result.Error.Description.Contains("encontrado", StringComparison.OrdinalIgnoreCase), Is.True);
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.ExistsByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        veiculoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenPlacaAlreadyExists()
    {
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var cliente = CreateCliente();

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = veiculoRepositoryMock
            .Setup(x => x.ExistsByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new CriarVeiculoUseCase(veiculoRepositoryMock.Object, clienteRepositoryMock.Object);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ja existe um veiculo cadastrado com esta placa."));
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.ExistsByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenVeiculoDataIsInvalid()
    {
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var cliente = CreateCliente();

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = veiculoRepositoryMock
            .Setup(x => x.ExistsByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = new CriarVeiculoUseCase(veiculoRepositoryMock.Object, clienteRepositoryMock.Object);
        var command = CreateCommand(marca: " ");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.ExistsByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var cliente = CreateCliente();
        Veiculo? addedVeiculo = null;
        var command = CreateCommand(clienteId: cliente.Id, placa: "ABC-1234");

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = veiculoRepositoryMock
            .Setup(x => x.ExistsByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _ = veiculoRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()))
            .Callback<Veiculo, CancellationToken>((veiculo, _) => addedVeiculo = veiculo)
            .Returns(Task.CompletedTask);

        var useCase = new CriarVeiculoUseCase(veiculoRepositoryMock.Object, clienteRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(addedVeiculo, Is.Not.Null);
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.ExistsByPlacaAsync("ABC1234", It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(addedVeiculo!.Id));
            Assert.That(response.ClienteId, Is.EqualTo(command.ClienteId));
            Assert.That(response.Placa, Is.EqualTo("ABC1234"));
            Assert.That(response.Marca, Is.EqualTo("Toyota"));
            Assert.That(response.Modelo, Is.EqualTo("Corolla"));
            Assert.That(response.Ano, Is.EqualTo(2023));
        });
    }

    private static CriarVeiculoCommand CreateCommand(
        Guid? clienteId = null,
        string placa = "ABC1234",
        string marca = "Toyota",
        string modelo = "Corolla",
        int ano = 2023) =>
        new()
        {
            ClienteId = clienteId ?? Guid.NewGuid(),
            Placa = placa,
            Marca = marca,
            Modelo = modelo,
            Ano = ano
        };

    private static Cliente CreateCliente()
    {
        var telefoneResult = Telefone.Create("11987654321");
        var cpfResult = Cpf.Create("52998224725");
        var clienteResult = Cliente.Create("Cliente Teste", cpfResult.Value, null, telefoneResult.Value!, null);

        Assert.Multiple(() =>
        {
            Assert.That(telefoneResult.IsSuccess, Is.True);
            Assert.That(cpfResult.IsSuccess, Is.True);
            Assert.That(clienteResult.IsSuccess, Is.True);
            Assert.That(clienteResult.Value, Is.Not.Null);
        });

        return clienteResult.Value!;
    }
}
