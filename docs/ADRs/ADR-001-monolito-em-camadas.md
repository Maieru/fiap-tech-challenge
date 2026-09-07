# ADR-001 — Monólito em camadas com domínio no centro

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** arquitetura da aplicação
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

A gestão da oficina reúne regras relacionadas e usa um único banco transacional. O núcleo precisa permanecer independente de HTTP e persistência.

## Decisão

Manter o backend como monólito em camadas, organizado por casos de uso:

- `Domain`: entidades, regras, value objects, resultados e interfaces.
- `Application`: coordenação dos casos de uso.
- `Infrastructure`: EF Core, repositórios, segurança, notificações e telemetria.
- `API`: controllers, HTTP e composição das dependências.
- `Frontend`: SPA implantada separadamente e integrada por HTTP.

Application e Infrastructure referenciam Domain; API referencia Application e Infrastructure. Domain não referencia essas camadas. Os módulos do backend são publicados e escalados juntos.

## Alternativas consideradas

- **Microserviços:** acrescentariam operação e consistência distribuída sem necessidade demonstrada de escala independente.
- **Regras nos controllers ou serviços de CRUD:** simplificariam o início, mas espalhariam invariantes e acoplariam o negócio a detalhes externos.

## Consequências

- O domínio pode ser testado sem banco ou servidor HTTP.
- Interfaces e mappers aumentam o código; mudanças transversais podem atingir várias camadas.
- Controllers de negócio delegam aos casos de uso. O controller de health acessa `AppDbContext` diretamente para verificar readiness.
- Regras de domínio devem permanecer nas entidades e value objects; integrações implementam interfaces do núcleo.

Reconsiderar quando um módulo precisar de escala, disponibilidade ou implantação independente.

## Evidências e relações

- [Solução e projetos](../../src/TechChallengeFase1.slnx)
- [Referências da Application](../../src/FIAP.TechChallenge.Fase1.Application/FIAP.TechChallenge.Fase1.Application.csproj)
- [Referências da Infrastructure](../../src/FIAP.TechChallenge.Fase1.Infrastructure/FIAP.TechChallenge.Fase1.Infrastructure.csproj)
- [Casos de uso](../../src/FIAP.TechChallenge.Fase1.Application/UseCases)
- [Visão das camadas](../ARQUITETURA.md#camadas-da-aplicação)
