# ADR-003 — Persistência por contratos e repositórios

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** aplicação e persistência
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

O domínio deve ficar independente do EF Core. Operações que gravam em vários repositórios precisam manter consistência no mesmo PostgreSQL.

## Decisão

- Definir interfaces de persistência em `Domain/Interfaces` e implementá-las na Infrastructure.
- Separar entidades de domínio e de persistência com mappers explícitos.
- Expor operações específicas, sem `IQueryable` ou tipos do EF nas interfaces.
- Cada repositório chama `SaveChangesAsync`; fluxos com múltiplas gravações devem coordenar uma transação.
- Propagar `CancellationToken` e usar consultas sem tracking quando não houver atualização da instância.

## Alternativas consideradas

- **DbContext na Application ou Active Record:** acoplariam regras e casos de uso à persistência.
- **Repositório genérico:** esconderia consultas específicas em uma abstração ampla.
- **Eventos distribuídos:** adicionariam complexidade a gravações no mesmo banco.

## Consequências

- O núcleo permanece testável sem EF Core, ao custo de interfaces, entidades e mappers adicionais.
- `CriarOrdemServicoCompleta`, `AdicionarPecaInsumoOrdemServico` e `CancelarOrdemServico` usam `TransactionScope` assíncrono.
- `CriarOrdemServicoComClienteEVeiculo` não abre transação própria: a chamada direta pode deixar gravações parciais. Essa lacuna precisa ser corrigida ou coberta por um escopo externo explícito.
- A transação cobre o banco, não notificações ou outros recursos externos. Não há outbox nem garantia geral de idempotência ou controle de concorrência.

Reconsiderar com múltiplos bancos ou quando uma unidade de trabalho explícita simplificar a coordenação das gravações.

## Evidências e relações

- [Contratos de domínio](../../src/FIAP.TechChallenge.Fase1.Domain/Interfaces)
- [Repositórios EF Core](../../src/FIAP.TechChallenge.Fase1.Infrastructure/Persistence/Repositories)
- [Entidades de persistência](../../src/FIAP.TechChallenge.Fase1.Infrastructure/Persistence/Entities)
- [Mappers](../../src/FIAP.TechChallenge.Fase1.Infrastructure/Persistence/Mappers)
- [Caso de uso transacional composto](../../src/FIAP.TechChallenge.Fase1.Application/UseCases/OrdensServico/CriarOrdemServicoCompleta/CriarOrdemServicoCompletaUseCase.cs)
- [Fluxo composto sem transação própria](../../src/FIAP.TechChallenge.Fase1.Application/UseCases/OrdensServico/CriarOrdemServicoComClienteEVeiculo/CriarOrdemServicoComClienteEVeiculoUseCase.cs)
- [RFC-002 — PostgreSQL no Amazon RDS](../RFCs/RFC-002-postgresql-no-amazon-rds.md)
