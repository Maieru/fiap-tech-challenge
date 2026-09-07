# RFC-002 — PostgreSQL com EF Core e Amazon RDS

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** dados e persistência
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

Clientes, veículos, estoque e ordens possuem relacionamentos e gravações que exigem integridade transacional.

## Decisão

Usar PostgreSQL como banco operacional, acessado pelo backend com EF Core e Npgsql.

- Localmente, executar PostgreSQL em Docker Compose.
- Na AWS, usar RDS em sub-redes de banco e guardar a conexão no Secrets Manager.
- Restringir a porta 5432 aos security groups dos nós EKS e da Lambda Authorizer. A regra da Lambda pertence ao estado `serverless`.
- Manter migrations com a aplicação e isolar persistência por repositórios, conforme a ADR-003.
- O autorizador usa seu próprio contexto EF de leitura no mesmo banco.

## Alternativas consideradas

- **PostgreSQL autogerenciado:** transferiria manutenção do servidor para a equipe.
- **Outro banco relacional:** exigiria revisar provider, tipos e migrations sem ganho demonstrado.
- **NoSQL:** não simplificaria os relacionamentos e transações centrais.

## Consequências

- Local e AWS usam o mesmo motor, mas versões diferentes: PostgreSQL 16 no Compose e 18.3 no Terraform do RDS.
- O schema e suas mudanças precisam atender à API e ao autorizador.
- Migrations rodam no startup dos pods; a coordenação entre réplicas continua uma limitação operacional.
- A indisponibilidade do banco afeta readiness e autorização das rotas na borda.
- O RDS está marcado como `publicly_accessible = true`, embora o security group restrinja as origens. Essa configuração precisa ser revista para um ambiente durável.
- Backups automáticos estão desabilitados e a destruição não cria snapshot final, conforme a ADR-010.

Reconsiderar por requisitos de disponibilidade, distribuição ou padrões de acesso que o modelo atual não atenda.

## Evidências e relações

- [ER, relacionamentos e justificativa dos ajustes do modelo](../arquitetura/banco-de-dados.md)
- [Registro do Npgsql](../../src/FIAP.TechChallenge.Fase1.Infrastructure/InfraestructureDependecyInjection.cs)
- [Contexto EF Core](../../src/FIAP.TechChallenge.Fase1.Infrastructure/Persistence/AppDbContext.cs)
- [Infraestrutura do banco](https://github.com/Maieru/fiap-tech-challenge-db/tree/main/infra/database)
- [ADR-003 — Persistência por repositórios](../ADRs/ADR-003-persistencia-por-repositorios.md)
- [ADR-010 — Ambiente efêmero orientado a custo](../ADRs/ADR-010-ambiente-efemero-orientado-a-custo.md)
- [Acesso do autorizador ao banco](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/serverless/networking.tf)
