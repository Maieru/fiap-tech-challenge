# ADR-015 — Execution com MongoDB e transações locais

- **Status:** Aceita
- **Data:** 2026-11-01
- **Última revisão:** 2026-11-01
- **Escopo:** fronteira do Execution, persistência e consistência

## Contexto

O Execution precisa assumir diagnóstico, catálogo, estoque e execução sem consultar o PostgreSQL do OS ou do legado.
A fase 4 prevê persistência SQL/NoSQL. A baixa de peças e sua devolução precisam permanecer consistentes com a ordem.

## Decisão

Manter o Execution em repositório independente e usar MongoDB exclusivo.
Armazenar uma execução por OrdemServicoId, com snapshots dos itens, nas coleções execucoes, servicos e pecas_insumos.
Manter deduplicação de mensagens na coleção mensagens_processadas, com chave única de consumidor/MessageId.

Usar replica set, sessões e transações explícitas nas operações com múltiplos documentos ou publicação.
Baixar estoque ao adicionar peças ao diagnóstico e devolver ao cancelar uma execução aguardando aprovação,
inclusive quando o cadastro da peça tiver sido inativado.
Preservar preços/descrições da ordem após mudanças ou exclusão lógica dos catálogos.

Atualizações usam versão esperada. O código de peça tem índice único parcial entre registros ativos.
Conflitos não recebem retry automático na aplicação.

A publicação segue a [ADR-014](ADR-014-contratos-mensageria.md) e o [padrão de mensageria](../arquitetura/mensageria.md):
gravar na transação, publicar e depois confirmar o banco. Falha de publicação aborta a transação local.
Entrega seguida de falha no commit continua sendo um limite aceito, não uma transação distribuída.

## Alternativas consideradas

- PostgreSQL: tecnologia já usada, mas não atende à decisão de NoSQL para o Execution.
- MongoDB standalone: não oferece as transações necessárias entre execução, estoque e deduplicação.
- Outbox: não adotada, conforme a decisão explícita de publicação direta com rollback limitado.
- Acesso ao banco legado ou catálogo compartilhado: rompe o isolamento dos serviços.

## Consequências

O Compose local inicia um replica set de nó único. Isso não constitui configuração de alta disponibilidade para produção.
MongoDB não substitui o PostgreSQL do OS e não há migração de dados antigos neste incremento.
Inicialização cria coleções e índices antes de iniciar o consumidor.

O OS publica e recebe Kafka conforme a [ADR-016](ADR-016-integracao-os-execution.md).
Os testes isolados do Execution produzem mensagens compatíveis com o OS diretamente no broker;
os testes distribuídos verificam as duas APIs reais. O [estado da migração](../arquitetura/estado-da-migracao.md) reúne entregas e pendências.

## Evidências e relações

- [Arquitetura e limites do Execution](../arquitetura/execution.md).
- [RFC-008](../RFCs/RFC-008-execution-mongodb-kafka.md).
- No checkout FiapTechChallengeExecution: MongoUnitOfWork, MongoStore, repositórios e TransactionTests.
- Os testes usam MongoDB replica set e Kafka reais, incluindo concorrência, rollback, devolução e deduplicação.
