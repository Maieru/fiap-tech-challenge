using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.AtualizarCliente;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Clientes.AtualizarCliente;

[TestFixture]
internal class AtualizarClienteUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenClienteIsNotFound()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Failure(new Error("Cliente não encontrado.")));

        var useCase = new AtualizarClienteUseCase(repositoryMock.Object);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Cliente não encontrado."));
        });

        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenTelefoneIsInvalid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(CreateCliente()));

        var useCase = new AtualizarClienteUseCase(repositoryMock.Object);
        var command = CreateCommand(telefone: "123");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenEmailIsInvalid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(CreateCliente()));

        var useCase = new AtualizarClienteUseCase(repositoryMock.Object);
        var command = CreateCommand(email: "email-invalido");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenNameIsInvalid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(CreateCliente()));

        var useCase = new AtualizarClienteUseCase(repositoryMock.Object);
        var command = CreateCommand(nome: "  ");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O nome do cliente é obrigatório."));
        });

        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        Cliente? updatedCliente = null;
        var command = CreateCommand();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(CreateCliente(command.Id)));
        _ = repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .Callback<Cliente, CancellationToken>((cliente, _) => updatedCliente = cliente)
            .Returns(Task.CompletedTask);

        var useCase = new AtualizarClienteUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(updatedCliente, Is.Not.Null);
        });

        repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(command.Id));
            Assert.That(response.Nome, Is.EqualTo("Cliente Atualizado"));
            Assert.That(response.Cpf, Is.EqualTo("529.982.247-25"));
            Assert.That(response.Cnpj, Is.Null);
            Assert.That(response.Telefone, Is.EqualTo("(11) 99888-7777"));
            Assert.That(response.Email, Is.EqualTo("atualizado@exemplo.com"));
        });
    }

    private static AtualizarClienteCommand CreateCommand(
        Guid? id = null,
        string nome = "Cliente Atualizado",
        string telefone = "11998887777",
        string? email = "atualizado@exemplo.com") =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Nome = nome,
            Telefone = telefone,
            Email = email
        };

    private static Cliente CreateCliente(Guid? id = null)
    {
        var telefoneResult = Telefone.Create("11987654321");
        var cpfResult = Cpf.Create("52998224725");
        var emailResult = Email.Create("cliente@exemplo.com");

        Assert.Multiple(() =>
        {
            Assert.That(telefoneResult.IsSuccess, Is.True);
            Assert.That(cpfResult.IsSuccess, Is.True);
            Assert.That(emailResult.IsSuccess, Is.True);
        });

        var clienteResult = Cliente.Rehydrate(
            id ?? Guid.NewGuid(),
            "Cliente Inicial",
            cpfResult.Value,
            null,
            telefoneResult.Value!,
            emailResult.Value);

        Assert.Multiple(() =>
        {
            Assert.That(clienteResult.IsSuccess, Is.True);
            Assert.That(clienteResult.Value, Is.Not.Null);
        });

        return clienteResult.Value!;
    }

}

