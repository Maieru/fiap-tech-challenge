# ADR-011 — Extração incremental do OS Service em monorepo

- **Status:** Aceita
- **Data:** 2026-10-31
- **Escopo:** primeiro incremento da Fase 4

## Contexto

O monólito reúne cadastros, orçamento e execução. A fase 4 exige independência entre serviços, mas a primeira entrega precisa preservar o fluxo atual e aproveitar domínio, aplicação e testes. A equipe decidiu manter o monorepo temporariamente e executar o OS em paralelo, apenas localmente e no CI.

## Alternativas consideradas

- Referenciar Domain/Application completos do legado: menos alterações iniciais, com dependência de responsabilidades alheias ao OS.
- Compartilhar módulos de negócio temporários: evita cópias, mas exige uma reorganização maior do legado e coordenação de mudanças.
- Extrair código pertinente para projetos próprios do OS: permite separar a responsabilidade e adiar a migração de tráfego.

## Decisão

Extrair as regras e casos de uso pertinentes, preservar suas validações e transportar seus testes. Manter o legado em funcionamento, sem referência do OS aos seus assemblies ou banco. Clientes e veículos pertencem inicialmente ao OS. Compartilhar somente recursos técnicos por pacote NuGet versionado.

Cada componente tem verificação independente e artefato próprio; alterações exclusivas dos novos componentes não acionam o provisionamento AWS atual. A branch de implementação é `inicio-segregacao-servicos`.

## Consequências

Há duplicação temporária de código de negócio, documentada no inventário de extração. Mudanças no legado durante a coexistência precisam ser confrontadas antes da migração definitiva. O OS ainda não oferece todo o ciclo da ordem. A separação em repositórios e o deploy AWS permanecem pendentes para conformidade final.

## Evidências

- [Arquitetura e inventário](../arquitetura/fase4-incremento1.md).
- [OS Service](../../src/Services/OrdensServico/README.md).
- [CI](../operacao/monorepo-ci.md).

As ADRs 001 e 008 continuam descrevendo o legado e a infraestrutura existente.

A [ADR-013](ADR-013-chassi-em-repositorio-proprio.md) atualiza a organização do chassi, extraído para repositório próprio. A coexistência temporária de legado e OS permanece neste monorepo.
