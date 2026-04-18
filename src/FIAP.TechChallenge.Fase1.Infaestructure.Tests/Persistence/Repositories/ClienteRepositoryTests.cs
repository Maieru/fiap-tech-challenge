using FIAP.TechChallenge.Fase1.Domain.Abstractions;
using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Repositories;

[TestFixture]
internal sealed class ClienteRepositoryTests
{
    [Test]
    public async Task ExistsByCpfAsync_ShouldReturnTrue_WhenCpfExists()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        _ = context.Clientes.Add(new ClienteEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente CPF",
            Cpf = "52998224725",
            Telefone = "11987654321"
        });
        _ = await context.SaveChangesAsync();

        var repository = new ClienteRepository(context);

        var exists = await repository.ExistsByCpfAsync("52998224725");

        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsByCpfAsync_ShouldReturnFalse_WhenCpfDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new ClienteRepository(context);

        var exists = await repository.ExistsByCpfAsync("52998224725");

        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task ExistsByCnpjAsync_ShouldReturnTrue_WhenCnpjExists()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        _ = context.Clientes.Add(new ClienteEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente CNPJ",
            Cnpj = "11444777000161",
            Telefone = "11987654321"
        });
        _ = await context.SaveChangesAsync();

        var repository = new ClienteRepository(context);

        var exists = await repository.ExistsByCnpjAsync("11444777000161");

        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsByCnpjAsync_ShouldReturnFalse_WhenCnpjDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new ClienteRepository(context);

        var exists = await repository.ExistsByCnpjAsync("11444777000161");

        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenClienteDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new ClienteRepository(context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Cliente não encontrado."));
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.NotFound));
        });
    }

    [Test]
    public async Task GetByCpfAsync_ShouldReturnFailure_WhenClienteDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new ClienteRepository(context);

        var result = await repository.GetByCpfAsync("52998224725");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Cliente não encontrado."));
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.NotFound));
        });
    }

    [Test]
    public async Task GetByCpfAsync_ShouldReturnSuccess_WhenClienteExists()
    {
        var databaseName = Guid.NewGuid().ToString();
        var clienteId = Guid.NewGuid();

        await using var context = CreateContext(databaseName);
        _ = context.Clientes.Add(new ClienteEntity
        {
            Id = clienteId,
            Nome = "Cliente CPF",
            Cpf = "52998224725",
            Telefone = "11987654321",
            Email = "cliente.cpf@exemplo.com"
        });
        _ = await context.SaveChangesAsync();

        var repository = new ClienteRepository(context);

        var result = await repository.GetByCpfAsync("52998224725");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(clienteId));
            Assert.That(result.Value.Cpf, Is.Not.Null);
            Assert.That(result.Value.Cpf!.Unformatted, Is.EqualTo("52998224725"));
            Assert.That(result.Value.Email!.Value, Is.EqualTo("cliente.cpf@exemplo.com"));
        });
    }

    [Test]
    public async Task GetByCnpjAsync_ShouldReturnSuccess_WhenClienteExists()
    {
        var databaseName = Guid.NewGuid().ToString();
        var clienteId = Guid.NewGuid();

        await using var context = CreateContext(databaseName);
        _ = context.Clientes.Add(new ClienteEntity
        {
            Id = clienteId,
            Nome = "Cliente CNPJ",
            Cnpj = "11444777000161",
            Telefone = "11987654321"
        });
        _ = await context.SaveChangesAsync();

        var repository = new ClienteRepository(context);

        var result = await repository.GetByCnpjAsync("11444777000161");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(clienteId));
            Assert.That(result.Value.Cnpj, Is.Not.Null);
            Assert.That(result.Value.Cnpj!.Unformatted, Is.EqualTo("11444777000161"));
            Assert.That(result.Value.Cpf, Is.Null);
        });
    }

    [Test]
    public async Task GetByCnpjAsync_ShouldReturnFailure_WhenClienteDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new ClienteRepository(context);

        var result = await repository.GetByCnpjAsync("11444777000161");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Cliente não encontrado."));
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.NotFound));
        });
    }

    [Test]
    public async Task GetPagedAsync_ShouldReturnPagedDataAndTotal_WhenClientesExist()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        context.Clientes.AddRange(
            new ClienteEntity
            {
                Id = Guid.NewGuid(),
                Nome = "Bruno",
                Cpf = "52998224725",
                Telefone = "11987654321"
            },
            new ClienteEntity
            {
                Id = Guid.NewGuid(),
                Nome = "Ana",
                Cpf = "39053344705",
                Telefone = "21987654321"
            },
            new ClienteEntity
            {
                Id = Guid.NewGuid(),
                Nome = "Carlos",
                Cnpj = "11444777000161",
                Telefone = "11999888777"
            });
        _ = await context.SaveChangesAsync();

        var repository = new ClienteRepository(context);

        var result = await repository.GetPagedAsync(1, 2);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Clientes, Has.Count.EqualTo(2));
            Assert.That(result.Value.TotalItems, Is.EqualTo(3));
            Assert.That(result.Value.Clientes.ElementAt(0).Nome, Is.EqualTo("Ana"));
            Assert.That(result.Value.Clientes.ElementAt(1).Nome, Is.EqualTo("Bruno"));
        });
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnSuccess_WhenClienteExists()
    {
        var databaseName = Guid.NewGuid().ToString();
        var clienteId = Guid.NewGuid();

        await using var context = CreateContext(databaseName);
        _ = context.Clientes.Add(new ClienteEntity
        {
            Id = clienteId,
            Nome = "Cliente Encontrado",
            Cpf = "52998224725",
            Telefone = "11987654321",
            Email = "cliente@exemplo.com"
        });
        _ = await context.SaveChangesAsync();

        var repository = new ClienteRepository(context);

        var result = await repository.GetByIdAsync(clienteId);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(result.Value!.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Value.Nome, Is.EqualTo("Cliente Encontrado"));
            Assert.That(result.Value.Cpf!.Unformatted, Is.EqualTo("52998224725"));
            Assert.That(result.Value.Cnpj, Is.Null);
            Assert.That(result.Value.Telefone.Unformatted, Is.EqualTo("11987654321"));
            Assert.That(result.Value.Email!.Value, Is.EqualTo("cliente@exemplo.com"));
        });
    }

    [Test]
    public async Task AddAsync_ShouldPersistCliente()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new ClienteRepository(context);
        var cliente = CreateClienteComCpf();

        await repository.AddAsync(cliente);

        var saved = await context.Clientes.FirstOrDefaultAsync(x => x.Id == cliente.Id);

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Nome, Is.EqualTo("Cliente Teste"));
            Assert.That(saved.Cpf, Is.EqualTo("52998224725"));
            Assert.That(saved.Cnpj, Is.Null);
            Assert.That(saved.Telefone, Is.EqualTo("11987654321"));
            Assert.That(saved.Email, Is.EqualTo("cliente@exemplo.com"));
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldPersistClienteChanges()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new ClienteRepository(context);
        var cliente = CreateClienteComCpf();

        await repository.AddAsync(cliente);
        context.ChangeTracker.Clear();

        var updatedNameResult = cliente.UpdateName("Cliente Atualizado");
        var updatedTelefoneResult = cliente.UpdateTelefone(Telefone.Create("21987654321").Value!);
        var updatedEmailResult = cliente.UpdateEmail(Email.Create("novo@exemplo.com").Value);

        await repository.UpdateAsync(cliente);

        var saved = await context.Clientes.FirstOrDefaultAsync(x => x.Id == cliente.Id);

        Assert.Multiple(() =>
        {
            Assert.That(updatedNameResult.IsSuccess, Is.True);
            Assert.That(updatedTelefoneResult.IsSuccess, Is.True);
            Assert.That(updatedEmailResult.IsSuccess, Is.True);
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Nome, Is.EqualTo("Cliente Atualizado"));
            Assert.That(saved.Telefone, Is.EqualTo("21987654321"));
            Assert.That(saved.Email, Is.EqualTo("novo@exemplo.com"));
            Assert.That(saved.Cpf, Is.EqualTo("52998224725"));
        });
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveCliente()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new ClienteRepository(context);
        var cliente = CreateClienteComCpf();

        await repository.AddAsync(cliente);
        context.ChangeTracker.Clear();
        await repository.DeleteAsync(cliente);

        var saved = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cliente.Id);

        Assert.That(saved, Is.Null);
    }

    private static AppDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AppDbContext(options);
    }

    private static Cliente CreateClienteComCpf()
    {
        var cpf = Cpf.Create("52998224725").Value!;
        var telefone = Telefone.Create("11987654321").Value!;
        var email = Email.Create("cliente@exemplo.com").Value;

        return Cliente.Create("Cliente Teste", cpf, null, telefone, email).Value!;
    }
}


