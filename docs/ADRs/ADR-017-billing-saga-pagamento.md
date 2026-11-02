# ADR-017 — Billing e Saga de pagamento no OS

- **Status:** Aceita
- **Data:** 2026-11-01

## Contexto

A aprovação administrativa autorizava imediatamente o Execution. O próximo incremento exige pagamento antes da execução e serviço Billing independente.

## Decisão

Billing usa PostgreSQL exclusivo e gateway abstrato com mock de aprovação imediata. OS persiste orçamento e estado da Saga; solicita pagamento em tópico dedicado e só autoriza Execution após confirmação. Recusa de negócio solicita cancelamento e devolução de estoque; mantém a aprovação original no histórico. Não há estorno nem cancelamento administrativo após aprovação. Domain/Application permanecem independentes de EF/Kafka concretos.

## Alternativas

Cobrar após execução não atende ao fluxo aprovado. Assinar diretamente diagnósticos no Billing criaria dependência de ordenação entre tópicos; OS já recebe o diagnóstico e envia um pedido autocontido. Um framework de Saga e Outbox foram adiados para preservar o padrão existente.

## Consequências

Novas dependências NuGet entre contratos dos produtores, estado eventual explícito e reconciliação manual em entrega incerta. Não existe atomicidade entre gateway, banco e Kafka. Uma cobrança por ordem; novas tentativas e Mercado Pago real ficam para outra entrega.

## Evidências

Implementação em Billing, Saga e migrations OS; contratos e testes reais. Consulte RFC-010 e o relatório de validação Billing para distinguir código entregue de verificações executadas.
