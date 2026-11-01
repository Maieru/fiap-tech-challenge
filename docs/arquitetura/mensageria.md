# Padrão de mensageria entre serviços

Este guia é mantido no repositório principal e define o padrão compartilhado por OS, Execution e chassi.
A organização dos contratos e do publicador está registrada na [ADR-014](../ADRs/ADR-014-contratos-mensageria.md).

Decisão aceita em 2026-11-01. OS e Execution já publicam e recebem mensagens segundo este padrão,
com transações locais, deduplicação e testes das duas APIs reais. Ambos os pacotes de contratos
Application.Messaging estão publicados no NuGet na versão 0.1.0.
Consulte a [integração implementada](execution.md) e o [estado da migração](estado-da-migracao.md).

## Responsabilidades e dependências

- Domain contém regras e entidades; não conhece Kafka.
- Application contém os casos de uso e recebe IMessagingService por injeção.
- Cada produtor mantém seu projeto Application.Messaging, contendo apenas contratos de integração.
- Infrastructure referencia o pacote Kafka, registra o adaptador e implementa persistência.
- Um BackgroundService no host de cada serviço recebe mensagens e delega a casos de uso de Application.
- Não usar dispatcher por reflexão, mediator, registro dinâmico de handlers, outbox, DLQ ou retry de aplicação neste incremento.

O chassi expõe MessageEnvelope<T> e IMessagingService. O adaptador distribuído separadamente não adiciona Confluent.Kafka
às dependências dos consumidores que usam apenas o chassi.

```csharp
services.AddKafkaMessaging(config =>
{
    config.BootstrapServers = configuration["Kafka:BootstrapServers"];
    config.ClientId = "oficina-execution";
});
```

O produtor é singleton e pertence ao contêiner de DI. O adaptador exige idempotência do produtor e acks=all,
serializa JSON camelCase e aguarda confirmação de persistência. O timeout de entrega padrão é 10 segundos.
A implementação segue a [API oficial Confluent .NET](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html).
Isso não fornece exactly-once de negócio nem atomicidade com banco.

## Contrato de mensagem

O envelope existente é preservado, sem quebra de API:

| Campo | Regra |
| --- | --- |
| MessageId | UUID da mensagem; preservar ao reenviar a mesma mensagem |
| Type e Version | Identificação estável publicada pelo produtor; contratos iniciais usam versão 1 |
| OccurredAtUtc | Instante original da transição em UTC; não substituir no reenvio |
| CorrelationId | Propagar a correlação da requisição/mensagem de origem |
| CausationId | MessageId da mensagem que causou a nova emissão, quando houver |
| TraceParent e TraceState | Propagar contexto W3C quando presente |
| Payload | DTO do produtor, sem entidades de domínio ou dados consultados de outro banco |

IDs são UUID; a chave Kafka é OrdemServicoId.ToString("D"). Valores monetários são decimal em BRL.
Datas da transição ficam no envelope. O pacote Execution publica snapshots de serviços e peças no diagnóstico;
as validações e os cálculos ficam no domínio/aplicação, não nos contratos NuGet.

| Produtor | Tópico | Tipos publicados |
| --- | --- | --- |
| OS | oficina.os.events.v1 | os.ordem-servico-criada, os.execucao-autorizada, os.cancelamento-solicitado |
| Execution | oficina.execution.events.v1 | execution.diagnostico-iniciado, execution.diagnostico-concluido, execution.execucao-iniciada, execution.execucao-concluida, execution.execucao-cancelada |

Usar grupos oficina-os-v1 e oficina-execution-v1, diferentes entre serviços e iguais entre réplicas do mesmo serviço.
Ambiente local começa com uma partição por tópico. Kafka ordena dentro de cada partição, não entre tópicos.
As pré-condições do domínio continuam obrigatórias.

Versão NuGet e Version do envelope são independentes. Fixar PackageReference por versão explícita.
Não renomear/remover campos nem mudar seu significado em contratos v1. Adições precisam ser opcionais para leitores antigos.
Tipos novos exigem atualização dos switches assinantes antes da ativação do produtor, pois o padrão atual rejeita tipos desconhecidos.
Mudanças incompatíveis exigem migração explícita de contrato/tópico; não assumir aceitação silenciosa.

## Publicação dentro do caso de uso

Cada caso de uso com publicação segue esta ordem, por meio da unidade de trabalho local:

1. Abrir transação e validar as pré-condições dentro dela quando houver concorrência.
2. Gravar todas as alterações e eventuais registros de idempotência usando a mesma sessão/conexão.
3. Chamar e aguardar IMessagingService.PublishAsync.
4. Fazer commit do banco e somente então retornar sucesso.
5. Se qualquer etapa anterior ao commit falhar, tentar rollback/abort e propagar o erro. Não transformar falha em sucesso.

