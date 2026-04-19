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
    public async Task GetByOrdemServicoIdAsync_ShouldReturnOnlyServicosFromRequestedOrdemServico()
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
                DescricaoProblema = "Ruido na dianteira",
                Status = StatusOrdemServico.Recebida,
                DataCriacao = DateTime.UtcNow
            },
            new OrdemServicoEntity
            {
                Id = outraOrdemServicoId,
                ClienteId = Guid.NewGuid(),
                VeiculoId = Guid.NewGuid(),
                DescricaoProblema = "Falha de ignicao",
                Status = StatusOrdemServico.Recebida,
                DataCriacao = DateTime.UtcNow
            });

        context.ServicoDaOrdemDeServico.AddRange(
            new ServicoDaOrdemDeServicoEntity
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = ordemServicoId,
                ServicoId = Guid.NewGuid(),
                Descricao = "Alinhamento",
                ValorUnitario = 120m,
                Quantidade = 1,
                TempoGastoMinutos = null,
                Concluido = false
            },
            new ServicoDaOrdemDeServicoEntity
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = ordemServicoId,
                ServicoId = Guid.NewGuid(),
                Descricao = "Balanceamento",
                ValorUnitario = 90m,
                Quantidade = 2,
                TempoGastoMinutos = 20,
                Concluido = true
            },
            new ServicoDaOrdemDeServicoEntity
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = outraOrdemServicoId,
                ServicoId = Guid.NewGuid(),
                Descricao = "Troca de velas",
                ValorUnitario = 220m,
                Quantidade = 1,
                TempoGastoMinutos = null,
                Concluido = false
            });

        _ = await context.SaveChangesAsync();

        var repository = new ServicoDaOrdemDeServicoRepository(context);

        var result = await repository.GetByOrdemServicoIdAsync(ordemServicoId);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Has.Count.EqualTo(2));
            Assert.That(result.Value!.All(x => x.OrdemServicoId == ordemServicoId), Is.True);
            Assert.That(result.Value!.Sum(x => x.ValorTotal), Is.EqualTo(300m));
            Assert.That(result.Value!.Any(x => x.Concluido), Is.True);
        });
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnServicoDaOrdem_WhenServicoExists()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);

        var ordemServicoId = Guid.NewGuid();
        var servicoDaOrdemId = Guid.NewGuid();

        _ = context.OrdensServico.Add(new OrdemServicoEntity
        {
            Id = ordemServicoId,
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Problema no motor",
            Status = StatusOrdemServico.Recebida,
            DataCriacao = DateTime.UtcNow
        });

        _ = context.ServicoDaOrdemDeServico.Add(new ServicoDaOrdemDeServicoEntity
        {
            Id = servicoDaOrdemId,
            OrdemServicoId = ordemServicoId,
            ServicoId = Guid.NewGuid(),
            Descricao = "Troca de oleo",
            ValorUnitario = 100m,
            Quantidade = 1,
            TempoGastoMinutos = null,
            Concluido = false
        });

        _ = await context.SaveChangesAsync();

        var repository = new ServicoDaOrdemDeServicoRepository(context);

        var result = await repository.GetByIdAsync(servicoDaOrdemId);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value!.Id, Is.EqualTo(servicoDaOrdemId));
            Assert.That(result.Value.Concluido, Is.False);
            Assert.That(result.Value.TempoGastoMinutos, Is.Null);
        });
    }

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
            DescricaoProblema = "Diagnosticar ruido no motor",
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
            Assert.That(saved.TempoGastoMinutos, Is.Null);
            Assert.That(saved.Concluido, Is.False);
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldPersistConclusaoDoServicoDaOrdemDeServico()
    {
        var databaseName = Guid.NewGuid().ToString();

        await using var context = CreateContext(databaseName);

        var ordemServicoId = Guid.NewGuid();
        var servicoDaOrdemId = Guid.NewGuid();

        _ = context.OrdensServico.Add(new OrdemServicoEntity
        {
            Id = ordemServicoId,
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Revisao",
            Status = StatusOrdemServico.Recebida,
            DataCriacao = DateTime.UtcNow
        });

        _ = context.ServicoDaOrdemDeServico.Add(new ServicoDaOrdemDeServicoEntity
        {
            Id = servicoDaOrdemId,
            OrdemServicoId = ordemServicoId,
            ServicoId = Guid.NewGuid(),
            Descricao = "Troca de oleo",
            ValorUnitario = 100m,
            Quantidade = 1,
            TempoGastoMinutos = null,
            Concluido = false
        });

        _ = await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new ServicoDaOrdemDeServicoRepository(context);
        var servicoDaOrdemResult = await repository.GetByIdAsync(servicoDaOrdemId);

        Assert.That(servicoDaOrdemResult.IsSuccess, Is.True);

        var concluirResult = servicoDaOrdemResult.Value!.Concluir(50);
        Assert.That(concluirResult.IsSuccess, Is.True);

        await repository.UpdateAsync(servicoDaOrdemResult.Value!);

        var saved = await context.ServicoDaOrdemDeServico.AsNoTracking().FirstOrDefaultAsync(x => x.Id == servicoDaOrdemId);

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.Concluido, Is.True);
            Assert.That(saved.TempoGastoMinutos, Is.EqualTo(50));
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
