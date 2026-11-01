# RFC-008 — Persistência MongoDB e consumidor Kafka do Execution

- **Status:** Aceita
- **Data:** 2026-11-01
- **Última revisão:** 2026-11-01
- **Responsáveis:** equipe
- **Escopo:** implementação local do Execution e verificação

## Contexto

A base de contratos aprovada precisa de uma implementação concreta com pouco código técnico adicional.
O serviço deve conservar as regras do legado e permitir testes sem depender da API OS em execução.

## Decisão

Usar MongoDB.Driver 3.13.0, chassi 1.1.1 e adaptador Kafka 0.1.1, fixados por PackageReference.
O contrato OS.Application.Messaging 0.1.0 permanece independente da aplicação produtora.
Os contratos locais do Execution são referenciados por projeto dentro do próprio repositório.

ExecutionKafkaConsumer é um BackgroundService com switch explícito para OrdemServicoCriada,
ExecucaoAutorizada e CancelamentoSolicitado. Cada mensagem recebe escopo próprio.
Casos de uso concentram regras, transação local, deduplicação e publicação pela interface IMessagingService.

O consumidor confirma o offset depois do commit. Qualquer falha de processamento interrompe o host com saída não zero,
sem DLQ ou retry na aplicação. Registra tópico, partição, offset, MessageId e correlação.
Contexto W3C e correlação acompanham o processamento.

A API valida JWT administrativo sem chamar o legado. MongoDB e Kafka compõem readiness;
liveness não consulta essas dependências. OpenAPI/Scalar ficam disponíveis em Development/Testing.
OTLP é opcional.

Issuer e audience usam a seção Jwt de IConfiguration. A composição lê a chave explicitamente de Jwt__SigningKey
e registra uma SymmetricSecurityKey singleton, com mínimo de 32 bytes UTF-8, injetada nas opções JWT validadas no startup.
No Compose, JWT_SIGNING_KEY do .env é mapeada para Jwt__SigningKey; não há fallback da chave para IConfiguration/arquivo.
Segredos devem ser provisionados externamente, sem valores versionados no código/configuração.
Os testes HTTP usam chave criptográfica temporária por ambiente e restauram o valor anterior no encerramento/falha;
os testes que modificam essa variável não são paralelos. Containers recebem a variável explicitamente.

## Alternativas consideradas

- Consumidor genérico, mediator ou reflexão para handlers: aumentariam a abstração sem benefício para três entradas.
- Publicação e persistência dentro de controllers: retirariam a coordenação dos casos de uso.
- Testes com bancos/broker substitutos: não verificariam sessões, constraints, serialização ou offsets reais.

## Consequências

O ambiente de desenvolvimento usa Kafka KRaft e MongoDB replica set em Compose.
O CI verifica testes, cobertura mínima de 80%, imagem, OpenAPI e configura análise Sonar.
Sonar requer SONAR_TOKEN e SONAR_PROJECT_KEY no repositório Execution.

Chassi 1.1.1, adaptador Kafka 0.1.1 e OS.Application.Messaging 0.1.0 estão publicados no NuGet.
O restore padrão e o CI usam o feed público. O roteiro local é opcional para testar um nupkg candidato,
sem compilar outro repositório. Não usar seleção dinâmica de versões nem feeds implícitos.

## Evidências e relações

- [ADR-015](../ADRs/ADR-015-execution-mongodb.md).
- [Padrão](../arquitetura/mensageria.md).
- [Arquitetura](../arquitetura/execution.md).
- No checkout Execution: scripts/Restore-Local.ps1, compose.yaml, TransactionTests e ImageSmokeTests.
