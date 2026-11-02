# Validação ponta a ponta dos três serviços

Validação inicial em 2026-11-02, anterior ao login OS e à entrega. As três imagens foram reconstruídas a partir dos checkouts atuais. A suíte `Distributed.slnx` passou com **6 cenários, 0 falhas**, em aproximadamente 41 segundos após os builds.

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

O fluxo atual dos três backends funciona com o gateway mockado nos cenários executados, incluindo reentrega e retomada. A execução inicial não cobria login do OS nem entrega administrativa. Os resultados adicionais abaixo cobrem essas funcionalidades. Pagamento real no Mercado Pago, frontend e deploy AWS permanecem fora desta validação.

A publicação direta antes do commit continua sem atomicidade entre banco, Kafka e gateway. Esta execução não elimina a necessidade de reconciliação em entrega incerta nem demonstra tolerância a todas as falhas de infraestrutura. Consulte a [Saga](../arquitetura/saga-pagamento.md), as [correções da revisão](revisao-billing-saga.md) e o [estado da migração](../arquitetura/estado-da-migracao.md).

## Validação adicional — acompanhamento e entrega (2026-11-02)

A suíte distribuída passou com **9 cenários, 0 falhas**, em aproximadamente 46 segundos. OS foi reconstruído com as novas funcionalidades; as imagens Execution/Billing foram reutilizadas porque seu código de produção não mudou. Os JWTs administrativos são obtidos por cadastro/login real no OS. As chamadas do cliente são feitas sem JWT, usando somente o token privado da ordem.

Dois novos cenários verificam consulta do orçamento de R$ 140,55, aprovação pelo link com publicação única, pagamento confirmado, execução e entrega administrativa idempotente; e cancelamento pelo link com devolução de cinco unidades ao estoque, sem nova devolução após replay. Uma confirmação tardia de conclusão não regride `Entregue` (OS 6). Execution continua `Finalizada` (5) e Billing confirmado.

Os 265 testes OS sem imagem passaram (136 domínio, 66 aplicação, 4 contratos e 59 integração). Dois testes da imagem também passaram. A integração HTTP verificou rotação, token de outra ordem, hash persistido, ausência de campos internos na resposta, JWT para emissão/entrega e entrega concorrente com histórico único.

Artefatos locais: OS `artifacts/customer-delivery/` e `artifacts/customer-delivery-image/`; Execution `artifacts/customer-delivery-distributed/` e `artifacts/customer-delivery-distributed.log`. O [guia de acompanhamento e entrega](../arquitetura/acompanhamento-e-entrega.md) descreve o contrato e os limites. Na execução registrada nesta seção, o frontend ainda usava o legado.

Revalidação da imagem final após incluir os schemas de resposta no OpenAPI: **9/9 cenários distribuídos aprovados**, em aproximadamente 54 segundos. A suíte OS com coleta de cobertura manteve **265/265 testes aprovados**. ReportGenerator 5.5.12 consolidou **90,9% de linhas cobertas**, acima do gate de 80%; relatórios em `artifacts/customer-delivery-coverage-summary/`. A stack local recebeu a nova imagem OS e aplicou a migration; readiness das três APIs e Scalar OS responderam 200. A especificação final inclui schemas de acompanhamento, decisão do cliente e entrega, exportada em `artifacts/os-openapi.json`.

## Validação posterior — frontend migrado (2026-11-02)

O frontend foi adaptado aos três serviços e verificado separadamente: nove testes Node/Vite, build TypeScript/Vite, imagem Nginx, smoke HTTP com APIs/Kafka/bancos isolados e navegação desktop/móvel. Foram exercitados login, cadastro em etapas com retomada após erro, orçamento, rotação de link, pagamento mockado, execução, entrega e cancelamento idempotente. Esses resultados não alteram as contagens das suítes backend acima. Consulte o [relatório do frontend](validacao-frontend.md), que também registra as limitações de lint, recuperação do consumidor Kafka e implantação AWS.
