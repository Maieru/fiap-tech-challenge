using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Entities;
using FIAP.TechChallenge.OS.Domain.Enums;
using FIAP.TechChallenge.OS.Domain.Interfaces;
using FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using FIAP.TechChallenge.OS.Infrastructure.Messaging;

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Repositories;

public sealed class OrdemServicoRepository(AppDbContext context, IntegrationContext? integration = null) : IOrdemServicoRepository, IHistoricoOrdemRepository
{
    public async Task<Result<OrdemServico>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.OrdensServico.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
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
        if (clienteId.HasValue) query = query.Where(x => x.ClienteId == clienteId);
        if (veiculoId.HasValue) query = query.Where(x => x.VeiculoId == veiculoId);
        if (status.Count > 0) query = query.Where(x => status.Contains(x.Status));
        var total = await query.CountAsync(cancellationToken);
        IOrderedQueryable<OrdemServicoEntity> ordered = statusSortDirection == SortDirection.Desc
            ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status);
        ordered = dataAberturaSortDirection == SortDirection.Asc
            ? ordered.ThenBy(x => x.DataCriacao) : ordered.ThenByDescending(x => x.DataCriacao);
        var rows = await ordered.ThenBy(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        IReadOnlyCollection<OrdemServico> items = rows.Select(x => Map(x).Value!).ToArray();
        return Result<(IReadOnlyCollection<OrdemServico>, int)>.Success((items, total));
    }

    public async Task AddAsync(OrdemServico ordem, CancellationToken cancellationToken = default)
    {
        context.OrdensServico.Add(new OrdemServicoEntity
        {
            Id = ordem.Id, ClienteId = ordem.ClienteId, VeiculoId = ordem.VeiculoId,
            DescricaoProblema = ordem.DescricaoProblema, Status = ordem.Status, DataCriacao = ordem.DataCriacao
        });
        context.HistoricoOrdens.Add(new HistoricoOrdemEntity
        {
            Id = Guid.NewGuid(), OrdemServicoId = ordem.Id, Tipo = "OrdemCriada", OcorridoEmUtc = ordem.DataCriacao,
            CorrelationId = System.Diagnostics.Activity.Current?.GetBaggageItem("correlation.id")
                ?? System.Diagnostics.Activity.Current?.TraceId.ToString()
        });
        var message = (integration ?? new IntegrationContext()).Create("OrdemCriada", new FlowPayload(ordem.Id, 1,
            ordem.ClienteId, ordem.VeiculoId, ordem.DescricaoProblema)) with { OccurredAtUtc = ordem.DataCriacao };
        context.Outbox.Add(new OutboxEntity { Id = message.MessageId, OrdemServicoId = ordem.Id, Sequence = 1,
            Body = IntegrationContext.Serialize(message), CreatedAtUtc = DateTime.UtcNow });
        // A single relational SaveChanges transaction commits both records or neither.
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<HistoricoOrdem>> GetByOrdemAsync(Guid ordemServicoId, CancellationToken cancellationToken = default) =>
        await context.HistoricoOrdens.AsNoTracking().Where(x => x.OrdemServicoId == ordemServicoId)
            .OrderBy(x => x.OcorridoEmUtc).ThenBy(x => x.Id)
            .Select(x => new HistoricoOrdem(x.Id, x.OrdemServicoId, x.Tipo, x.OcorridoEmUtc, x.CorrelationId, x.Detalhes)).ToArrayAsync(cancellationToken);

    internal static Result<OrdemServico> Map(OrdemServicoEntity row) =>
        OrdemServico.Rehydrate(new OrdemServicoSnapshot(row.Id, row.ClienteId, row.VeiculoId, row.DescricaoProblema, row.DataCriacao,
            row.Status, row.DataInicioDiagnostico, row.DataEnvioAprovacao, row.DataInicioExecucao, row.DataFinalizacao,
            row.AcaoPendente, row.SolicitacaoPendenteId, row.DiagnosticoJson));

    internal static void Apply(OrdemServicoEntity row, OrdemServico value)
    {
        row.Status = value.Status; row.DataInicioDiagnostico = value.DataInicioDiagnostico; row.DataEnvioAprovacao = value.DataEnvioAprovacao;
        row.DataInicioExecucao = value.DataInicioExecucao; row.DataFinalizacao = value.DataFinalizacao; row.AcaoPendente = value.AcaoPendente;
        row.SolicitacaoPendenteId = value.SolicitacaoPendenteId; row.DiagnosticoJson = value.DiagnosticoJson;
    }
}
