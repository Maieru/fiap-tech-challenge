using FIAP.TechChallenge.Fase1.Application.UseCases.Clientes.CriarCliente;
using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Interfaces;

namespace FIAP.TechChallenge.Fase1.Application.Tests.UseCases.Clientes.CriarCliente;

[TestFixture]
internal class CriarClienteUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenTelefoneIsInvalid()
    {
        var repository = new FakeClienteRepository();
        var useCase = new CriarClienteUseCase(repository);
        var command = CreateCommand(telefone: "123");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
            Assert.That(repository.CpfCheckCalls, Is.EqualTo(0));
            Assert.That(repository.CnpjCheckCalls, Is.EqualTo(0));
            Assert.That(repository.AddCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCpfIsInvalid()
    {
        var repository = new FakeClienteRepository();
        var useCase = new CriarClienteUseCase(repository);
        var command = CreateCommand(cpf: "123");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
            Assert.That(repository.CpfCheckCalls, Is.EqualTo(0));
            Assert.That(repository.CnpjCheckCalls, Is.EqualTo(0));
            Assert.That(repository.AddCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCpfAlreadyExists()
    {
        var repository = new FakeClienteRepository
        {
            ExistsByCpfResult = true
        };

        var useCase = new CriarClienteUseCase(repository);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Já existe um cliente cadastrado com este CPF."));
            Assert.That(repository.CpfCheckCalls, Is.EqualTo(1));
            Assert.That(repository.CnpjCheckCalls, Is.EqualTo(0));
            Assert.That(repository.AddCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCnpjIsInvalid()
    {
        var repository = new FakeClienteRepository();
        var useCase = new CriarClienteUseCase(repository);
        var command = CreateCommand(cpf: null, cnpj: "123");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
            Assert.That(repository.CnpjCheckCalls, Is.EqualTo(0));
            Assert.That(repository.AddCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenCnpjAlreadyExists()
    {
        var repository = new FakeClienteRepository
        {
            ExistsByCnpjResult = true
        };

        var useCase = new CriarClienteUseCase(repository);
        var command = CreateCommand(cpf: null, cnpj: "11444777000161");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Já existe um cliente cadastrado com este CNPJ."));
            Assert.That(repository.CnpjCheckCalls, Is.EqualTo(1));
            Assert.That(repository.AddCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenEmailIsInvalid()
    {
        var repository = new FakeClienteRepository();
        var useCase = new CriarClienteUseCase(repository);
        var command = CreateCommand(email: "email-invalido");

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error, Is.Not.EqualTo(Error.None));
            Assert.That(repository.AddCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task ExecuteAsync_ShouldSucceed_WhenCommandIsValid()
    {
        var repository = new FakeClienteRepository();
        var useCase = new CriarClienteUseCase(repository);
        var command = CreateCommand();

        var result = await useCase.ExecuteAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.EqualTo(Error.None));
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(repository.CpfCheckCalls, Is.EqualTo(1));
            Assert.That(repository.CnpjCheckCalls, Is.EqualTo(0));
            Assert.That(repository.AddCalls, Is.EqualTo(1));
            Assert.That(repository.AddedCliente, Is.Not.Null);
        });

        var response = result.Value!;

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(repository.AddedCliente!.Id));
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

    private sealed class FakeClienteRepository : IClienteRepository
    {
        public bool ExistsByCpfResult { get; init; }
        public bool ExistsByCnpjResult { get; init; }
        public int CpfCheckCalls { get; private set; }
        public int CnpjCheckCalls { get; private set; }
        public int AddCalls { get; private set; }
        public Cliente? AddedCliente { get; private set; }

        public Task<bool> ExistsByCpfAsync(string cpf, CancellationToken cancellationToken = default)
        {
            CpfCheckCalls++;
            return Task.FromResult(ExistsByCpfResult);
        }

        public Task<bool> ExistsByCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
        {
            CnpjCheckCalls++;
            return Task.FromResult(ExistsByCnpjResult);
        }

        public Task<Result<Cliente>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result<Cliente>.Failure(Error.NotFound(nameof(Cliente))));

        public Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            AddCalls++;
            AddedCliente = cliente;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Cliente cliente, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(Cliente cliente, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
