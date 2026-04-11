using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Clientes.CriarCliente;

[TestFixture]
internal class CriarClienteUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenTelefoneIsInvalid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var useCase = new CriarClienteUseCase(repositoryMock.Object);
        var command = CreateCommand(telefone: "123");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        repositoryMock.Verify(x => x.ExistsByCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.ExistsByCnpjAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCpfIsInvalid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var useCase = new CriarClienteUseCase(repositoryMock.Object);
        var command = CreateCommand(cpf: "123");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        repositoryMock.Verify(x => x.ExistsByCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.ExistsByCnpjAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCpfAlreadyExists()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        _ = repositoryMock
            .Setup(x => x.ExistsByCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new CriarClienteUseCase(repositoryMock.Object);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Já existe um cliente cadastrado com este CPF."));
        });

        repositoryMock.Verify(x => x.ExistsByCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.ExistsByCnpjAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCnpjIsInvalid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var useCase = new CriarClienteUseCase(repositoryMock.Object);
        var command = CreateCommand(cpf: null, cnpj: "123");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        repositoryMock.Verify(x => x.ExistsByCnpjAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCnpjAlreadyExists()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        _ = repositoryMock
            .Setup(x => x.ExistsByCnpjAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = new CriarClienteUseCase(repositoryMock.Object);
        var command = CreateCommand(cpf: null, cnpj: "11444777000161");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Já existe um cliente cadastrado com este CNPJ."));
        });

        repositoryMock.Verify(x => x.ExistsByCnpjAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenEmailIsInvalid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var useCase = new CriarClienteUseCase(repositoryMock.Object);
        var command = CreateCommand(email: "email-invalido");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        Cliente? addedCliente = null;

        _ = repositoryMock
            .Setup(x => x.ExistsByCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _ = repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Callback<Cliente, CancellationToken>((cliente, _) => addedCliente = cliente)
            .Returns(Task.CompletedTask);

        var useCase = new CriarClienteUseCase(repositoryMock.Object);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(addedCliente, Is.Not.Null);
        });

        repositoryMock.Verify(x => x.ExistsByCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.ExistsByCnpjAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(addedCliente!.Id));
            Assert.That(response.Nome, Is.EqualTo("Cliente Teste"));
            Assert.That(response.Cpf, Is.EqualTo("529.982.247-25"));
            Assert.That(response.Cnpj, Is.Null);
            Assert.That(response.Telefone, Is.EqualTo("(11) 98765-4321"));
            Assert.That(response.Email, Is.EqualTo("cliente@exemplo.com"));
        });
    }

    private static CriarClienteCommand CreateCommand(
        string nome = "Cliente Teste",
        string telefone = "11987654321",
        string? cpf = "52998224725",
        string? cnpj = null,
        string? email = "cliente@exemplo.com") =>
        new()
        {
            Nome = nome,
            Telefone = telefone,
            Cpf = cpf,
            Cnpj = cnpj,
            Email = email
        };
}