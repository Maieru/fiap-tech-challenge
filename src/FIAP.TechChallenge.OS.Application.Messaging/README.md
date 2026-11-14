# OS Application.Messaging

Contratos públicos de integração do OS, em .NET 10. O pacote FIAP.TechChallenge.OS.Application.Messaging declara versão 0.2.0, sem dependência de domínio, persistência, Kafka ou chassi. A versão do contrato no envelope é 1 e independe da versão NuGet.

## Eventos

| Payload | Type | Tópico padrão |
| --- | --- | --- |
| `CancelamentoSolicitado` | `os.cancelamento-solicitado` | `oficina.os.events.v1` |
| `ExecucaoAutorizada` | `os.execucao-autorizada` | `oficina.os.events.v1` |
| `OrdemServicoCriada` | `os.ordem-servico-criada` | `oficina.os.events.v1` |
| `PagamentoSolicitado` | `os.pagamento-solicitado` | `oficina.os.billing.requests.v1` |

Use OSMessaging.Topic e, para pedidos de pagamento, OSMessaging.BillingTopic, a constante Version e a constante Type do payload. A chave Kafka é OrdemServicoId no formato UUID com hífens.

O publicador usa `MessageEnvelope<T>` do chassi, com MessageId estável em reentrega, instante UTC, correlação, causação e contexto de trace. JSON é camelCase, IDs são UUID e valores monetários são decimais em BRL. O pacote contém somente payloads/constantes: transporte, serialização, validação, offsets, idempotência e coordenação com banco pertencem aos serviços.

A versão 0.2.0 acrescenta PagamentoSolicitado e BillingTopic. Os eventos de abertura, autorização e cancelamento continuam compatíveis com 0.1.0, versão usada pelo Execution. Billing consome o pacote OS 0.2.0.

## Consumo, build e teste

No projeto consumidor, selecione uma versão disponível no feed:

```powershell
dotnet add package FIAP.TechChallenge.OS.Application.Messaging --version 0.2.0
```

Na raiz do repositório produtor, com .NET SDK 10:

```powershell
dotnet pack src/FIAP.TechChallenge.OS.Application.Messaging -c Release -o artifacts/packages
dotnet test src/FIAP.TechChallenge.OS.Application.Messaging.Tests -c Release
```

Para testar candidatos, use versão NuGet inédita e passe `-p:PackageVersion=<versao-candidata>` ao pack. A tag de publicação é os-messaging-vX.Y.Z, no workflow publish-os-messaging.yml, com commit integrado em main, NUGET_USER e Trusted Publishing configurados. Não confunda gerar o pacote localmente com publicá-lo.

## Compatibilidade

Não renomeie tipos/campos nem altere seu significado dentro da versão 1. Campos novos precisam ser opcionais para os consumidores existentes. Mudança incompatível exige nova versão de contrato e migração explícita. Uma versão NuGet nova não autoriza mudar silenciosamente a semântica do envelope.

Consulte o [padrão de mensageria](https://github.com/Maieru/fiap-tech-challenge/blob/main/docs/arquitetura/mensageria.md) para validação, publicação e reconciliação.
