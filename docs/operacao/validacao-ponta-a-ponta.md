# Validação ponta a ponta dos três serviços

Executada em 2026-11-02. As três imagens foram reconstruídas a partir dos checkouts atuais. A suíte `Distributed.slnx` passou com **6 cenários, 0 falhas**, em aproximadamente 41 segundos após os builds.

## Ambiente e fronteiras verificadas

Testcontainers iniciou as três APIs reais, PostgreSQL separado para OS e Billing, MongoDB replica set para Execution e Kafka compartilhado. As requisições HTTP usaram JWT real. Não houve substituição de casos de uso/repositórios nem acesso a bancos ou volumes de desenvolvimento. O gateway usado pelo Billing no fluxo aprovado foi o `MockPaymentGateway` de produção.

O cenário principal fez cadastro de cliente e veículo, abriu a ordem no OS, aguardou sua recepção no Execution, cadastrou serviço e peça, concluiu diagnóstico, aprovou pelo OS, aguardou pagamento/autorização, concluiu o serviço e finalizou a execução. Consultou diretamente as **três APIs** para comparar os dados.

## Resultado observado

| Verificação | Resultado |
| --- | --- |
| Ordem | `28312d38-e300-4322-be62-76977dff40a9` |
| Cobrança | `67023491-4614-4117-9030-6b0b3e6d7948` |
| Orçamento | `e74bccf1-57f7-4434-bbd9-60c7e782bcdd` |
| Valor Execution / Billing / OS | R$ 140,55 nos três |
| Composição | Serviço R$ 100,25 + duas peças de R$ 20,15 |
| Situação da cobrança | Confirmada |
| Identificador do gateway | `mock-28312d38-e300-4322-be62-76977dff40a9` |
| Status OS / Execution | Finalizada nos dois |
| Etapa da Saga | Concluida |
| Estoque | 5 iniciais, 2 utilizadas, 3 finais |
| Reentrega | Mesmo pedido, confirmação e autorização; mesma cobrança, mesmo histórico e uma única emissão de início da execução |
| Saúde após reentrega | Readiness 200 nas três APIs |

A suíte também verificou correlação, causação e trace na cadeia **pedido de pagamento → confirmação Billing → autorização OS → início Execution**. As duas cópias das mensagens de confirmação e autorização observadas no replay correspondem ao original e ao reenvio deliberado pelo teste; o processamento não publicou novas cópias derivadas.

## Demais cenários aprovados

- Cancelamento anterior à aprovação e reentrega sem nova devolução de estoque.
- Aprovação e cancelamento concorrentes: uma decisão aceita e convergência.
- Abertura com Execution parado e recepção após reinício.
- Aprovação com Billing parado: execução bloqueada até o Billing voltar.
- Recusa pelo contrato Billing: cancelamento, aprovação original preservada e devolução única de estoque.

O cenário de recusa usa o responder Kafka de teste com Billing parado. O mock de produção sempre aprova; a recusa do gateway substituível é verificada separadamente na suíte Billing.

## Evidências e reprodução

No checkout Execution, os artefatos locais desta execução estão em `artifacts/e2e-20261102.log`, `artifacts/e2e-20261102/` (TRX e `evidencia.json`). O JSON contém as respostas finais das três APIs, IDs de correlação, estoque e instante UTC, sem tokens ou segredos. O NUnit também anexa o JSON original ao resultado do cenário; o workflow distribuído coleta esses anexos.

Com Docker Desktop iniciado e as três imagens construídas:

```powershell
$env:OS_TEST_IMAGE = 'fiap-os:local'
$env:BILLING_TEST_IMAGE = 'fiap-billing:local'
$env:EXECUTION_TEST_IMAGE = 'fiap-execution:local'
dotnet test Distributed.slnx --logger 'console;verbosity=normal' --logger trx --results-directory artifacts/distributed
```

Imagens locais verificadas (IDs de imagem, não digests de publicação):

| Imagem | ID |
| --- | --- |
| `fiap-os:local` | `sha256:3a55ffeae2b1f861d4875c29f399722aac87cb20f780f573a231112324d7ad16` |
| `fiap-billing:local` | `sha256:9c77e964316070d5bae52c2ee03d171dd484f4118c490dc8de199b796a692921` |
| `fiap-execution:local` | `sha256:e669c07d17be3420a40478c4a5361946c7badb4dc195e198e8678b028dd29b50` |

## O que esta validação permite afirmar

O fluxo atual dos três backends funciona com o gateway mockado nos cenários executados, incluindo reentrega e retomada. Isso não comprova pagamento real no Mercado Pago, integração do frontend, entrega da ordem ao cliente ou deploy AWS. O JWT do teste é assinado pelo ambiente de teste; o fluxo de login/emissão de tokens no legado não é exercitado.

A publicação direta antes do commit continua sem atomicidade entre banco, Kafka e gateway. Esta execução não elimina a necessidade de reconciliação em entrega incerta nem demonstra tolerância a todas as falhas de infraestrutura. Consulte a [Saga](../arquitetura/saga-pagamento.md), as [correções da revisão](revisao-billing-saga.md) e o [estado da migração](../arquitetura/estado-da-migracao.md).
