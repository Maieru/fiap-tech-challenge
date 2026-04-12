using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Repositories;

[TestFixture]
internal sealed class PecaInsumoRepositoryTests
{
    [Test]
    public async Task GetByIdAsync_ShouldReturnSuccess_WhenPecaInsumoExists()
    {
        var databaseName = Guid.NewGuid().ToString();
        var entity = CreateEntity();

        await using var context = CreateContext(databaseName);
        _ = context.PecasInsumos.Add(entity);
        _ = await context.SaveChangesAsync();

        var repository = new PecaInsumoRepository(context);

        var result = await repository.GetByIdAsync(entity.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(result.Value!.Id, Is.EqualTo(entity.Id));
            Assert.That(result.Value.Nome, Is.EqualTo(entity.Nome));
            Assert.That(result.Value.Codigo, Is.EqualTo(entity.Codigo));
            Assert.That(result.Value.Descricao, Is.EqualTo(entity.Descricao));
            Assert.That(result.Value.PrecoUnitario, Is.EqualTo(entity.PrecoUnitario));
            Assert.That(result.Value.QuantidadeEstoque, Is.EqualTo(entity.QuantidadeEstoque));
            Assert.That(result.Value.Ativo, Is.EqualTo(entity.Ativo));
        });
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenPecaInsumoDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new PecaInsumoRepository(context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Peca ou insumo nao encontrado."));
        });
    }

    [Test]
    public async Task GetByCodigoAsync_ShouldReturnSuccess_WhenPecaInsumoExists()
    {
        var databaseName = Guid.NewGuid().ToString();
        var entity = CreateEntity();

        await using var context = CreateContext(databaseName);
        _ = context.PecasInsumos.Add(entity);
        _ = await context.SaveChangesAsync();

        var repository = new PecaInsumoRepository(context);

        var result = await repository.GetByCodigoAsync(entity.Codigo);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(entity.Id));
            Assert.That(result.Value.Codigo, Is.EqualTo(entity.Codigo));
        });
    }

    [Test]
    public async Task GetByCodigoAsync_ShouldReturnFailure_WhenPecaInsumoDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new PecaInsumoRepository(context);

        var result = await repository.GetByCodigoAsync("NAO-EXISTE");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Peca ou insumo nao encontrado."));
        });
    }

    [Test]
    public async Task GetPagedAsync_ShouldReturnPagedItemsAndTotalCount()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        _ = context.PecasInsumos.Add(new PecaInsumoEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Pastilha de freio",
            Codigo = "PST-010",
            Descricao = "Conjunto dianteiro",
            PrecoUnitario = 100m,
            QuantidadeEstoque = 5,
            Ativo = true
        });
        _ = context.PecasInsumos.Add(new PecaInsumoEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Filtro de ar",
            Codigo = "FLT-001",
            Descricao = "Elemento filtrante",
            PrecoUnitario = 30m,
            QuantidadeEstoque = 12,
            Ativo = true
        });
        _ = context.PecasInsumos.Add(new PecaInsumoEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Oleo 5W30",
            Codigo = "OLE-500",
            Descricao = "Lubrificante sintetico",
            PrecoUnitario = 60m,
            QuantidadeEstoque = 20,
            Ativo = true
        });
        _ = await context.SaveChangesAsync();

        var repository = new PecaInsumoRepository(context);

        var result = await repository.GetPagedAsync(pageNumber: 1, pageSize: 2);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.PecasInsumos, Has.Count.EqualTo(2));
            Assert.That(result.Value.TotalItems, Is.EqualTo(3));
            Assert.That(result.Value.PecasInsumos.Select(x => x.Codigo), Is.EqualTo(new[] { "FLT-001", "OLE-500" }));
        });
    }

    [Test]
    public async Task AddAsync_ShouldPersistPecaInsumo()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new PecaInsumoRepository(context);
        var pecaInsumo = CreatePecaInsumo();

        await repository.AddAsync(pecaInsumo);

        var saved = await context.PecasInsumos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == pecaInsumo.Id);

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Id, Is.EqualTo(pecaInsumo.Id));
            Assert.That(saved.Nome, Is.EqualTo(pecaInsumo.Nome));
            Assert.That(saved.Codigo, Is.EqualTo(pecaInsumo.Codigo));
            Assert.That(saved.Descricao, Is.EqualTo(pecaInsumo.Descricao));
            Assert.That(saved.PrecoUnitario, Is.EqualTo(pecaInsumo.PrecoUnitario));
            Assert.That(saved.QuantidadeEstoque, Is.EqualTo(pecaInsumo.QuantidadeEstoque));
            Assert.That(saved.Ativo, Is.True);
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldPersistAllowedChanges_AndKeepStockValue()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new PecaInsumoRepository(context);
        var pecaInsumo = CreatePecaInsumo();

        await repository.AddAsync(pecaInsumo);
        context.ChangeTracker.Clear();

        _ = pecaInsumo.UpdateNome("Filtro de cabine premium");
        _ = pecaInsumo.UpdateCodigo("FCB-999");
        _ = pecaInsumo.UpdateDescricao("Filtro com maior capacidade de retencao");
        _ = pecaInsumo.UpdatePrecoUnitario(89.9m);
        _ = pecaInsumo.Inactivate();

        await repository.UpdateAsync(pecaInsumo);

        var saved = await context.PecasInsumos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == pecaInsumo.Id);

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Nome, Is.EqualTo("Filtro de cabine premium"));
            Assert.That(saved.Codigo, Is.EqualTo("FCB-999"));
            Assert.That(saved.Descricao, Is.EqualTo("Filtro com maior capacidade de retencao"));
            Assert.That(saved.PrecoUnitario, Is.EqualTo(89.9m));
            Assert.That(saved.QuantidadeEstoque, Is.EqualTo(15));
            Assert.That(saved.Ativo, Is.False);
        });
    }

    private static AppDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AppDbContext(options);
    }

    private static PecaInsumoEntity CreateEntity()
    {
        return new PecaInsumoEntity
        {
            Id = Guid.NewGuid(),
            Nome = "Pastilha de freio",
            Codigo = "PST-045",
            Descricao = "Jogo dianteiro",
            PrecoUnitario = 179.90m,
            QuantidadeEstoque = 8,
            Ativo = true
        };
    }

    private static PecaInsumo CreatePecaInsumo()
    {
        var pecaInsumoResult = PecaInsumo.Create("Filtro de cabine", "fcb-010", "Filtro de cabine com carvao ativado", 69.5m, 15);

        Assert.Multiple(() =>
        {
            Assert.That(pecaInsumoResult.IsSuccess, Is.True);
            Assert.That(pecaInsumoResult.Value, Is.Not.Null);
        });

        return pecaInsumoResult.Value!;
    }
}
