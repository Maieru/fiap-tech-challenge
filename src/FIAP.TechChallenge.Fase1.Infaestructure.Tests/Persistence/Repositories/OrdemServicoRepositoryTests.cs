using FIAP.TechChallenge.Fase1.Domain.Abstractions;
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
            Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.NotFound));
        });
    }

    [Test]
    public async Task GetPagedAsync_ShouldReturnFilteredOrders_WhenClienteVeiculoAndStatusAreInformed()
    {
        var databaseName = Guid.NewGuid().ToString();
        var clienteId = Guid.NewGuid();
        var veiculoId = Guid.NewGuid();
        var ordemEsperada = CreateEntity(
            id: Guid.NewGuid(),
            clienteId: clienteId,
            veiculoId: veiculoId,
            status: StatusOrdemServico.EmDiagnostico,
            dataCriacao: new DateTime(2026, 04, 12, 12, 0, 0, DateTimeKind.Utc));

        await using var context = CreateContext(databaseName);
        context.OrdensServico.AddRange(
            ordemEsperada,
            CreateEntity(clienteId: clienteId, veiculoId: veiculoId, status: StatusOrdemServico.Recebida),
            CreateEntity(clienteId: clienteId, veiculoId: Guid.NewGuid(), status: StatusOrdemServico.EmDiagnostico),
            CreateEntity(clienteId: Guid.NewGuid(), veiculoId: veiculoId, status: StatusOrdemServico.EmDiagnostico));
        _ = await context.SaveChangesAsync();

        var repository = new OrdemServicoRepository(context);

        var result = await repository.GetPagedAsync(
            clienteId,
            veiculoId,
            [StatusOrdemServico.EmDiagnostico],
            statusSortDirection: null,
            dataAberturaSortDirection: null,
            pageNumber: 1,
            pageSize: 10);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.OrdensServico, Has.Count.EqualTo(1));
            Assert.That(result.Value.TotalItems, Is.EqualTo(1));
            Assert.That(result.Value.OrdensServico.First().Id, Is.EqualTo(ordemEsperada.Id));
        });
    }

    [Test]
    public async Task GetPagedAsync_ShouldApplyPaginationAndOrdering_WhenNoFilterIsInformed()
    {
        var databaseName = Guid.NewGuid().ToString();

        var ordemMaisAntiga = CreateEntity(dataCriacao: new DateTime(2026, 04, 10, 8, 0, 0, DateTimeKind.Utc));
        var ordemIntermediaria = CreateEntity(dataCriacao: new DateTime(2026, 04, 11, 8, 0, 0, DateTimeKind.Utc));
        var ordemMaisRecente = CreateEntity(dataCriacao: new DateTime(2026, 04, 12, 8, 0, 0, DateTimeKind.Utc));

        await using var context = CreateContext(databaseName);
        context.OrdensServico.AddRange(ordemMaisAntiga, ordemIntermediaria, ordemMaisRecente);
        _ = await context.SaveChangesAsync();

        var repository = new OrdemServicoRepository(context);

        var result = await repository.GetPagedAsync(
            clienteId: null,
            veiculoId: null,
            status: [],
            statusSortDirection: null,
            dataAberturaSortDirection: null,
            pageNumber: 1,
            pageSize: 2);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.TotalItems, Is.EqualTo(3));
            Assert.That(result.Value.OrdensServico, Has.Count.EqualTo(2));
            Assert.That(result.Value.OrdensServico.First().Id, Is.EqualTo(ordemMaisRecente.Id));
            Assert.That(result.Value.OrdensServico.Skip(1).First().Id, Is.EqualTo(ordemIntermediaria.Id));
        });
    }

    [Test]
    public async Task GetPagedAsync_ShouldReturnFilteredOrders_WhenMultipleStatusAreInformed()
    {
        var databaseName = Guid.NewGuid().ToString();

        var aguardandoAprovacao = CreateEntity(status: StatusOrdemServico.AguardandoAprovacao);
        var emExecucao = CreateEntity(status: StatusOrdemServico.EmExecucao);
        var recebida = CreateEntity(status: StatusOrdemServico.Recebida);
        var finalizada = CreateEntity(status: StatusOrdemServico.Finalizada);

        await using var context = CreateContext(databaseName);
        context.OrdensServico.AddRange(recebida, emExecucao, finalizada, aguardandoAprovacao);
        _ = await context.SaveChangesAsync();

        var repository = new OrdemServicoRepository(context);

        var result = await repository.GetPagedAsync(
            clienteId: null,
            veiculoId: null,
            status: [StatusOrdemServico.AguardandoAprovacao, StatusOrdemServico.EmExecucao],
            statusSortDirection: SortDirection.Asc,
            dataAberturaSortDirection: null,
            pageNumber: 1,
            pageSize: 10);

        var ids = result.Value.OrdensServico.Select(x => x.Id).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.TotalItems, Is.EqualTo(2));
            Assert.That(ids, Is.EqualTo(new[] { aguardandoAprovacao.Id, emExecucao.Id }));
        });
    }

    [Test]
    public async Task GetPagedAsync_ShouldOrderByStatusAscAndDataAberturaDesc_WhenBothSortsAreInformed()
    {
        var databaseName = Guid.NewGuid().ToString();

        var recebidaAntiga = CreateEntity(
            status: StatusOrdemServico.Recebida,
            dataCriacao: new DateTime(2026, 04, 10, 8, 0, 0, DateTimeKind.Utc));
        var recebidaRecente = CreateEntity(
            status: StatusOrdemServico.Recebida,
            dataCriacao: new DateTime(2026, 04, 12, 8, 0, 0, DateTimeKind.Utc));
        var emDiagnostico = CreateEntity(
            status: StatusOrdemServico.EmDiagnostico,
            dataCriacao: new DateTime(2026, 04, 11, 8, 0, 0, DateTimeKind.Utc));
        var finalizada = CreateEntity(
            status: StatusOrdemServico.Finalizada,
            dataCriacao: new DateTime(2026, 04, 09, 8, 0, 0, DateTimeKind.Utc));

        await using var context = CreateContext(databaseName);
        context.OrdensServico.AddRange(finalizada, recebidaAntiga, emDiagnostico, recebidaRecente);
        _ = await context.SaveChangesAsync();

        var repository = new OrdemServicoRepository(context);

        var result = await repository.GetPagedAsync(
            clienteId: null,
            veiculoId: null,
            status: [],
            statusSortDirection: SortDirection.Asc,
            dataAberturaSortDirection: SortDirection.Desc,
            pageNumber: 1,
            pageSize: 10);

        var ids = result.Value.OrdensServico.Select(x => x.Id).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(ids, Is.EqualTo(new[] { recebidaRecente.Id, recebidaAntiga.Id, emDiagnostico.Id, finalizada.Id }));
        });
    }

    [Test]
    public async Task GetPagedAsync_ShouldOrderByStatusDesc_WhenStatusSortIsInformed()
    {
        var databaseName = Guid.NewGuid().ToString();

        var recebida = CreateEntity(status: StatusOrdemServico.Recebida);
        var emDiagnostico = CreateEntity(status: StatusOrdemServico.EmDiagnostico);
        var finalizada = CreateEntity(status: StatusOrdemServico.Finalizada);

        await using var context = CreateContext(databaseName);
        context.OrdensServico.AddRange(recebida, finalizada, emDiagnostico);
        _ = await context.SaveChangesAsync();

        var repository = new OrdemServicoRepository(context);

        var result = await repository.GetPagedAsync(
            clienteId: null,
            veiculoId: null,
            status: [],
            statusSortDirection: SortDirection.Desc,
            dataAberturaSortDirection: null,
            pageNumber: 1,
            pageSize: 10);

        var ids = result.Value.OrdensServico.Select(x => x.Id).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(ids, Is.EqualTo(new[] { finalizada.Id, emDiagnostico.Id, recebida.Id }));
        });
    }

    [Test]
    public async Task GetPagedAsync_ShouldOrderByDataAberturaAsc_WhenDataAberturaSortIsInformed()
    {
        var databaseName = Guid.NewGuid().ToString();

        var ordemMaisAntiga = CreateEntity(dataCriacao: new DateTime(2026, 04, 10, 8, 0, 0, DateTimeKind.Utc));
        var ordemIntermediaria = CreateEntity(dataCriacao: new DateTime(2026, 04, 11, 8, 0, 0, DateTimeKind.Utc));
        var ordemMaisRecente = CreateEntity(dataCriacao: new DateTime(2026, 04, 12, 8, 0, 0, DateTimeKind.Utc));

        await using var context = CreateContext(databaseName);
        context.OrdensServico.AddRange(ordemMaisRecente, ordemMaisAntiga, ordemIntermediaria);
        _ = await context.SaveChangesAsync();

        var repository = new OrdemServicoRepository(context);

        var result = await repository.GetPagedAsync(
            clienteId: null,
            veiculoId: null,
            status: [],
            statusSortDirection: null,
            dataAberturaSortDirection: SortDirection.Asc,
            pageNumber: 1,
            pageSize: 10);

        var ids = result.Value.OrdensServico.Select(x => x.Id).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(ids, Is.EqualTo(new[] { ordemMaisAntiga.Id, ordemIntermediaria.Id, ordemMaisRecente.Id }));
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

    private static OrdemServicoEntity CreateEntity(
        Guid? id = null,
        Guid? clienteId = null,
        Guid? veiculoId = null,
        StatusOrdemServico status = StatusOrdemServico.Recebida,
        DateTime? dataCriacao = null)
    {
        var baseData = dataCriacao ?? new DateTime(2026, 04, 11, 12, 0, 0, DateTimeKind.Utc);
        var entity = new OrdemServicoEntity
        {
            Id = id ?? Guid.NewGuid(),
            CodigoAprovacao = Guid.NewGuid(),
            ClienteId = clienteId ?? Guid.NewGuid(),
            VeiculoId = veiculoId ?? Guid.NewGuid(),
            DescricaoProblema = "Problema no sistema de freio",
            Status = status,
            DataCriacao = baseData
        };

        if (status >= StatusOrdemServico.EmDiagnostico)
            entity.DataInicioDiagnostico = baseData.AddHours(1);

        if (status >= StatusOrdemServico.AguardandoAprovacao)
            entity.DataEnvioAprovacao = baseData.AddHours(2);

        if (status >= StatusOrdemServico.EmExecucao)
            entity.DataInicioExecucao = baseData.AddHours(3);

        if (status >= StatusOrdemServico.Finalizada)
            entity.DataFinalizacao = baseData.AddHours(4);

        if (status == StatusOrdemServico.Entregue)
            entity.DataEntrega = baseData.AddHours(5);

        return entity;
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


