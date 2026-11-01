# ADR-016 — Decisões administrativas e projeção do Execution no OS

- **Status:** Aceita
- **Data:** 2026-11-01
- **Escopo:** etapa 3 da implementação aprovada

## Contexto

OS administra a ordem; Execution controla diagnóstico, estoque e execução em repositório e banco próprios.
A comunicação deve seguir a ADR-014, com Kafka, interface injetada nos casos de uso e consumidor explícito,
sem coordenador de transações distribuídas.

## Decisão

OS publica OrdemServicoCriada, ExecucaoAutorizada e CancelamentoSolicitado.
Os endpoints administrativos de aprovação e cancelamento retornam 202 e persistem a decisão;
o status continua AguardandoAprovacao até chegar ExecucaoIniciada ou ExecucaoCancelada.
A mesma decisão pode ser repetida sem republicação, inclusive após sua confirmação. A decisão oposta retorna 409.

O BackgroundService do OS trata os cinco eventos Execution com switch e um caso de uso Application para cada entrada.
Status, datas, histórico e deduplicação são gravados na mesma transação PostgreSQL.
Eventos já aplicados não repetem histórico nem fazem o status regredir, mesmo com outro MessageId.
O offset só avança depois do commit. Erros param o host com saída não zero, sem retry de aplicação ou DLQ.

Casos que alteram uma ordem abrem transação e leem a linha com FOR UPDATE.
Isso serializa decisões contraditórias e permite que uma confirmação rápida aguarde o commit da decisão administrativa.
Version também protege atualizações contra escrita concorrente. Consultas comuns não bloqueiam a linha.

Persistir → publicar e aguardar confirmação Kafka → commit local continua sendo a sequência aprovada.
Uma publicação que falha aborta a alteração local. Mensagem entregue antes de um commit que falha exige reconciliação;
não existe garantia de atomicidade entre PostgreSQL/MongoDB e Kafka.

## Alternativas consideradas

- Alterar imediatamente o status no endpoint: confundiria solicitação com confirmação do Execution.
- Fazer HTTP entre os serviços ou compartilhar bancos: criaria dependência operacional e de dados desnecessária.
- Outbox, Saga, mediator e despacho genérico: adiados para manter o escopo simples aprovado.
- Somente concorrência otimista na decisão: detectaria conflito depois de já publicar uma decisão contraditória.

## Consequências e evidências

A consulta OS passa a ser uma projeção eventual, com decisão pendente explicitamente disponível.
Uma ordem anterior em Recebida permanece válida na migration. Estados novos impedem downgrade direto para o schema anterior.
Não há migração do legado, pagamento, entrega, frontend ou deploy AWS nesta etapa.

[RFC-009](../RFCs/RFC-009-integracao-os-execution.md), [padrão](../arquitetura/mensageria.md)
e [evidências](../operacao/revisao-execution-etapa3.md).
