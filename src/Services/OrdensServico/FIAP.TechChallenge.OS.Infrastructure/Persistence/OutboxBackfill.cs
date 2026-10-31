namespace FIAP.TechChallenge.OS.Infrastructure.Persistence;

public static class OutboxBackfill
{
    // Stable semantic identity is (order, sequence); rerunning never creates a second opening event.
    public const string Sql = """
        WITH received AS (
            SELECT o.*, gen_random_uuid() AS message_id FROM "OrdensServico" o WHERE o."Status" = 1
        )
        INSERT INTO "Outbox" ("Id", "OrdemServicoId", "Sequence", "Body", "CreatedAtUtc", "Published")
        SELECT message_id, "Id", 1,
            jsonb_build_object('messageId', message_id, 'type', 'OrdemCriada', 'version', 1,
                'occurredAtUtc', "DataCriacao", 'correlationId', 'backfill-' || "Id"::text,
                'causationId', NULL, 'traceParent', NULL, 'traceState', NULL,
                'payload', jsonb_build_object('ordemServicoId', "Id", 'sequence', 1,
                    'clienteId', "ClienteId", 'veiculoId', "VeiculoId", 'descricaoProblema', "DescricaoProblema")),
            CURRENT_TIMESTAMP, false
        FROM received ON CONFLICT ("OrdemServicoId", "Sequence") DO NOTHING;
        """;
}
