using FIAP.TechChallenge.Fase1.Application.UseCases.OrdensServico.CriarOrdemServico;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.OrdensServico.CriarOrdemServico;

[TestFixture]
internal sealed class CriarOrdemServicoUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenClienteDoesNotExist()
    {
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Failure(new Error("Cliente nao encontrado.")));

        var useCase = new CriarOrdemServicoUseCase(ordemServicoRepositoryMock.Object, clienteRepositoryMock.Object, veiculoRepositoryMock.Object);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        ordemServicoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenVeiculoDoesNotExist()
    {
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var cliente = CreateCliente();

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Failure(new Error("Veiculo nao encontrado.")));

        var useCase = new CriarOrdemServicoUseCase(ordemServicoRepositoryMock.Object, clienteRepositoryMock.Object, veiculoRepositoryMock.Object);
        var command = CreateCommand(clienteId: cliente.Id);

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        ordemServicoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenVeiculoDoesNotBelongToCliente()
    {
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var cliente = CreateCliente();
        var outroClienteId = Guid.NewGuid();
        var veiculo = CreateVeiculo(outroClienteId);

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(veiculo));

        var useCase = new CriarOrdemServicoUseCase(ordemServicoRepositoryMock.Object, clienteRepositoryMock.Object, veiculoRepositoryMock.Object);
        var command = CreateCommand(clienteId: cliente.Id, veiculoId: veiculo.Id);

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O veiculo informado nao pertence ao cliente informado."));
        });

        ordemServicoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenDescricaoProblemaIsInvalid()
    {
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var cliente = CreateCliente();
        var veiculo = CreateVeiculo(cliente.Id);

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(veiculo));

        var useCase = new CriarOrdemServicoUseCase(ordemServicoRepositoryMock.Object, clienteRepositoryMock.Object, veiculoRepositoryMock.Object);
        var command = CreateCommand(clienteId: cliente.Id, veiculoId: veiculo.Id, descricaoProblema: " ");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        ordemServicoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var clienteRepositoryMock = new Mock<IClienteRepository>();
        var veiculoRepositoryMock = new Mock<IVeiculoRepository>();
        var ordemServicoRepositoryMock = new Mock<IOrdemServicoRepository>();
        var cliente = CreateCliente();
        var veiculo = CreateVeiculo(cliente.Id);
        OrdemServico? addedOrdemServico = null;
        var command = CreateCommand(
            clienteId: cliente.Id,
            veiculoId: veiculo.Id,
            descricaoProblema: "Barulho forte na suspensao dianteira.");

        _ = clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));
        _ = veiculoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Veiculo>.Success(veiculo));
        _ = ordemServicoRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()))
            .Callback<OrdemServico, CancellationToken>((ordemServico, _) => addedOrdemServico = ordemServico)
            .Returns(Task.CompletedTask);

        var useCase = new CriarOrdemServicoUseCase(ordemServicoRepositoryMock.Object, clienteRepositoryMock.Object, veiculoRepositoryMock.Object);

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(addedOrdemServico, Is.Not.Null);
        });

        clienteRepositoryMock.Verify(x => x.GetByIdAsync(cliente.Id, It.IsAny<CancellationToken>()), Times.Once);
        veiculoRepositoryMock.Verify(x => x.GetByIdAsync(veiculo.Id, It.IsAny<CancellationToken>()), Times.Once);
        ordemServicoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(addedOrdemServico!.Id));
            Assert.That(response.CodigoAprovacao, Is.EqualTo(addedOrdemServico.CodigoAprovacao));
            Assert.That(response.ClienteId, Is.EqualTo(command.ClienteId));
            Assert.That(response.VeiculoId, Is.EqualTo(command.VeiculoId));
            Assert.That(response.DescricaoProblema, Is.EqualTo("Barulho forte na suspensao dianteira."));
            Assert.That(response.Status, Is.EqualTo(StatusOrdemServico.Recebida));
            Assert.That(response.DataCriacao, Is.Not.EqualTo(default(DateTime)));
        });
    }

    private static CriarOrdemServicoCommand CreateCommand(
        Guid? clienteId = null,
        Guid? veiculoId = null,
        string descricaoProblema = "Motor com falha intermitente") =>
        new()
        {
            ClienteId = clienteId ?? Guid.NewGuid(),
            VeiculoId = veiculoId ?? Guid.NewGuid(),
            DescricaoProblema = descricaoProblema
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

    private static Veiculo CreateVeiculo(Guid clienteId)
    {
        var placaResult = Placa.Create("ABC1234");
        var veiculoResult = Veiculo.Create(clienteId, placaResult.Value!, "Toyota", "Corolla", 2023);

        Assert.Multiple(() =>
        {
            Assert.That(placaResult.IsSuccess, Is.True);
            Assert.That(veiculoResult.IsSuccess, Is.True);
            Assert.That(veiculoResult.Value, Is.Not.Null);
        });

        return veiculoResult.Value!;
    }
}

