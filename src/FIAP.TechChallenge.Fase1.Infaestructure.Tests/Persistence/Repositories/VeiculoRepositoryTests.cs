using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.ValueObjects;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Repositories;

[TestFixture]
internal sealed class VeiculoRepositoryTests
{
    [Test]
    public async Task GetByIdAsync_ShouldReturnSuccess_WhenVeiculoExists()
    {
        var databaseName = Guid.NewGuid().ToString();
        var veiculo = CreateVeiculo();

        await using var context = CreateContext(databaseName);
        _ = context.Veiculos.Add(new VeiculoEntity
        {
            Id = veiculo.Id,
            ClienteId = veiculo.ClienteId,
            Placa = veiculo.Placa.Unformatted,
            Marca = veiculo.Marca,
            Modelo = veiculo.Modelo,
            Ano = veiculo.Ano
        });
        _ = await context.SaveChangesAsync();

        var repository = new VeiculoRepository(context);

        var result = await repository.GetByIdAsync(veiculo.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(result.Value!.Id, Is.EqualTo(veiculo.Id));
            Assert.That(result.Value.ClienteId, Is.EqualTo(veiculo.ClienteId));
            Assert.That(result.Value.Placa.Unformatted, Is.EqualTo(veiculo.Placa.Unformatted));
            Assert.That(result.Value.Marca, Is.EqualTo(veiculo.Marca));
            Assert.That(result.Value.Modelo, Is.EqualTo(veiculo.Modelo));
            Assert.That(result.Value.Ano, Is.EqualTo(veiculo.Ano));
        });
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenVeiculoDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new VeiculoRepository(context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Veículo não encontrado."));
        });
    }

    [Test]
    public async Task ExistsByPlacaAsync_ShouldReturnTrue_WhenPlacaExists()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        _ = context.Veiculos.Add(new VeiculoEntity
        {
            Id = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            Placa = "ABC1234",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023
        });
        _ = await context.SaveChangesAsync();

        var repository = new VeiculoRepository(context);

        var exists = await repository.ExistsByPlacaAsync("ABC1234");

        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsByPlacaAsync_ShouldReturnFalse_WhenPlacaDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new VeiculoRepository(context);

        var exists = await repository.ExistsByPlacaAsync("ABC1234");

        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task AddAsync_ShouldPersistVeiculo()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new VeiculoRepository(context);
        var veiculo = CreateVeiculo();

        await repository.AddAsync(veiculo);

        var saved = await context.Veiculos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == veiculo.Id);

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Id, Is.EqualTo(veiculo.Id));
            Assert.That(saved.ClienteId, Is.EqualTo(veiculo.ClienteId));
            Assert.That(saved.Placa, Is.EqualTo(veiculo.Placa.Unformatted));
            Assert.That(saved.Marca, Is.EqualTo(veiculo.Marca));
            Assert.That(saved.Modelo, Is.EqualTo(veiculo.Modelo));
            Assert.That(saved.Ano, Is.EqualTo(veiculo.Ano));
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var databaseName = Guid.NewGuid().ToString();
        var veiculo = CreateVeiculo();
        var novaPlaca = Placa.Create("BRA2E19").Value!;

        await using var context = CreateContext(databaseName);
        var repository = new VeiculoRepository(context);
        await repository.AddAsync(veiculo);

        context.ChangeTracker.Clear();

        _ = veiculo.UpdatePlaca(novaPlaca);
        _ = veiculo.UpdateMarca("Honda");
        _ = veiculo.UpdateModelo("Civic");
        _ = veiculo.UpdateAno(2024);

        await repository.UpdateAsync(veiculo);

        var saved = await context.Veiculos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == veiculo.Id);

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Placa, Is.EqualTo("BRA2E19"));
            Assert.That(saved.Marca, Is.EqualTo("Honda"));
            Assert.That(saved.Modelo, Is.EqualTo("Civic"));
            Assert.That(saved.Ano, Is.EqualTo(2024));
        });
    }

    private static AppDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AppDbContext(options);
    }

    private static Veiculo CreateVeiculo()
    {
        var placaResult = Placa.Create("ABC1234");
        Assert.That(placaResult.IsSuccess, Is.True);

        var veiculoResult = Veiculo.Create(Guid.NewGuid(), placaResult.Value!, "Toyota", "Corolla", 2023);
        Assert.Multiple(() =>
        {
            Assert.That(veiculoResult.IsSuccess, Is.True);
            Assert.That(veiculoResult.Value, Is.Not.Null);
        });

        return veiculoResult.Value!;
    }
}
