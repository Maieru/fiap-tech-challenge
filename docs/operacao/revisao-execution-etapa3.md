# Revisão — etapa 3: OS e Execution por Kafka

Consulte o [índice de validação](validacao-execution.md) para o histórico das etapas anteriores.

Data: 2026-11-01. Implementação local entregue para revisão, sem commit, push, tag ou publicação.

## Entrega

- OS publica abertura, autorização e solicitação de cancelamento pelo IMessagingService injetado na Application.
- Aprovação e cancelamento administrativos exigem JWT e retornam 202; decisão pendente fica persistida até a confirmação.
- Decisões repetidas não republicam. Decisões contraditórias, inclusive concorrentes, têm apenas uma vencedora.
- BackgroundService OS recebe os cinco eventos Execution com switch explícito e casos de uso específicos.
- PostgreSQL preserva status, datas, decisão, histórico, versão e deduplicação com migration incremental.
- A projeção grava dados/histórico/deduplicação na mesma transação; offset Kafka avança após commit.
- Correlação, causação e trace W3C atravessam as duas APIs. Replays não repetem estoque/histórico nem regridem status.
- Execution mantém repositório, solução e MongoDB próprios; a integração não usa ProjectReference entre repositórios.
- Compose integrado e BDD Reqnroll das duas imagens ficam no Execution. Padrão, ADR e RFC ficam neste repositório principal.
- Novos comandos, interfaces e casos de uso estão em arquivos separados.

## Evidências locais

| Verificação | Resultado |
| --- | --- |
| OS Domain | 131 testes aprovados |
| OS Application | 56 testes aprovados |
| OS contratos | 4 testes aprovados |
| OS integração HTTP/PostgreSQL/Kafka | 37 testes aprovados |
| OS imagem real | 1 teste aprovado |
| Cobertura consolidada OS | 90,1% — 1649/1829 linhas; mínimo 80% atendido |
| Execution regressão isolada | 148 testes — 81 domínio, 44 aplicação, 6 contratos e 17 integração |
| BDD com ambas as imagens | 4 cenários aprovados |
| Imagens fiap-os:phase3 e fiap-execution:phase3 | Builds locais concluídos |
| Compose integrado em projeto descartável | Ambas as APIs com readiness HTTP 200 |
| EF Core has-pending-model-changes | Nenhuma mudança pendente |
| Workflow distribuído | actionlint 1.7.12 sem erros |

Os testes usam PostgreSQL, MongoDB replica set e Kafka reais; endpoints recebem JWTs realmente assinados.
O BDD prepara clientes, veículos, catálogo e ordens pelos contratos HTTP/Kafka, sem mocks ou inserts nos bancos.
Os containers têm rede/portas/volumes isolados e não acessam dados de desenvolvimento.

Os quatro cenários distribuídos cobrem conclusão com snapshots/histórico, cancelamento e replay sem devolução duplicada,
aprovação/cancelamento concorrentes e abertura enquanto Execution está parado com consumo após reinício.

Os testes transacionais do OS param o broker e verificam rollback de abertura, aprovação e cancelamento.
Constraints diferidas de teste falham no commit PostgreSQL real para demonstrar rollback de projeção/histórico/deduplicação
e possibilidade de mensagem Kafka órfã. Não há flags de falha na produção.
A imagem também recebe JSON inválido: processo encerra com saída 1, registra o erro e deixa o offset pendente.

Artefatos ignorados pelo Git, no repositório principal:

- artifacts/test-results/os-phase3-final: TRX e cobertura por projeto.
- artifacts/coverage/os-phase3: relatório consolidado.
- artifacts/image-tests/phase3-final e artifacts/os-openapi.json: teste da imagem e OpenAPI.

No Execution: artifacts/test-results/distributed-phase3 e artifacts/test-results/execution-phase3-regression.
O Compose de verificação usou exclusivamente o projeto fiap-phase3-verification; containers, rede e volumes foram removidos ao final.

## Reproduzir

Consulte o [README OS](../../src/Services/OrdensServico/README.md) para restaurar contratos e construir a imagem OS.
No Execution, consulte o README para restaurar o contrato OS, construir sua imagem e executar Distributed.slnx.
O Compose integrado recebe OS_IMAGE e usa os mesmos parâmetros JWT nos dois serviços.
Os testes distribuídos precisam apenas das imagens OS_TEST_IMAGE e EXECUTION_TEST_IMAGE e do Docker.

O workflow manual distributed.yml recebe referências de imagens publicadas; não compila nem faz checkout do outro repositório.
Os workflows isolados dos serviços continuam com build, testes, imagem e cobertura próprios.

## Limites e operação

Os dois pacotes de contratos 0.1.0 ainda exigem publicação para restore/CI em ambiente limpo.
Nesta validação, foram usados nupkgs locais explícitos em artifacts/packages, com scripts que conferem ID/versão.
Chassi 1.1.1 e adaptador Kafka 0.1.1 foram restaurados do NuGet público.
Não houve execução remota de workflow, análise Sonar ou deploy.

Publicar e depois fazer commit local aceita o risco de mensagem órfã ou entrega incerta.
Em falha, não repetir cegamente: verificar dados confirmados, Kafka, grupo/offset e MessageId, reconciliar e reiniciar.
O consumidor não descarta mensagens, não usa DLQ nem reinicia automaticamente pelo Compose.
O procedimento canônico está no [padrão de mensageria](../arquitetura/mensageria.md).

A migration preserva ordens anteriores em Recebida. Seu Down restaura a constraint antiga que só aceita Recebida:
não aplicar downgrade com estados novos presentes nem descartar datas/histórico sem procedimento de reconciliação.
Não há migração de ordens do legado, entrega, Billing, pagamentos, frontend ou infraestrutura AWS nesta etapa.

Decisões: [ADR-016](../ADRs/ADR-016-integracao-os-execution.md) e [RFC-009](../RFCs/RFC-009-integracao-os-execution.md).
