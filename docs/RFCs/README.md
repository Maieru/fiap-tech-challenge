# Escolhas técnicas aceitas

Este catálogo reúne 19 registros aceitos do projeto. Os documentos apresentam contexto, decisão, alternativas, consequências e evidências. Propostas ainda abertas ficam em issues ou pull requests.

## Catálogo

| Registro | Status |
| --- | --- |
| [RFC-001 — Adoção da AWS como provedor de nuvem](RFC-001-escolha-da-nuvem-aws.md) | Aceita |
| [RFC-002 — PostgreSQL com EF Core e Amazon RDS](RFC-002-postgresql-no-amazon-rds.md) | Aceita |
| [RFC-003 — Autenticação local com BCrypt e JWT](RFC-003-autenticacao-jwt-e-bcrypt.md) | Aceita |
| [RFC-004 — Execução de containers no Amazon EKS](RFC-004-containers-no-amazon-eks.md) | Aceita |
| [RFC-005 — Terraform e GitHub Actions com OIDC](RFC-005-terraform-e-github-actions-oidc.md) | Aceita |
| [RFC-006 — Chassi NuGet e verificação dos componentes](RFC-006-chassi-nuget-e-verificacao-monorepo.md) | Aceita |
| [RFC-007 — Contratos de integração do OS por NuGet](RFC-007-contratos-mensageria-kafka.md) | Aceita |
| [RFC-008 — Persistência MongoDB e consumidor Kafka do Execution](RFC-008-execution-mongodb-kafka.md) | Aceita |
| [RFC-009 — Integração Kafka do OS e BDD distribuído](RFC-009-integracao-os-execution.md) | Aceita |
| [RFC-010 — Billing, gateway mockado e contratos Kafka](RFC-010-billing-mock-kafka.md) | Aceita |
| [RFC-011 — Cadastro/login OS com BCrypt e JWT compatível](RFC-011-autenticacao-os-bcrypt-jwt.md) | Aceita |
| [RFC-012 — APIs de entrega e acompanhamento privado no OS](RFC-012-entrega-acompanhamento-os.md) | Aceita |
| [RFC-013 — Proxy configurável e contratos do frontend](RFC-013-proxy-frontend-microsservicos.md) | Aceita |
| [RFC-014 — Soluções e verificação de OS e frontend](RFC-014-solucoes-e-ci.md) | Aceita |
| [RFC-015 — Projetos OS diretamente em src](RFC-015-projetos-os-diretamente-src.md) | Aceita |
| [RFC-016 — Manifestos Kubernetes das APIs e integração OTLP](RFC-016-manifestos-kubernetes-microsservicos.md) | Aceita |
| [RFC-017 — Actions de publicação ECR e aplicação EKS](RFC-017-actions-ecr-eks.md) | Aceita |
| [RFC-018 — Orquestração e segurança da plataforma AWS](RFC-018-orquestracao-plataforma-aws.md) | Aceita |
| [RFC-019 — Checkout hospedado Mercado Pago](RFC-019-mercado-pago-checkout.md) | Aceita |

## Como consultar e manter

A data original identifica o registro e a última revisão indica atualização do texto/evidências. Avaliações retrospectivas não são atas de reuniões. Correções documentais devem refletir a implementação atual; resultados de testes em datas específicas não comprovam o último CI remoto ou deploy.

Use o [template](TEMPLATE.md) para um registro novo e mantenha este índice sincronizado com os arquivos. Decisões técnicas e arquiteturais relevantes acompanham sua implementação. Não inclua registros em rascunho neste catálogo; preserve IDs e links ao atualizar títulos.

Consulte o [índice técnico](../README.md) e o [estado da aplicação](../arquitetura/estado-da-migracao.md) para visão de conjunto.
