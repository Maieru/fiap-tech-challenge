using FIAP.TechChallenge.OS.Domain.Abstractions;
using FIAP.TechChallenge.OS.Domain.Interfaces;
using FIAP.TechChallenge.OS.Infrastructure.Persistence;
using FIAP.TechChallenge.OS.Infrastructure.Persistence.Entities;
using FIAP.TechChallenge.OS.Infrastructure.Messaging;
using Maieru.FiapTechChallenge.Chassis;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;

namespace FIAP.TechChallenge.OS.Infrastructure.Persistence.Repositories;

public sealed class FluxoOrdemRepository(AppDbContext db, IntegrationContext context) : IFluxoOrdemRepository, IIntegrationEventHandler
{
    public async Task<Result<SolicitacaoOrdem>> SolicitarAsync(Guid ordemId, string acao, string operadorId, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var row = await db.OrdensServico.SingleOrDefaultAsync(x => x.Id == ordemId, ct);

        if (row is null)
            return Result<SolicitacaoOrdem>.Failure(new Error("Ordem de servico nao encontrada.", ErrorCode.NotFound));

        var order = OrdemServicoRepository.Map(row).Value!;
        var request = order.Solicitar(acao);

        if (!request.IsSuccess)
            return Result<SolicitacaoOrdem>.Failure(request.Error);

        OrdemServicoRepository.Apply(row, order);
        row.Version++;
        row.OutputSequence++;
        var type = acao == "Autorizacao" ? "AutorizacaoExecucaoSolicitada" : "CancelamentoSolicitado";
        var payload = new FlowPayload(ordemId, row.OutputSequence, SolicitacaoId: request.Value, OperadorId: operadorId,
            Origem: acao == "Autorizacao" ? "AdministrativaTemporaria" : "Administrativa");
        var message = context.Create(type, payload);

        _ = db.Outbox.Add(new OutboxEntity
        {
            Id = message.MessageId,
            OrdemServicoId = ordemId,
            Sequence = row.OutputSequence,
            Body = IntegrationContext.Serialize(message),
            CreatedAtUtc = message.OccurredAtUtc.UtcDateTime
        });

        _ = db.HistoricoOrdens.Add(new HistoricoOrdemEntity
        {
            Id = Guid.NewGuid(),
            OrdemServicoId = ordemId,
            Tipo = type,
            OcorridoEmUtc = message.OccurredAtUtc.UtcDateTime,
            CorrelationId = message.CorrelationId,
            Detalhes = JsonSerializer.Serialize(payload, IntegrationContext.Json)
        });

        try
        {
            _ = await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException) { return Conflict(); }
        catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { return Conflict(); }

        return Result<SolicitacaoOrdem>.Success(new(request.Value, acao));
    }
    private static Result<SolicitacaoOrdem> Conflict() => Result<SolicitacaoOrdem>.Failure(new Error("A ordem foi alterada ou ja possui uma solicitacao pendente.", ErrorCode.Conflict));

    public async Task ApplyAsync(MessageEnvelope<FlowPayload> message, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        if (await db.Inbox.AnyAsync(x => x.Id == message.MessageId, ct))
            return;

        var row = await db.OrdensServico.SingleOrDefaultAsync(x => x.Id == message.Payload.OrdemServicoId, ct)
            ?? throw new InvalidOperationException("Ordem nao encontrada para aplicar evento.");

        if (message.Payload.Sequence > row.InputSequence)
        {
            if (message.Payload.Sequence != row.InputSequence + 1)
                throw new InvalidOperationException("Lacuna na sequencia de eventos do Execution.");

            var order = OrdemServicoRepository.Map(row).Value!;
            var result = order.AplicarEvento(message.Type, message.OccurredAtUtc.UtcDateTime,
                message.Payload.SolicitacaoId, message.Payload.Snapshot?.GetRawText());

            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Error.Description);

            OrdemServicoRepository.Apply(row, order);
            row.Version++;
            row.InputSequence = message.Payload.Sequence;
            _ = db.HistoricoOrdens.Add(new HistoricoOrdemEntity
            {
                Id = Guid.NewGuid(),
                OrdemServicoId = row.Id,
                Tipo = message.Type,
                OcorridoEmUtc = message.OccurredAtUtc.UtcDateTime,
                CorrelationId = message.CorrelationId,
                Detalhes = JsonSerializer.Serialize(new { message.MessageId, message.CausationId, message.Payload }, IntegrationContext.Json)
            });
        }

        _ = db.Inbox.Add(new InboxEntity { Id = message.MessageId, ProcessedAtUtc = DateTime.UtcNow });
        _ = await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

}
