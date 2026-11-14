# ADR-003 — Persistência por contratos e repositórios

- **Status:** Aceita
- **Data:** 2026-08-23
- **Última revisão:** 2026-11-14
- **Escopo:** aplicação e persistência

## Contexto

As regras precisam permanecer independentes do EF Core e do driver MongoDB. Cada serviço coordena suas gravações no próprio banco.

## Decisão

Definir contratos de persistência no núcleo e implementá-los em Infrastructure. Usar mappers explícitos quando houver modelos distintos de domínio e persistência. Não expor `IQueryable` ou tipos concretos de persistência nas interfaces de negócio.

Casos de uso coordenam transações locais pelas abstrações de unidade de trabalho. Propagar `CancellationToken` em operações assíncronas. OS e Billing usam PostgreSQL; Execution usa MongoDB com suporte a transações.

## Alternativas consideradas

DbContext na Application acoplaria os casos de uso à persistência. Um repositório genérico esconderia consultas específicas. Transações distribuídas ampliariam a coordenação entre bancos independentes.

## Consequências

O núcleo permanece testável sem persistência concreta. Transações locais não tornam atômicos banco, Kafka e gateway de pagamento. Entrega incerta exige reconciliação; os limites de publicação estão documentados no padrão de mensageria e na Saga.

## Evidências e relações

- [Contratos de domínio](../../src/FIAP.TechChallenge.OS.Domain/Interfaces).
- [Repositórios OS](../../src/FIAP.TechChallenge.OS.Infrastructure/Persistence/Repositories).
- [Unidade de trabalho](../../src/FIAP.TechChallenge.OS.Application/Abstractions/IUnitOfWork.cs).
- [Mensageria](../arquitetura/mensageria.md) e [Saga](../arquitetura/saga-pagamento.md).
