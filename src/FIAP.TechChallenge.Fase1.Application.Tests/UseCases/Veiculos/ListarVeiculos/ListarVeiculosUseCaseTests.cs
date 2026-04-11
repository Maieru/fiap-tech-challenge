using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.ListarVeiculos;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Veiculos.ListarVeiculos;

[TestFixture]
internal sealed class ListarVeiculosUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenPaginationIsInvalid()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();
        var useCase = new ListarVeiculosUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarVeiculosCommand { PageNumber = 0, PageSize = 10 });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O número da página deve ser maior que zero."));
        });

        repositoryMock.Verify(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenMoreThanOneFilterIsInformed()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();
        var useCase = new ListarVeiculosUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarVeiculosCommand { Id = Guid.NewGuid(), Placa = "ABC1234" });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Informe apenas um filtro por vez: id, placa ou clienteId."));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenListingPagedVehicles()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();
        var veiculo1 = CreateVeiculo(placa: "ABC1234");
        var veiculo2 = CreateVeiculo(placa: "BRA2E19");

        _ = repositoryMock
            .Setup(x => x.GetPagedAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<(IReadOnlyCollection<Veiculo> Veiculos, int TotalItems)>.Success((new[] { veiculo1, veiculo2 }, 2)));

        var useCase = new ListarVeiculosUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarVeiculosCommand { PageNumber = 1, PageSize = 2 });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.TotalItems, Is.EqualTo(2));
            Assert.That(result.Value.PageNumber, Is.EqualTo(1));
            Assert.That(result.Value.PageSize, Is.EqualTo(2));
            Assert.That(result.Value.Veiculos, Has.Count.EqualTo(2));
        });

        repositoryMock.Verify(x => x.GetPagedAsync(1, 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenGettingByPlaca()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();
        var veiculo = CreateVeiculo(placa: "ABC1234");

        _ = repositoryMock
            .Setup(x => x.GetByPlacaAsync("ABC1234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(veiculo));

        var useCase = new ListarVeiculosUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarVeiculosCommand { Placa = "ABC-1234" });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Veiculos, Has.Count.EqualTo(1));
            Assert.That(result.Value.Veiculos.First().Placa, Is.EqualTo("ABC1234"));
        });

        repositoryMock.Verify(x => x.GetByPlacaAsync("ABC1234", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenGettingById()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();
        var veiculo = CreateVeiculo();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(veiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(veiculo));

        var useCase = new ListarVeiculosUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarVeiculosCommand { Id = veiculo.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Veiculos, Has.Count.EqualTo(1));
            Assert.That(result.Value.Veiculos.First().Id, Is.EqualTo(veiculo.Id));
        });

        repositoryMock.Verify(x => x.GetByIdAsync(veiculo.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenPlacaIsInvalid()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();
        var useCase = new ListarVeiculosUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarVeiculosCommand { Placa = "123" });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        repositoryMock.Verify(x => x.GetByPlacaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenPlacaDoesNotExist()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();

        _ = repositoryMock
            .Setup(x => x.GetByPlacaAsync("ABC1234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Failure(new Error("VeÃ­culo nÃ£o encontrado.")));

        var useCase = new ListarVeiculosUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarVeiculosCommand { Placa = "ABC1234" });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("VeÃ­culo nÃ£o encontrado."));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenListingByClienteId()
    {
        var repositoryMock = new Mock<IVeiculoRepository>();
        var clienteId = Guid.NewGuid();
        var veiculo1 = CreateVeiculo(clienteId: clienteId, placa: "ABC1234");
        var veiculo2 = CreateVeiculo(clienteId: clienteId, placa: "BRA2E19");

        _ = repositoryMock
            .Setup(x => x.GetByClienteIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<(IReadOnlyCollection<Veiculo> Veiculos, int TotalItems)>.Success((new[] { veiculo1, veiculo2 }, 2)));

        var useCase = new ListarVeiculosUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarVeiculosCommand { ClienteId = clienteId });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.TotalItems, Is.EqualTo(2));
            Assert.That(result.Value.Veiculos, Has.Count.EqualTo(2));
            Assert.That(result.Value.Veiculos.All(x => x.ClienteId == clienteId), Is.True);
        });

        repositoryMock.Verify(x => x.GetByClienteIdAsync(clienteId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Veiculo CreateVeiculo(
        Guid? id = null,
        Guid? clienteId = null,
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

        var veiculoResult = Veiculo.Rehydrate(id ?? Guid.NewGuid(), clienteId ?? Guid.NewGuid(), placaResult.Value!, marca, modelo, ano);

        Assert.Multiple(() =>
        {
            Assert.That(veiculoResult.IsSuccess, Is.True);
            Assert.That(veiculoResult.Value, Is.Not.Null);
        });

        return veiculoResult.Value!;
    }
}
