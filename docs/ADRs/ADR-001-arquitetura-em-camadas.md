# ADR-001 — Arquitetura em camadas com domínio no centro

- **Status:** Aceita
- **Data:** 2026-08-23
- **Última revisão:** 2026-11-14
- **Escopo:** organização interna dos microsserviços

## Contexto

OS, Execution e Billing possuem responsabilidades, dados e implantação independentes. Em cada serviço, as regras de negócio precisam permanecer independentes de HTTP, persistência e mensageria concretos.

## Decisão

Organizar cada serviço em Domain, Application, Infrastructure e API. Domain concentra entidades, value objects, resultados e contratos; Application coordena casos de uso; Infrastructure implementa persistência e integrações; API traduz HTTP e compõe dependências. O frontend é uma SPA integrada às APIs por HTTP.

Application depende de Domain. Infrastructure implementa os contratos do núcleo. Controllers e consumidores delegam aos casos de uso. Contratos de mensagens são distribuídos em pacotes próprios por produtor.

## Alternativas consideradas

- Regras em controllers ou consumidores: acoplam invariantes aos mecanismos de entrada.
- Compartilhar assemblies de negócio entre serviços: reduz a autonomia de evolução.

## Consequências

O domínio pode ser testado sem servidor HTTP ou banco. Interfaces e mappers acrescentam código, mas mantêm os detalhes externos fora das regras de negócio. Cada serviço conserva sua própria unidade de implantação e persistência.

## Evidências e relações

- [Solução OS](../../Backend.slnx).
- [Casos de uso](../../src/FIAP.TechChallenge.OS.Application/UseCases).
- [Contratos de domínio](../../src/FIAP.TechChallenge.OS.Domain/Interfaces).
- [Visão da arquitetura](../arquitetura/README.md).
