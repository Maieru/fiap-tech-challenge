using FIAP.TechChallenge.Fase1.Domain.Entities;
using FIAP.TechChallenge.Fase1.Domain.Enums;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.Fase1.Infaestructure.Tests.Persistence.Repositories;

[TestFixture]
internal sealed class ServicoDaOrdemDeServicoRepositoryTests
{
    [Test]
    public async Task AddAsync_ShouldPersistServicoDaOrdemDeServico()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);

        var ordemServicoEntity = new OrdemServicoEntity
        {
            Id = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Diagnosticar ruído no motor",
            Status = StatusOrdemServico.Recebida,
            DataCriacao = DateTime.UtcNow
        };

        _ = context.OrdensServico.Add(ordemServicoEntity);
        _ = await context.SaveChangesAsync();

        var servicoResult = Servico.Create("Troca de correia dentada", 550m);
        Assert.That(servicoResult.IsSuccess, Is.True);

        var servicoDaOrdemResult = ServicoDaOrdemDeServico.Create(ordemServicoEntity.Id, servicoResult.Value!, 2);
        Assert.That(servicoDaOrdemResult.IsSuccess, Is.True);

        var repository = new ServicoDaOrdemDeServicoRepository(context);

        await repository.AddAsync(servicoDaOrdemResult.Value!);

        var saved = await context.ServicoDaOrdemDeServico.AsNoTracking().FirstOrDefaultAsync(x => x.Id == servicoDaOrdemResult.Value!.Id);

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.OrdemServicoId, Is.EqualTo(ordemServicoEntity.Id));
            Assert.That(saved.ServicoId, Is.EqualTo(servicoDaOrdemResult.Value!.ServicoId));
            Assert.That(saved.Descricao, Is.EqualTo("Troca de correia dentada"));
            Assert.That(saved.ValorUnitario, Is.EqualTo(550m));
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
