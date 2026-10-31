# RFC-007 — Outbox/Inbox e solicitação de execução no OS

Status: Aceita. Data: 2026-10-31.

## Contexto e decisão

O plano aprovado requer integrar o OS ao Execution preservando o PostgreSQL existente. Adotar Confluent.Kafka e transações PostgreSQL locais para ordem, histórico, Outbox e Inbox. Persistir versão otimista e no máximo uma solicitação administrativa pendente. Atualizar o mapeamento para reidratar estado/datas reais e substituir a restrição Status=1 pela faixa do enum existente.

Usar migration EF Core aditiva e backfill com índice único (OS, sequência), sem importar dados legados ou alterar IDs. A autorização manual usa JWT/sub e flag de ambiente; o resultado confirmado vem por evento. O pipeline mantém testes HTTP reais, upgrade do esquema anterior, coverage gate próprio e análise Sonar.

## Alternativas e consequências

Publicar Kafka antes/depois de SaveChanges sem Outbox permitiria perda de operação confirmada. Compartilhar transação entre serviços aumentaria acoplamento. O modelo escolhido aceita consistência eventual explícita (HTTP 202 e ação pendente), retries e necessidade operacional de replay em lacunas. Rollback da migration é bloqueado quando há estados operacionais ou solicitações pendentes para evitar perda silenciosa.

## Evidências

`FluxoOrdemRepository` e `PostgresOutbox`, `OutboxBackfill`, migration `ExecutionIntegration`, `FluxoOrdemTests`, `ExecutionMigrationTests` e `OsKafkaTests`. A [ADR-014](../ADRs/ADR-014-execution-kafka-consistencia.md) detalha a decisão arquitetural e o [Execution](https://github.com/Maieru/fiap-tech-challenge-execution) mantém BDD e documentação operacional.
