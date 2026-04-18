using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Repositories;

[TestFixture]
internal sealed class PecaOuInsumoDaOrdemDeServicoRepositoryTests
{
    [Test]
    public async Task GetByOrdemServicoIdAsync_ShouldReturnOnlyPecasEInsumosFromRequestedOrdemServico()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);

        var ordemServicoId = Guid.NewGuid();
        var outraOrdemServicoId = Guid.NewGuid();

        context.OrdensServico.AddRange(
            new OrdemServicoEntity
            {
                Id = ordemServicoId,
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                DescricaoProblema = "Troca de sistema de freio",
                Status = StatusOrdemServico.Recebida,
                DataCriacao = DateTime.UtcNow
            },
            new OrdemServicoEntity
            {
                Id = outraOrdemServicoId,
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                DescricaoProblema = "Problema no ar-condicionado",
                Status = StatusOrdemServico.Recebida,
                DataCriacao = DateTime.UtcNow
            });

        context.PecaOuInsumoDaOrdemDeServico.AddRange(
            new PecaOuInsumoDaOrdemDeServicoEntity
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = ordemServicoId,
                PecaInsumoId = Guid.NewGuid(),
                Nome = "Pastilha de freio",
                Codigo = "PST-100",
                Descricao = "Dianteira",
                PrecoUnitario = 190m,
                Quantidade = 2
            },
            new PecaOuInsumoDaOrdemDeServicoEntity
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = ordemServicoId,
                PecaInsumoId = Guid.NewGuid(),
                Nome = "Fluido DOT4",
                Codigo = "FLD-101",
                Descricao = "Freio",
                PrecoUnitario = 50m,
                Quantidade = 1
            },
            new PecaOuInsumoDaOrdemDeServicoEntity
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = outraOrdemServicoId,
                PecaInsumoId = Guid.NewGuid(),
                Nome = "Filtro de cabine",
                Codigo = "FLT-201",
                Descricao = "Cabine",
                PrecoUnitario = 60m,
                Quantidade = 1
            });

        _ = await context.SaveChangesAsync();

        var repository = new PecaOuInsumoDaOrdemDeServicoRepository(context);

        var result = await repository.GetByOrdemServicoIdAsync(ordemServicoId);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(2));
            Assert.That(result.Value!.All(x => x.OrdemServicoId == ordemServicoId), Is.True);
            Assert.That(result.Value.Sum(x => x.ValorTotal), Is.EqualTo(430m));
        });
    }

    [Test]
    public async Task AddAsync_ShouldPersistPecaOuInsumoDaOrdemDeServico()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);

        var ordemServicoEntity = new OrdemServicoEntity
        {
            Id = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Diagnosticar desgaste de freio",
            Status = StatusOrdemServico.Recebida,
            DataCriacao = DateTime.UtcNow
        };

        _ = context.OrdensServico.Add(ordemServicoEntity);
        _ = await context.SaveChangesAsync();

        var pecaInsumoResult = PecaInsumo.Create("Pastilha de freio", "PST-010", "Pastilha ceramica", 230m, 18);
        Assert.That(pecaInsumoResult.IsSuccess, Is.True);

        var pecaOuInsumoDaOrdemResult = PecaOuInsumoDaOrdemDeServico.Create(ordemServicoEntity.Id, pecaInsumoResult.Value!, 2);
        Assert.That(pecaOuInsumoDaOrdemResult.IsSuccess, Is.True);

        var repository = new PecaOuInsumoDaOrdemDeServicoRepository(context);

        await repository.AddAsync(pecaOuInsumoDaOrdemResult.Value!);

        var saved = await context.PecaOuInsumoDaOrdemDeServico.AsNoTracking().FirstOrDefaultAsync(x => x.Id == pecaOuInsumoDaOrdemResult.Value!.Id);

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.OrdemServicoId, Is.EqualTo(ordemServicoEntity.Id));
            Assert.That(saved.PecaInsumoId, Is.EqualTo(pecaOuInsumoDaOrdemResult.Value!.PecaInsumoId));
            Assert.That(saved.Nome, Is.EqualTo("Pastilha de freio"));
            Assert.That(saved.Codigo, Is.EqualTo("PST-010"));
            Assert.That(saved.Descricao, Is.EqualTo("Pastilha ceramica"));
            Assert.That(saved.PrecoUnitario, Is.EqualTo(230m));
            Assert.That(saved.Quantidade, Is.EqualTo(2));
        });
    }

    private static AppDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AppDbContext(options);
    }
}
