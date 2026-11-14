# ADR-002 — Comunicação síncrona por HTTP/JSON e REST pragmático

- **Status:** Aceita
- **Data:** 2026-08-23
- **Última revisão:** 2026-11-14
- **Escopo:** integração entre frontend e APIs

## Contexto

A SPA consulta e altera dados por contratos HTTP. A coordenação entre serviços também inclui operações assíncronas por Kafka.

## Decisão

Usar HTTP/JSON nos prefixos `/api/os/v1`, `/api/execution/v1` e `/api/billing/v1`. Controllers traduzem `Result<T>` para status e respostas HTTP. Chamadas administrativas enviam JWT Bearer; o acompanhamento público usa o token privado da ordem validado pelo OS.

Operações que aguardam confirmação por evento retornam `202 Accepted`. O frontend consulta o estado até a confirmação. Kafka transporta os contratos entre os serviços, conforme a ADR-014.

## Alternativas consideradas

GraphQL ou gRPC acrescentariam outro contrato para a SPA. Acesso direto ao banco contornaria regras e controles das APIs. Mensageria é adequada à coordenação entre serviços, mas não substitui as consultas HTTP do navegador.

## Consequências

A API e suas dependências precisam estar disponíveis durante a chamada. Confirmação HTTP não garante a conclusão de uma Saga. Retentativas de escrita exigem regras próprias de idempotência; timeout pode ocorrer após a gravação.

## Evidências e relações

- [Controllers OS](../../src/FIAP.TechChallenge.OS.API/Controllers).
- [Mapeamento de resultados](../../src/FIAP.TechChallenge.OS.API/Extensions/ResultExtensions.cs).
- [Cliente HTTP do frontend](../../src/FIAP.TechChallenge.Fase1.Frontend/src/services/api.ts).
- [Contratos de mensagens](ADR-014-contratos-mensageria.md).
