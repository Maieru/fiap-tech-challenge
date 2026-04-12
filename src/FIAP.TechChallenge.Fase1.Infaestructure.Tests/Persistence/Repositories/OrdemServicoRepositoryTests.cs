using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Repositories;

[TestFixture]
internal sealed class OrdemServicoRepositoryTests
{
    [Test]
    public async Task GetByIdAsync_ShouldReturnSuccess_WhenOrdemServicoExists()
    {
        var databaseName = Guid.NewGuid().ToString();
        var entity = CreateEntity();

        await using var context = CreateContext(databaseName);
        _ = context.OrdensServico.Add(entity);
        _ = await context.SaveChangesAsync();

        var repository = new OrdemServicoRepository(context);

        var result = await repository.GetByIdAsync(entity.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Error, Is.EqualTo(FIAP.TechChallenge.Fase1.Domain.Abstractions.Error.None));
            Assert.That(result.Value!.Id, Is.EqualTo(entity.Id));
            Assert.That(result.Value.ClienteId, Is.EqualTo(entity.ClienteId));
            Assert.That(result.Value.VeiculoId, Is.EqualTo(entity.VeiculoId));
            Assert.That(result.Value.DescricaoProblema, Is.EqualTo(entity.DescricaoProblema));
            Assert.That(result.Value.Status, Is.EqualTo(entity.Status));
            Assert.That(result.Value.DataCriacao, Is.EqualTo(entity.DataCriacao));
        });
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnFailure_WhenOrdemServicoDoesNotExist()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new OrdemServicoRepository(context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Value, Is.Null);
            Assert.That(result.Error.Description, Is.EqualTo("Ordem de servico nao encontrada."));
        });
    }

    [Test]
    public async Task AddAsync_ShouldPersistOrdemServico()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new OrdemServicoRepository(context);
        var ordemServico = CreateOrdemServico();

        await repository.AddAsync(ordemServico);

        var saved = await context.OrdensServico.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ordemServico.Id);

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Id, Is.EqualTo(ordemServico.Id));
            Assert.That(saved.ClienteId, Is.EqualTo(ordemServico.ClienteId));
            Assert.That(saved.VeiculoId, Is.EqualTo(ordemServico.VeiculoId));
            Assert.That(saved.DescricaoProblema, Is.EqualTo(ordemServico.DescricaoProblema));
            Assert.That(saved.Status, Is.EqualTo(StatusOrdemServico.Recebida));
            Assert.That(saved.DataCriacao, Is.EqualTo(ordemServico.DataCriacao));
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldPersistOrdemServicoChanges()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);
        var repository = new OrdemServicoRepository(context);
        var ordemServico = CreateOrdemServico();

        await repository.AddAsync(ordemServico);
        context.ChangeTracker.Clear();

        var iniciarDiagnosticoResult = ordemServico.IniciarDiagnostico();

        Assert.Multiple(() =>
        {
            Assert.That(iniciarDiagnosticoResult.IsSuccess, Is.True);
            Assert.That(iniciarDiagnosticoResult.Value, Is.True);
        });

        await repository.UpdateAsync(ordemServico);

        var saved = await context.OrdensServico.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ordemServico.Id);

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Status, Is.EqualTo(StatusOrdemServico.EmDiagnostico));
            Assert.That(saved.DataInicioDiagnostico, Is.Not.Null);
        });
    }

    private static AppDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AppDbContext(options);
    }

    private static OrdemServicoEntity CreateEntity()
    {
        return new OrdemServicoEntity
        {
            Id = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Problema no sistema de freio",
            Status = StatusOrdemServico.Recebida,
            DataCriacao = new DateTime(2026, 04, 11, 12, 0, 0, DateTimeKind.Utc)
        };
    }

    private static OrdemServico CreateOrdemServico()
    {
        var ordemServicoResult = OrdemServico.Create(Guid.NewGuid(), Guid.NewGuid(), "Falha no ar-condicionado");

        Assert.Multiple(() =>
        {
            Assert.That(ordemServicoResult.IsSuccess, Is.True);
            Assert.That(ordemServicoResult.Value, Is.Not.Null);
        });

        return ordemServicoResult.Value!;
    }
}
