# ADR-014 — Execution independente e consistência por Kafka

- **Status:** Aceita
- **Data:** 2026-10-31
- **Escopo:** estados da OS, integração, persistência e autorização temporária

## Contexto

O OS independente aceitava somente Recebida. A migração aprovada extrai diagnóstico, catálogo, estoque e execução para `Maieru/fiap-tech-challenge-execution`, mantendo o monólito em coexistência e o OS como responsável pelo estado geral/histórico.

## Decisão

Execution usa MongoDB próprio com replica set, reservas, snapshots, versão, Inbox/Outbox e transações locais. OS mantém PostgreSQL, cadastros, IDs e volume atuais. Sua migration amplia estados/datas, acrescenta versão, solicitação pendente, Inbox/Outbox e cria OrdemCriada para ordens Recebida existentes, de forma idempotente.

Kafka transporta `MessageEnvelope<T>` v1 do chassi 1.0.2, com chave da OS e sequência independente por direção. Efeito, histórico e mensagem são atômicos no banco local; chamadas ao broker ocorrem fora da transação. Dispatcher com lease publica em ordem, confirmação do broker precede a marcação da Outbox. Consumidor confirma offset após Inbox/efeitos; retries de 1/2/4/8/16 segundos e DLQ confirmada precedem confirmação do original. Lacunas impedem aplicação de mensagens seguintes da mesma OS. Entrega ao menos uma vez, com efeitos idempotentes, sem promessa de transação distribuída.

Autorizar e cancelar retornam 202 após persistência da solicitação, histórico e Outbox. Uma ação pendente por ordem, protegida por concorrência otimista. A autorização administrativa é temporária, desabilitada por padrão e limitada a Development/Testing com flag explícita. O `sub` do JWT e a origem ficam auditados. Execution confirma início/cancelamento ou recusa; somente a confirmação altera o estado correspondente no OS. Cancelamento parte apenas de AguardandoAprovacao e compensa reservas uma única vez.

## Alternativas consideradas

- Acessar diretamente o MongoDB a partir do OS: violaria a responsabilidade de dados.
- Chamar Execution por HTTP durante a transação: acoplaria disponibilidade e não resolveria atomicidade.
- Compartilhar domínio/contratos num novo NuGet: exigiria evolução simultânea e ampliaria o chassi.
- Migrar dados/tráfego do monólito agora: excederia o incremento local/CI.

## Consequências e evidências

Transportes e contratos existem em cada serviço; BDD entre imagens independentes detecta incompatibilidade. O CI Execution fixa um SHA externo do OS e mede sua própria cobertura; o CI OS conserva seu gate de 80%. O overlay Compose usa a rede `fiap-oficina` sem mudar o volume PostgreSQL existente. Testes exercitam upgrade do esquema anterior, backfill, concorrência, rollback de histórico/Inbox, broker indisponível, lacunas/DLQ/replay e confirmação duplicada. A regressão do legado continua separada.

Billing substituirá a autorização manual; aprovação pelo cliente, frontend, AWS, entrega e migração de tráfego/dados ficam fora desta entrega. A separação futura do repositório OS continua pendente. Detalhes de contratos, operação e adaptações estão no [segundo incremento](../arquitetura/fase4-incremento2.md) e na [documentação do Execution](https://github.com/Maieru/fiap-tech-challenge-execution/tree/main/docs).

## Análise da duplicação durante a coexistência

O Sonar compara legado e OS no mesmo projeto e identifica como duplicação as cópias intencionais de cadastros, value objects, resultados, abertura/listagem e seus adaptadores. Criar dependência de negócio entre os serviços apenas para eliminar esse apontamento contrariaria os limites desta decisão.

O workflow lista exceções temporárias **somente de CPD** para esses componentes reaproveitados. Eles continuam no build, testes, cobertura, análise de bugs e segurança. O fluxo novo de integração, Outbox/Inbox e a entidade operacional não estão excluídos do CPD. Remover as exceções ao separar o OS em repositório/projeto Sonar próprios. A cobertura mínima de 80% por componente permanece integral.
