# ADR-010 — Ambiente remoto efêmero orientado a custo

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** operação e durabilidade
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

O projeto acadêmico precisa reduzir custos quando não está em demonstração. O ambiente AWS atual é efêmero, sem garantia de disponibilidade contínua ou retenção dos dados do banco.

## Decisão

- Agendar a destruição diária às `03:00 UTC` (meia-noite em São Paulo), sujeita a atrasos do GitHub Actions.
- Remover serverless, API Gateway, Ingresses/ALB, configurações e add-ons Kubernetes; depois destruir RDS e remover EKS.
- Preservar bootstrap, estados e recursos compartilhados conforme os módulos para permitir reconstrução.
- Usar dados de demonstração recriáveis. O RDS está configurado com `backup_retention_period = 0` e `skip_final_snapshot = true`.

Nomes de workflows contendo `production` não alteram essa classificação.

## Alternativas consideradas

- **Manter tudo ativo:** reduziria tempo de preparação, com custo contínuo.
- **Reduzir apenas pods/nós:** manteria custos do cluster e banco.
- **Pausar componentes:** exigiria outra automação, sem reproduzir o fluxo completo de reconstrução.

## Consequências

- O ambiente fica indisponível até nova implantação e os dados do RDS são perdidos.
- Reconstruções exercitam a automação, mas podem demorar ou falhar.
- Telemetria ainda em memória pode ser perdida; dados já enviados ao New Relic seguem a retenção desse serviço.
- Criação e destruição não compartilham uma trava única de execução; há risco de sobreposição.
- A repetição da destruição ainda pode falhar quando data sources dependem de recursos já removidos.
- Demonstrações exigem implantação e seed prévios; o ambiente não deve conter dados únicos ou de usuários reais.

Reconsiderar antes de uso contínuo ou com dados reais, definindo disponibilidade, backups, recuperação, segurança e retenção em nova decisão.

## Evidências e relações

- [Workflow agendado de destruição](../../.github/workflows/destroy-expensive-infrastructure.yml)
- [Ordem de criação](../../.github/workflows/initialize-and-deploy.yml)
- [Destruição da infraestrutura Kubernetes](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/.github/workflows/destroy-kubernetes-infrastructure.yml)
- [Configuração atual do RDS](https://github.com/Maieru/fiap-tech-challenge-db/blob/main/infra/database/rds.tf)
- [RFC-001 — Escolha da AWS](../RFCs/RFC-001-escolha-da-nuvem-aws.md)
- [RFC-002 — PostgreSQL no RDS](../RFCs/RFC-002-postgresql-no-amazon-rds.md)
- [ADR-008 — Estados Terraform e repositórios](ADR-008-estados-terraform-e-repositorios.md)
