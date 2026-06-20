using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;
using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServicoComClienteEVeiculo;
using FIAP.TechChallenge.Fase1.Application.UseCases.Veiculos.CriarVeiculo;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.CriarOrdemServicoComClienteEVeiculo;

[TestFixture]
internal sealed class CriarOrdemServicoComClienteEVeiculoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCriarClienteFails()
    {
        var criarClienteUseCaseMock = new Mock<ICriarClienteUseCase>();
        var criarVeiculoUseCaseMock = new Mock<ICriarVeiculoUseCase>();
        var criarOrdemServicoUseCaseMock = new Mock<ICriarOrdemServicoUseCase>();
        var expectedError = new Error("Telefone invalido.");

        _ = criarClienteUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CriarClienteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CriarClienteResponse>.Failure(expectedError));

        var useCase = new CriarOrdemServicoComClienteEVeiculoUseCase(
            criarClienteUseCaseMock.Object,
            criarVeiculoUseCaseMock.Object,
            criarOrdemServicoUseCaseMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.EqualTo(expectedError));
        });

        criarClienteUseCaseMock.Verify(x => x.ExecuteAsync(It.IsAny<CriarClienteCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        criarVeiculoUseCaseMock.Verify(x => x.ExecuteAsync(It.IsAny<CriarVeiculoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        criarOrdemServicoUseCaseMock.Verify(x => x.ExecuteAsync(It.IsAny<CriarOrdemServicoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCriarVeiculoFails()
    {
        var criarClienteUseCaseMock = new Mock<ICriarClienteUseCase>();
        var criarVeiculoUseCaseMock = new Mock<ICriarVeiculoUseCase>();
        var criarOrdemServicoUseCaseMock = new Mock<ICriarOrdemServicoUseCase>();
        var clienteId = Guid.NewGuid();
        var expectedError = new Error("Ja existe um veiculo cadastrado com esta placa.");

        _ = criarClienteUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CriarClienteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CriarClienteResponse>.Success(new CriarClienteResponse { Id = clienteId }));
        _ = criarVeiculoUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CriarVeiculoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CriarVeiculoResponse>.Failure(expectedError));

        var useCase = new CriarOrdemServicoComClienteEVeiculoUseCase(
            criarClienteUseCaseMock.Object,
            criarVeiculoUseCaseMock.Object,
            criarOrdemServicoUseCaseMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.EqualTo(expectedError));
        });

        criarVeiculoUseCaseMock.Verify(x => x.ExecuteAsync(
            It.Is<CriarVeiculoCommand>(command => command.ClienteId == clienteId),
            It.IsAny<CancellationToken>()), Times.Once);
        criarOrdemServicoUseCaseMock.Verify(x => x.ExecuteAsync(It.IsAny<CriarOrdemServicoCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCriarOrdemServicoFails()
    {
        var criarClienteUseCaseMock = new Mock<ICriarClienteUseCase>();
        var criarVeiculoUseCaseMock = new Mock<ICriarVeiculoUseCase>();
        var criarOrdemServicoUseCaseMock = new Mock<ICriarOrdemServicoUseCase>();
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var expectedError = new Error("A descricao do problema e obrigatoria.");

        _ = criarClienteUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CriarClienteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CriarClienteResponse>.Success(new CriarClienteResponse { Id = clienteId }));
        _ = criarVeiculoUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CriarVeiculoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CriarVeiculoResponse>.Success(new CriarVeiculoResponse { Id = veiculoId, ClienteId = clienteId }));
        _ = criarOrdemServicoUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CriarOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CriarOrdemServicoResponse>.Failure(expectedError));

        var useCase = new CriarOrdemServicoComClienteEVeiculoUseCase(
            criarClienteUseCaseMock.Object,
            criarVeiculoUseCaseMock.Object,
            criarOrdemServicoUseCaseMock.Object);

        var result = await useCase.ExecuteAsync(CreateCommand(descricaoProblema: " "));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.EqualTo(expectedError));
        });

        criarOrdemServicoUseCaseMock.Verify(x => x.ExecuteAsync(
            It.Is<CriarOrdemServicoCommand>(command => command.ClienteId == clienteId && command.VeiculoId == veiculoId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var criarClienteUseCaseMock = new Mock<ICriarClienteUseCase>();
        var criarVeiculoUseCaseMock = new Mock<ICriarVeiculoUseCase>();
        var criarOrdemServicoUseCaseMock = new Mock<ICriarOrdemServicoUseCase>();
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ordemServicoId = Guid.NewGuid();
        var command = CreateCommand();
        CriarVeiculoCommand? criarVeiculoCommand = null;
        CriarOrdemServicoCommand? criarOrdemServicoCommand = null;

        _ = criarClienteUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CriarClienteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CriarClienteResponse>.Success(new CriarClienteResponse { Id = clienteId }));
        _ = criarVeiculoUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CriarVeiculoCommand>(), It.IsAny<CancellationToken>()))
            .Callback<CriarVeiculoCommand, CancellationToken>((receivedCommand, _) => criarVeiculoCommand = receivedCommand)
            .ReturnsAsync(Result<CriarVeiculoResponse>.Success(new CriarVeiculoResponse { Id = veiculoId, ClienteId = clienteId }));
        _ = criarOrdemServicoUseCaseMock
            .Setup(x => x.ExecuteAsync(It.IsAny<CriarOrdemServicoCommand>(), It.IsAny<CancellationToken>()))
            .Callback<CriarOrdemServicoCommand, CancellationToken>((receivedCommand, _) => criarOrdemServicoCommand = receivedCommand)
            .ReturnsAsync(Result<CriarOrdemServicoResponse>.Success(new CriarOrdemServicoResponse
            {
                Id = ordemServicoId,
                ClienteId = clienteId,
                VeiculoId = veiculoId,
                DescricaoProblema = command.DescricaoProblema,
                Status = StatusOrdemServico.Recebida,
                DataCriacao = DateTime.UtcNow
            }));

        var useCase = new CriarOrdemServicoComClienteEVeiculoUseCase(
            criarClienteUseCaseMock.Object,
            criarVeiculoUseCaseMock.Object,
            criarOrdemServicoUseCaseMock.Object);

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(ordemServicoId));
            Assert.That(result.Value.ClienteId, Is.EqualTo(clienteId));
            Assert.That(result.Value.VeiculoId, Is.EqualTo(veiculoId));
            Assert.That(criarVeiculoCommand, Is.Not.Null);
            Assert.That(criarVeiculoCommand!.ClienteId, Is.EqualTo(clienteId));
            Assert.That(criarVeiculoCommand.Placa, Is.EqualTo(command.Veiculo.Placa));
            Assert.That(criarVeiculoCommand.Marca, Is.EqualTo(command.Veiculo.Marca));
            Assert.That(criarVeiculoCommand.Modelo, Is.EqualTo(command.Veiculo.Modelo));
            Assert.That(criarVeiculoCommand.Ano, Is.EqualTo(command.Veiculo.Ano));
            Assert.That(criarOrdemServicoCommand, Is.Not.Null);
            Assert.That(criarOrdemServicoCommand!.ClienteId, Is.EqualTo(clienteId));
            Assert.That(criarOrdemServicoCommand.VeiculoId, Is.EqualTo(veiculoId));
            Assert.That(criarOrdemServicoCommand.DescricaoProblema, Is.EqualTo(command.DescricaoProblema));
        });
    }

    private static CriarOrdemServicoComClienteEVeiculoCommand CreateCommand(string descricaoProblema = "Motor falhando ao ligar") =>
        new()
        {
            Cliente = new CriarClienteCommand
            {
                Nome = "Cliente Teste",
                Telefone = "11987654321",
                Cpf = "52998224725",
                Email = "cliente@exemplo.com"
            },
            Veiculo = new CriarVeiculoOrdemServicoCommand
            {
                Placa = "ABC1234",
                Marca = "Toyota",
                Modelo = "Corolla",
                Ano = 2023
            },
            DescricaoProblema = descricaoProblema
        };
}
