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
