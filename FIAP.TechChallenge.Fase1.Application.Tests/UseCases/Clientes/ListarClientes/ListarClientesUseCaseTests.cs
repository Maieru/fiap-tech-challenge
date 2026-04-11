using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.ListarClientes;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using Moq;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Clientes.ListarClientes;

[TestFixture]
internal sealed class ListarClientesUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenPaginationIsInvalid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var useCase = new ListarClientesUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarClientesCommand { PageNumber = 0, PageSize = 10 });

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
        var repositoryMock = new Mock<IClienteRepository>();
        var useCase = new ListarClientesUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarClientesCommand { Id = Guid.NewGuid(), Cpf = "52998224725" });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Informe apenas um filtro por vez: id, cpf ou cnpj."));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenListingPagedClients()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var cliente1 = CreateClienteComCpf();
        var cliente2 = CreateClienteComCnpj();

        _ = repositoryMock
            .Setup(x => x.GetPagedAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<(IReadOnlyCollection<Cliente> Clientes, int TotalItems)>.Success((new[] { cliente1, cliente2 }, 2)));

        var useCase = new ListarClientesUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarClientesCommand { PageNumber = 1, PageSize = 2 });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.TotalItems, Is.EqualTo(2));
            Assert.That(result.Value.PageNumber, Is.EqualTo(1));
            Assert.That(result.Value.PageSize, Is.EqualTo(2));
            Assert.That(result.Value.Clientes, Has.Count.EqualTo(2));
        });

        repositoryMock.Verify(x => x.GetPagedAsync(1, 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenGettingByCpf()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var cliente = CreateClienteComCpf();

        _ = repositoryMock
            .Setup(x => x.GetByCpfAsync("52998224725", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));

        var useCase = new ListarClientesUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarClientesCommand { Cpf = "529.982.247-25" });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Clientes, Has.Count.EqualTo(1));
            Assert.That(result.Value.Clientes.First().Cpf, Is.EqualTo("529.982.247-25"));
        });

        repositoryMock.Verify(x => x.GetByCpfAsync("52998224725", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenGettingById()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var cliente = CreateClienteComCpf();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));

        var useCase = new ListarClientesUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarClientesCommand { Id = cliente.Id });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Clientes, Has.Count.EqualTo(1));
            Assert.That(result.Value.Clientes.First().Id, Is.EqualTo(cliente.Id));
            Assert.That(result.Value.Clientes.First().Nome, Is.EqualTo("Cliente CPF"));
        });

        repositoryMock.Verify(x => x.GetByIdAsync(cliente.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenIdDoesNotExist()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var clienteId = Guid.NewGuid();

        _ = repositoryMock
            .Setup(x => x.GetByIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Failure(new Error("Cliente não encontrado.")));

        var useCase = new ListarClientesUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarClientesCommand { Id = clienteId });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Cliente não encontrado."));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenGettingByCnpj()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var cliente = CreateClienteComCnpj();

        _ = repositoryMock
            .Setup(x => x.GetByCnpjAsync("11444777000161", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Cliente>.Success(cliente));

        var useCase = new ListarClientesUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarClientesCommand { Cnpj = "11.444.777/0001-61" });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Clientes, Has.Count.EqualTo(1));
            Assert.That(result.Value.Clientes.First().Cnpj, Is.EqualTo("11.444.777/0001-61"));
        });

        repositoryMock.Verify(x => x.GetByCnpjAsync("11444777000161", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCnpjIsInvalid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var useCase = new ListarClientesUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarClientesCommand { Cnpj = "123" });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        repositoryMock.Verify(x => x.GetByCnpjAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCpfIsInvalid()
    {
        var repositoryMock = new Mock<IClienteRepository>();
        var useCase = new ListarClientesUseCase(repositoryMock.Object);

        var result = await useCase.ExecuteAsync(new ListarClientesCommand { Cpf = "123" });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
        });

        repositoryMock.Verify(x => x.GetByCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
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

    private static Cliente CreateClienteComCnpj()
    {
        var telefoneResult = Telefone.Create("11999888777");
        var cnpjResult = Cnpj.Create("11444777000161");

        Assert.Multiple(() =>
        {
            Assert.That(telefoneResult.IsSuccess, Is.True);
            Assert.That(cnpjResult.IsSuccess, Is.True);
        });

        var clienteResult = Cliente.Create("Cliente CNPJ", null, cnpjResult.Value, telefoneResult.Value!, null);

        Assert.Multiple(() =>
        {
            Assert.That(clienteResult.IsSuccess, Is.True);
            Assert.That(clienteResult.Value, Is.Not.Null);
        });

        return clienteResult.Value!;
    }
}
