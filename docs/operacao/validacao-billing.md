# Validação do Billing e Saga no OS

Data: 2026-11-01. Validação local com .NET SDK 10, Docker Desktop e recursos Testcontainers isolados. Nenhum dado ou volume de desenvolvimento foi utilizado.

## Verificado

- OS: 134 testes de domínio, 56 de aplicação, 4 de contratos e 40 HTTP/PostgreSQL/Kafka aprovados, além do smoke da imagem. A aprovação publica solicitação de pagamento; confirmação libera autorização. Estado financeiro e aprovação original sobrevivem ao ciclo e à compensação.
- Billing: 11 testes de contratos/unidade/integração e 1 smoke da imagem aprovados; cobertura de linhas de 90,29% na suíte sem imagem. Cobrem contratos JSON, invariantes, mock determinístico, valor zero e decimal sem arredondamento, HTTP/JWT real, PostgreSQL/Kafka reais, aprovação e recusa por substituição apenas do gateway, concorrência por ordem e rejeição de solicitação conflitante.
- Falhas Billing: gateway lança exceção; publicador real não consegue entregar; constraint diferida PostgreSQL rejeita o commit após publicação real. Os testes verificam rollback local e a mensagem órfã no último caso.
- Smoke da imagem Billing: JWT obrigatório, OpenAPI exportado, cobrança persistida após restart e mensagem desconhecida encerrando host com código 1 sem confirmar seu offset.
- BDD: seis cenários aprovados com OS, Execution e Billing em imagens reais, PostgreSQLs separados, MongoDB replica set e Kafka compartilhado. Cobrem conclusão, cancelamento/replay, decisões concorrentes, retomada do Execution, espera pelo Billing e compensação com devolução única de estoque.
- Recusa no BDD é um responder Kafka de teste, enquanto Billing real fica parado; confirma explicitamente seu próprio offset no grupo isolado após publicar a recusa. O resultado recusado do gateway real substituível é validado nos testes Billing. O mock de produção sempre aprova.
- Composes independente e integrado passaram por `docker compose config --quiet`. Builds das três imagens concluíram localmente.

Logs e relatórios ficam em artifacts (ignorados pelo Git) de cada repositório. A cobertura Billing exclui testes, migrations e fontes geradas; o CI mantém o limite mínimo de 80%. Consulte o relatório Cobertura da execução mais recente para o valor exato.

## Limites e liberação

Sem publicação NuGet, execução remota de CI/Sonar ou deploy AWS nesta validação. OS.Application.Messaging 0.2.0 e Billing.Application.Messaging 0.1.0 estão preparados como candidatos; publicar contratos antes de exigir restore público dos consumidores. Restore local explícito e Docker aceitam nupkg candidatos sem referência ao código-fonte de outro serviço.

A sequência persistir/publicar/commit não garante atomicidade entre banco, Kafka e gateway. Entrega incerta exige reconciliação conforme o padrão canônico. Sem Outbox, retry de aplicação, DLQ, estorno, nova tentativa comercial ou cancelamento administrativo após aprovação. Os bancos de demonstração podem começar vazios; não há cobrança retroativa nem migração de histórico legado.

## Correções após revisão

A revisão identificou e corrigiu a aceitação de campos monetários ausentes e a falta de espera pelo consumidor no teste de reentrega. Validação posterior: 21 testes Billing, 244 OS e 6 BDD aprovados, totalizando 271. Cobertura Billing sem imagem: 92,22%. Consulte o [relatório da revisão e correções](revisao-billing-saga.md) para os cenários e logs correspondentes.
