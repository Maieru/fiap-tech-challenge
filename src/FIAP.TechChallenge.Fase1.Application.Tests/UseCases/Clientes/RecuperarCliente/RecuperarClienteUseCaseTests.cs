using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.RecuperarCliente;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Clientes.RecuperarCliente;

[TestFixture]
internal sealed class RecuperarClienteUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenClienteIdIsEmpty()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var useCase = new RecuperarClienteUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new RecuperarClienteCommand { ClienteId = Guid.Empty });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O identificador do cliente deve ser valido."));
        });

        repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenClienteDoesNotExist()
    {
        var repositoryMock = new Mock<IClienteRepository>();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Failure(new Error("Cliente não encontrado.")));

        var useCase = new RecuperarClienteUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new RecuperarClienteCommand { ClienteId = Guid.NewGuid() });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Cliente não encontrado."));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenClienteExists()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var cliente = CreateClienteComCpf();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));

        var useCase = new RecuperarClienteUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new RecuperarClienteCommand { ClienteId = cliente.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(cliente.Id));
            Assert.That(result.Value.Nome, Is.EqualTo("Cliente CPF"));
            Assert.That(result.Value.Cpf, Is.EqualTo("529.982.247-25"));
            Assert.That(result.Value.Telefone, Is.EqualTo("(11) 98765-4321"));
            Assert.That(result.Value.Email, Is.EqualTo("cliente@exemplo.com"));
        });
    }

    private static Cliente CreateClienteComCpf()
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

        var clienteResult = Cliente.Create("Cliente CPF", cpfResult.Value, null, telefoneResult.Value!, emailResult.Value);

        Assert.Multiple(() =>
        {
            Assert.That(clienteResult.IsSuccess, Is.True);
            Assert.That(clienteResult.Value, Is.Not.Null);
        });

        return clienteResult.Value!;
    }
}