O publicador não abre nem confirma transações. No PostgreSQL, usar transação explícita do contexto.
No MongoDB, usar sessão/transação explícita compartilhada pelos repositórios, com replica set no ambiente local.
Não executar publicação em callbacks transacionais que possam repetir o caso de uso automaticamente:
a [API de transações MongoDB pode repetir callbacks](https://www.mongodb.com/docs/drivers/csharp/current/crud/transactions/).

Esta é uma publicação direta com rollback limitado, escolhida explicitamente no planejamento.
A gravação precisa ter sucesso antes da publicação, mas ainda não está committed.
Mensagem pode chegar ao consumidor antes do commit da origem.

**Não existe commit atômico entre Kafka e banco.** Se a entrega ocorrer e o commit falhar, a mensagem pode ficar órfã.
Timeout ou cancelamento não prova ausência de entrega. Rollback também pode falhar, e erro de commit pode ter resultado incerto.
Registrar a incerteza e reconciliar; não afirmar que o negócio foi desfeito sem verificar.
Essa limitação decorre da [coordenação necessária com sistemas externos ao Kafka](https://kafka.apache.org/40/design/design/).

Os testes de OS e Execution cobrem publicação falhando com rollback e publicação entregue seguida de falha no commit.

## Consumo por BackgroundService

Consumidor concreto pertence ao próprio serviço. Não haverá consumidor genérico no pacote.
Configuração mínima:

```csharp
new ConsumerConfig
{
    BootstrapServers = configuration["Kafka:BootstrapServers"],
    GroupId = "oficina-execution-v1",
    EnableAutoCommit = false,
    EnableAutoOffsetStore = false,
    AutoOffsetReset = AutoOffsetReset.Earliest
};
```

Para cada mensagem: ler e validar envelope/versão/chave; criar escopo com IServiceScopeFactory.CreateAsyncScope;
restaurar correlação/trace; executar switch explícito e chamar o caso de uso correspondente.
O [escopo por operação é necessário no BackgroundService](https://learn.microsoft.com/en-us/dotnet/core/extensions/scoped-service).

Exemplo simplificado do roteamento do Execution:

```csharp
switch (envelope.Type, envelope.Version)
{
    case (OrdemServicoCriada.Type, OSMessaging.Version):
        // Desserializar payload e chamar RegistrarOrdemRecebidaUseCase.
        break;
    case (ExecucaoAutorizada.Type, OSMessaging.Version):
        // Desserializar payload e chamar IniciarExecucaoUseCase.
        break;
    case (CancelamentoSolicitado.Type, OSMessaging.Version):
        // Desserializar payload e chamar CancelarExecucaoUseCase.
        break;
    default:
        throw new InvalidOperationException("Tipo ou versão de mensagem não suportado.");
}
```

Não colocar regra de negócio no switch. Desserialização não substitui validação.
Registrar a deduplicação por (consumidor, MessageId), com unicidade e na mesma transação da alteração.
Uma mensagem já concluída retorna sucesso sem repetir efeitos. Publicações derivadas seguem a sequência transacional acima.
Confirmar o offset somente após o commit. Se cair antes do commit do offset, a redelivery será deduplicada.

Qualquer falha de processamento ou commit de offset interrompe o loop, registra tópico/partição/offset/MessageId,
define código de saída não zero e encerra o host (BackgroundServiceExceptionBehavior.StopHost).
Não continuar consumindo mensagens posteriores. Close/Dispose do consumidor em finally.
Cancelamento solicitado pelo host é encerramento normal. Não fazer retry automático da aplicação nem enviar a DLQ.
Retries internos do cliente Kafka permanecem habilitados.

## Correção, reinício e reconciliação

1. Parar o consumidor afetado e registrar grupo, tópico, partição, offset, MessageId, correlação e erro.
2. Verificar os dados confirmados no produtor e no consumidor; em entrega incerta, verificar também o log Kafka.
3. Corrigir infraestrutura/configuração/código ou dados por procedimento administrativo explícito.
4. Reiniciar com o mesmo grupo: a mensagem cujo offset não foi confirmado volta a ser processada.
5. Para reenvio técnico, preservar envelope e chave originais. Validar idempotência e o estado de negócio antes de reenviar.
6. Mensagem órfã exige conciliação explícita do estado. Não repor estoque ou desfazer transições automaticamente sem verificar o que foi committed.
7. Mensagem inválida pode bloquear o grupo. Não descartar nem avançar offsets silenciosamente; qualquer descarte exige decisão operacional documentada.

No ambiente local, não configurar reinício automático dos hosts que esconda esse estado de falha.
Os testes Kafka criam e removem tópicos exclusivos, sem alterar os tópicos de desenvolvimento.

## Integração OS e Execution

O OS publica abertura, autorização e solicitação de cancelamento. O Execution publica as cinco confirmações do ciclo.
Aprovação/cancelamento no OS retornam 202, preservando AguardandoAprovacao até a confirmação.
A mesma decisão não republica; a decisão oposta retorna 409. Status, datas, histórico e deduplicação da projeção
compartilham a transação PostgreSQL. Uma confirmação repetida não faz o status regredir.

Os dois serviços recebem por BackgroundService com switch explícito e casos de uso separados.
O ambiente integrado usa um broker e bancos exclusivos; o Compose integrado e o BDD ficam no repositório Execution.
Consulte [ADR-016](../ADRs/ADR-016-integracao-os-execution.md), [RFC-009](../RFCs/RFC-009-integracao-os-execution.md)
e [evidências da etapa 3](../operacao/revisao-execution-etapa3.md).

## Verificação local do chassi

Na raiz do repositório do chassi, com Docker Desktop e .NET 10:

```powershell
dotnet pack src/Maieru.FiapTechChallenge.Chassis -c Release -o artifacts/packages
dotnet pack src/Maieru.FiapTechChallenge.Messaging.Kafka -c Release -o artifacts/packages
dotnet build Chassis.slnx -c Release
docker compose -f tests/kafka.compose.yaml up -d --wait --wait-timeout 120
$env:KAFKA_TEST_BOOTSTRAP_SERVERS = '127.0.0.1:19092'
dotnet test Chassis.slnx -c Release --no-build
dotnet run --project samples/Maieru.FiapTechChallenge.Messaging.Sample -c Release --no-build -- 127.0.0.1:19092
docker compose -f tests/kafka.compose.yaml down
```

Sem KAFKA_TEST_BOOTSTRAP_SERVERS, somente o teste real do broker é ignorado; os testes de unidade continuam executando.
O CI define essa variável e exige o teste real, além do exemplo que consome o pacote por PackageReference.
Validação local não significa publicação NuGet, execução remota do CI ou aprovação Sonar.
