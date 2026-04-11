using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.AtualizarCliente;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Clientes.AtualizarCliente;

[TestFixture]
internal class AtualizarClienteUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenClienteIsNotFound()
    {
        var repository = new FakeClienteRepository();
        var useCase = new AtualizarClienteUseCase(repository);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Cliente não encontrado."));
            Assert.That(repository.UpdateCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenTelefoneIsInvalid()
    {
        var repository = new FakeClienteRepository
        {
            GetByIdResult = Result<Cliente>.Success(CreateCliente())
        };

        var useCase = new AtualizarClienteUseCase(repository);
        var command = CreateCommand(telefone: "123");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
            Assert.That(repository.UpdateCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenEmailIsInvalid()
    {
        var repository = new FakeClienteRepository
        {
            GetByIdResult = Result<Cliente>.Success(CreateCliente())
        };

        var useCase = new AtualizarClienteUseCase(repository);
        var command = CreateCommand(email: "email-invalido");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
            Assert.That(repository.UpdateCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenNameIsInvalid()
    {
        var repository = new FakeClienteRepository
        {
            GetByIdResult = Result<Cliente>.Success(CreateCliente())
        };

        var useCase = new AtualizarClienteUseCase(repository);
        var command = CreateCommand(nome: "  ");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("O nome do cliente é obrigatório."));
            Assert.That(repository.UpdateCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var repository = new FakeClienteRepository
        {
            GetByIdResult = Result<Cliente>.Success(CreateCliente())
        };

        var useCase = new AtualizarClienteUseCase(repository);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(repository.UpdateCalls, Is.EqualTo(1));
            Assert.That(repository.UpdatedCliente, Is.Not.Null);
        });

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

    private static Cliente CreateCliente()
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
            Guid.NewGuid(),
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

    private sealed class FakeClienteRepository : IClienteRepository
    {
        public Result<Cliente> GetByIdResult { get; init; } = Result<Cliente>.Failure(new Error("Cliente não encontrado."));
        public int UpdateCalls { get; private set; }
        public Cliente? UpdatedCliente { get; private set; }

        public Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task<bool> ExistsByCnpjAsync(string cnpj, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task<Result<Cliente>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(GetByIdResult);

        public Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            UpdateCalls++;
            UpdatedCliente = cliente;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Cliente cliente, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
