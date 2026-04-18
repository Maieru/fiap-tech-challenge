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