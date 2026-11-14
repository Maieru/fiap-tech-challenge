# Decisões arquiteturais

Este catálogo reúne 23 registros aceitos do projeto. Os documentos apresentam contexto, decisão, alternativas, consequências e evidências. Propostas ainda abertas ficam em issues ou pull requests.

## Catálogo

| Registro | Status |
| --- | --- |
| [ADR-001 — Arquitetura em camadas com domínio no centro](ADR-001-arquitetura-em-camadas.md) | Aceita |
| [ADR-002 — Comunicação síncrona por HTTP/JSON e REST pragmático](ADR-002-comunicacao-rest-json.md) | Aceita |
| [ADR-003 — Persistência por contratos e repositórios](ADR-003-persistencia-por-repositorios.md) | Aceita |
| [ADR-004 — Preservação do histórico operacional](ADR-004-preservacao-do-historico.md) | Aceita |
| [ADR-005 — Entrada pública por API Gateway e ALB interno](ADR-005-topologia-de-entrada-na-aws.md) | Aceita |
| [ADR-006 — HPA das APIs baseado em CPU](ADR-006-hpa-do-backend-por-cpu.md) | Aceita |
| [ADR-007 — Gestão centralizada de segredos](ADR-007-gestao-centralizada-de-segredos.md) | Aceita |
| [ADR-008 — Separação de repositórios e estados Terraform](ADR-008-estados-terraform-e-repositorios.md) | Aceita |
| [ADR-009 — Observabilidade com OpenTelemetry e New Relic](ADR-009-observabilidade-com-opentelemetry.md) | Aceita |
| [ADR-010 — Ambiente remoto efêmero orientado a custo](ADR-010-ambiente-efemero-orientado-a-custo.md) | Aceita |
| [ADR-011 — Responsabilidade e isolamento do OS Service](ADR-011-extracao-incremental-os-monorepo.md) | Aceita |
| [ADR-012 — PostgreSQL exclusivo e histórico independente dos cadastros ativos](ADR-012-dados-historico-os.md) | Aceita |
| [ADR-013 — Chassi em repositório próprio](ADR-013-chassi-em-repositorio-proprio.md) | Aceita |
| [ADR-014 — Contratos por produtor e publicador Kafka separado](ADR-014-contratos-mensageria.md) | Aceita |
| [ADR-015 — Execution com MongoDB e transações locais](ADR-015-execution-mongodb.md) | Aceita |
| [ADR-016 — Decisões administrativas e projeção do Execution no OS](ADR-016-integracao-os-execution.md) | Aceita |
| [ADR-017 — Billing e Saga de pagamento no OS](ADR-017-billing-saga-pagamento.md) | Aceita |
| [ADR-018 — Identidade administrativa temporariamente no OS](ADR-018-autenticacao-administrativa-no-os.md) | Aceita |
| [ADR-019 — Entrega e acesso do cliente à ordem no OS](ADR-019-entrega-acesso-cliente.md) | Aceita |
| [ADR-020 — Frontend distribuído por prefixos e mesma origem](ADR-020-frontend-microsservicos-mesma-origem.md) | Aceita |
| [ADR-022 — Microsserviços no EKS com HTTP interno e credenciais isoladas](ADR-022-kubernetes-microsservicos.md) | Aceita |
| [ADR-023 — Entrega independente por repositório com OIDC e digest](ADR-023-entrega-servicos-ecr-eks.md) | Aceita |
| [ADR-024 — Plataforma efêmera AWS dos microsserviços](ADR-024-plataforma-microsservicos-aws.md) | Aceita |

## Como consultar e manter

A data original identifica o registro e a última revisão indica atualização do texto/evidências. Avaliações retrospectivas não são atas de reuniões. Correções documentais devem refletir a implementação atual; resultados de testes em datas específicas não comprovam o último CI remoto ou deploy.

Use o [template](TEMPLATE.md) para um registro novo e mantenha este índice sincronizado com os arquivos. Decisões técnicas e arquiteturais relevantes acompanham sua implementação. Não inclua registros em rascunho neste catálogo; preserve IDs e links ao atualizar títulos.

Consulte o [índice técnico](../README.md) e o [estado da aplicação](../arquitetura/estado-da-migracao.md) para visão de conjunto.
