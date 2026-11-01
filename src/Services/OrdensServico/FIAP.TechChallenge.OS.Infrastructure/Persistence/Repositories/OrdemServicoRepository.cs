using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;
using FIAP.TechChallenge.OS.Domain.Interfaces;
using FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Repositories;

public sealed class OrdemServicoRepository(AppDbContext context) : IOrdemServicoRepository, IHistoricoOrdemRepository
{
    public async Task<Result<OrdemServico>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Mutating use cases open a transaction. Locking also lets a fast Kafka confirmation
        // wait until the administrative decision has committed.
        var query = context.Database.CurrentTransaction is null
            ? context.OrdensServico.AsNoTracking().Where(x => x.Id == id)
            : context.OrdensServico.FromSqlInterpolated(
                $"""SELECT * FROM "OrdensServico" WHERE "Id" = {id} FOR UPDATE""").AsNoTracking();
        var entity = await query.SingleOrDefaultAsync(cancellationToken);
        return entity is null
            ? Result<OrdemServico>.Failure(new Error("Ordem de servico nao encontrada.", ErrorCode.NotFound))
            : Map(entity);
    }

    public async Task<Result<(IReadOnlyCollection<OrdemServico> OrdensServico, int TotalItems)>> GetPagedAsync(
        Guid? clienteId, Guid? veiculoId, IReadOnlyCollection<StatusOrdemServico> status,
        SortDirection? statusSortDirection, SortDirection? dataAberturaSortDirection,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.OrdensServico.AsNoTracking();

        if (clienteId.HasValue)
            query = query.Where(x => x.ClienteId == clienteId);

        if (veiculoId.HasValue)
            query = query.Where(x => x.VeiculoId == veiculoId);

        if (status.Count > 0)
            query = query.Where(x => status.Contains(x.Status));

        var total = await query.CountAsync(cancellationToken);
        var ordered = statusSortDirection == SortDirection.Desc
            ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status);
        ordered = dataAberturaSortDirection == SortDirection.Asc
            ? ordered.ThenBy(x => x.DataCriacao) : ordered.ThenByDescending(x => x.DataCriacao);
        var rows = await ordered.ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        IReadOnlyCollection<OrdemServico> items = rows.Select(x => Map(x).Value!).ToArray();

        return Result<(IReadOnlyCollection<OrdemServico>, int)>.Success((items, total));
    }

    public async Task AddAsync(OrdemServico ordem, CancellationToken cancellationToken = default)
    {
        _ = context.OrdensServico.Add(new OrdemServicoEntity
        {
            Id = ordem.Id,
            ClienteId = ordem.ClienteId,
            VeiculoId = ordem.VeiculoId,
            DescricaoProblema = ordem.DescricaoProblema,
            Status = ordem.Status,
            DataCriacao = ordem.DataCriacao
        });
        _ = context.HistoricoOrdens.Add(new HistoricoOrdemEntity
        {
            Id = Guid.NewGuid(),
            OrdemServicoId = ordem.Id,
            Tipo = "OrdemCriada",
            OcorridoEmUtc = ordem.DataCriacao,
            CorrelationId = System.Diagnostics.Activity.Current?.GetBaggageItem("correlation.id")
                ?? System.Diagnostics.Activity.Current?.TraceId.ToString()
        });
        // A single relational SaveChanges transaction commits both records or neither.
        _ = await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<HistoricoOrdem>> GetByOrdemAsync(Guid ordemServicoId, CancellationToken cancellationToken = default) =>
        await context.HistoricoOrdens.AsNoTracking().Where(x => x.OrdemServicoId == ordemServicoId)
            .OrderBy(x => x.OcorridoEmUtc).ThenBy(x => x.Id)
            .Select(x => new HistoricoOrdem(x.Id, x.OrdemServicoId, x.Tipo, x.OcorridoEmUtc, x.CorrelationId)).ToArrayAsync(cancellationToken);

    public async Task UpdateAsync(OrdemServico ordem, HistoricoOrdem historico, CancellationToken cancellationToken = default)
    {
        var row = new OrdemServicoEntity
        {
            Id = ordem.Id,
            ClienteId = ordem.ClienteId,
            VeiculoId = ordem.VeiculoId,
            DescricaoProblema = ordem.DescricaoProblema,
            DataCriacao = ordem.DataCriacao,
            Status = ordem.Status,
            Decisao = ordem.Decisao,
            DataDecisaoUtc = ordem.DataDecisaoUtc,
            DataInicioDiagnostico = ordem.DataInicioDiagnostico,
            DataEnvioAprovacao = ordem.DataEnvioAprovacao,
            DataInicioExecucao = ordem.DataInicioExecucao,
            DataFinalizacao = ordem.DataFinalizacao,
            DataCancelamento = ordem.DataCancelamento,
            Version = ordem.Version
        };
        _ = context.Attach(row);
        context.Entry(row).State = EntityState.Modified;
        row.Version++;
        _ = context.HistoricoOrdens.Add(new HistoricoOrdemEntity
        {
            Id = historico.Id,
            OrdemServicoId = historico.OrdemServicoId,
            Tipo = historico.Tipo,
            OcorridoEmUtc = historico.OcorridoEmUtc,
            CorrelationId = historico.CorrelationId
        });
        try
        { _ = await context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { throw new ConcurrencyException(); }
    }

    private static Result<OrdemServico> Map(OrdemServicoEntity row) =>
        OrdemServico.Rehydrate(new OrdemServicoSnapshot(row.Id, row.ClienteId, row.VeiculoId, row.DescricaoProblema,
            row.DataCriacao, row.Status, row.Decisao, row.DataDecisaoUtc, row.DataInicioDiagnostico,
            row.DataEnvioAprovacao, row.DataInicioExecucao, row.DataFinalizacao, row.DataCancelamento, row.Version));
}
