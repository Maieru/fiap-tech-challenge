# RFC-002 — PostgreSQL com EF Core e Amazon RDS

- **Status:** Aceita
- **Data:** 2026-08-23
- **Última revisão:** 2026-11-14
- **Escopo:** dados relacionais de OS e Billing

## Contexto

Cadastros, ordens, histórico e cobranças exigem integridade e transações locais. Execution mantém catálogo e execução em MongoDB conforme sua decisão própria.

## Decisão

Usar PostgreSQL com EF Core e Npgsql em OS e Billing. Cada serviço tem banco, usuário, credenciais e migrations próprios. Na AWS, DB provisiona RDS privado e criptografado em sub-redes de banco; a aplicação recebe sua conexão pelo mecanismo de segredos. O ambiente local usa PostgreSQL no Docker Compose.

Restringir acesso de rede e privilégios ao necessário por serviço. Não realizar consultas ou compartilhamento de schemas entre OS e Billing. O modelo de persistência fica isolado por contratos e repositórios.

## Alternativas consideradas

PostgreSQL autogerenciado transferiria manutenção à equipe. Outro banco relacional exigiria revisar provider e migrations. MongoDB atende a outro contexto e não substitui as transações relacionais destes serviços.

## Consequências

Migrations devem ser compatíveis com rollout e concorrência entre pods. Indisponibilidade do banco afeta as operações e a readiness. A configuração do laboratório não mantém backup automático nem snapshot final no destroy; não oferece retenção durável de dados únicos.

## Evidências

- [Registro Npgsql OS](../../src/FIAP.TechChallenge.OS.Infrastructure/InfrastructureDependencyInjection.cs).
- [Contexto OS](../../src/FIAP.TechChallenge.OS.Infrastructure/Persistence/AppDbContext.cs).
- [Dados dos serviços](../arquitetura/banco-de-dados.md).
- [Guia AWS](../operacao/plataforma-aws.md).
