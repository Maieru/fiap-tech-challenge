# ADR-014 — Contratos por produtor e publicador Kafka separado

- **Status:** Aceita
- **Data:** 2026-11-01
- **Última revisão:** 2026-11-01
- **Escopo:** contratos, distribuição NuGet e dependências

## Contexto

OS e Execution precisam compartilhar mensagens sem compartilhar entidades ou casos de uso. O chassi já fornece um envelope genérico e possui consumidores HTTP que não precisam do cliente Kafka.

## Decisão

Adicionar IMessagingService ao chassi 1.1.0, preservando MessageEnvelope<T>.
Implementar KafkaMessagingService no pacote independente Maieru.FiapTechChallenge.Messaging.Kafka 0.1.0.
Manter OS.Application.Messaging e Execution.Application.Messaging nos repositórios de seus produtores.
Contratos contêm somente DTOs e identificação estável de mensagens. Consumidores usam versões NuGet explícitas.

## Alternativas consideradas

- Um pacote com todos os eventos de negócio: centralizaria a evolução de serviços independentes.
- Interface por produtor: duplicaria a abstração técnica.
- Kafka dentro do pacote principal: adicionaria cliente nativo e configuração Kafka aos consumidores HTTP.

## Consequências

O adaptador precisa de sua versão do chassi publicada antes de sua própria liberação. A decisão inicial definiu chassi 1.1.0 e adaptador 0.1.0.
OS e Execution atualmente consomem o chassi 1.1.1 e o adaptador 0.1.1, ambos públicos no NuGet.
Os pacotes de contratos não dependem do chassi nem de Kafka. O adaptador depende do chassi por PackageReference,
inclusive dentro do repositório do chassi, e tem publicação própria.
Não há consumidor genérico, persistência ou transação distribuída na biblioteca.

## Evidências

- [Interface](https://github.com/Maieru/fiap-tech-challenge-chassis/blob/main/src/Maieru.FiapTechChallenge.Chassis/IMessagingService.cs).
- [Adaptador](https://github.com/Maieru/fiap-tech-challenge-chassis/blob/main/src/Maieru.FiapTechChallenge.Messaging.Kafka/KafkaMessagingService.cs).
- [Testes](https://github.com/Maieru/fiap-tech-challenge-chassis/blob/main/tests/Maieru.FiapTechChallenge.Messaging.Kafka.Tests/KafkaMessagingTests.cs).
- [Padrão e limites transacionais](../arquitetura/mensageria.md).
