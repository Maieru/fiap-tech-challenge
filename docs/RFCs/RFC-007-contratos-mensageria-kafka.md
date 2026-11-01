# RFC-007 — Contratos de integração do OS por NuGet

- **Status:** Aceita
- **Data:** 2026-11-01
- **Última revisão:** 2026-11-01
- **Responsáveis:** equipe
- **Escopo:** contratos OS, compatibilidade e publicação

## Contexto

A extração do Execution exige comunicação Kafka, mantendo a aplicação simples e sem referências entre os projetos dos diferentes serviços.

## Decisão

Criar FIAP.TechChallenge.OS.Application.Messaging 0.1.0, sem dependências externas, com contratos
OrdemServicoCriada, ExecucaoAutorizada e CancelamentoSolicitado.
Usar JSON camelCase, versão 1 no envelope e tópico oficina.os.events.v1.
Interface e adaptador técnico ficam no repositório do chassi; contratos Execution pertencem ao repositório Execution.
O [padrão canônico](../arquitetura/mensageria.md)
descreve BackgroundService com switch explícito, parada em falha e publicação direta com rollback limitado.

## Alternativas consideradas

- HTTP síncrono entre os novos serviços: não atende à comunicação Kafka escolhida.
- Contratos e domínio no mesmo pacote: acopla modelos internos aos consumidores.
- Outbox: rejeitada neste incremento em favor da publicação direta, com os limites explicitados.

## Consequências

Os contratos OS e Execution estão publicados no NuGet na versão 0.1.0, com testes e workflows de publicação próprios.
O OS publica as entradas do Execution e recebe suas confirmações; os dois serviços usam BackgroundService e casos de uso específicos.
A [integração das duas aplicações](RFC-009-integracao-os-execution.md) está implementada e verificada com imagens reais.
A transação local não pode garantir atomicidade com Kafka.
Os contratos não alteram as versões do chassi consumidas pelo OS e pelo legado.

## Evidências e relações

- [Contratos OS](../../src/Services/OrdensServico/FIAP.TechChallenge.OS.Application.Messaging/README.md).
- [Testes de compatibilidade](../../src/Services/OrdensServico/FIAP.TechChallenge.OS.Application.Messaging.Tests/ContractTests.cs).
- [Publicação](../../.github/workflows/publish-os-messaging.yml).
- [ADR-013](../ADRs/ADR-013-chassi-em-repositorio-proprio.md).
- [ADR-014](../ADRs/ADR-014-contratos-mensageria.md).
