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

## Atualização — 2026-11-08

A [RFC-019](../RFCs/RFC-019-mercado-pago-checkout.md) substitui o gateway mock pela integração com checkout hospedado Mercado Pago. A criação mantém a cobrança pendente; a confirmação/recusa ocorre por webhook validado e conserva os contratos e compensações desta Saga. Valor zero confirma localmente. Novas tentativas após recusa e estornos continuam fora do escopo. Os eventos terminais usam o ID da cobrança como MessageId estável para deduplicação em reentregas após falha de commit.
