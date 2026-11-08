# Validação local — integração Mercado Pago

Data: 2026-11-08. Implementação conforme [RFC-019](../RFCs/RFC-019-mercado-pago-checkout.md). O cadastro das credenciais, deploy AWS e compra externa foram adiados pelo solicitante.

## Resultados executados

| Verificação | Resultado |
| --- | --- |
| Build Billing .NET 10 | Aprovado |
| Testes Billing com PostgreSQL e Kafka descartáveis | 49 aprovados |
| Contratos Billing Messaging | 2 aprovados |
| Cobertura Billing | 91,24% das linhas (396/434); migrations excluídas pelas configurações existentes |
| Imagem `fiap-billing:mercadopago` e testes ContainerImage | Build e 2 testes aprovados; OpenAPI exportado |
| BDD OS/Billing/Execution com bancos e Kafka descartáveis | 9 cenários aprovados |
| Frontend | Build, 9 testes e lint:ci aprovados |
| Entrega Billing | 9 testes aprovados, incluindo referências ao novo ExternalSecret |
| Entrega principal | 10 testes aprovados |
| Preflight da plataforma | 3 testes aprovados: secrets obrigatórios em apply; destroy independente |
| Scripts Infra | 6 testes aprovados, incluindo escrita/limpeza do segredo em sucesso e falha |
| Estabilização do gateway público | 6 testes aprovados com servidor HTTP local |
| Terraform | Validação de aws-resources, bootstrap e kubernetes-configs aprovada; teste mock de isolamento do segredo por namespace aprovado |
| Migration | Sem alteração pendente no modelo EF; script SQL idempotente gerado |
| YAML | Workflows, manifests e Compose alterados analisados sem erros; encaminhamento dos secrets restrito ao job core |

As verificações não fizeram chamadas de pagamento ao Mercado Pago. O gateway foi exercitado com respostas HTTP simuladas; o BDD usou a implementação HTTP do Billing contra servidor Node descartável e confirmou via endpoint real de webhook com assinatura HMAC. OS e Execution usaram as imagens locais `fiap-os:local` e `fiap-execution:local`.

Os testes cobrem cobrança pendente sem publicação prematura, confirmação/recusa/cancelamento, zero sem chamada externa, precisão decimal, idempotência, assinatura inválida, divergências, notificações concorrentes/repetidas e rollback por falha de consulta, publicação e commit. O teste de imagem verificou persistência após reinício.

## Comandos principais

No Billing:

```powershell
dotnet build Billing.slnx -m:1 -p:UseSharedCompilation=false
dotnet test Billing.slnx --no-build --filter 'TestCategory!=ContainerImage' --settings coverage.runsettings --collect:'XPlat Code Coverage;Format=cobertura'
docker build -t fiap-billing:mercadopago .
$env:BILLING_TEST_IMAGE='fiap-billing:mercadopago'
dotnet test tests/FIAP.TechChallenge.Billing.Tests --no-build --filter 'TestCategory=ContainerImage'
npm test --prefix scripts/delivery
```

No Execution:

```powershell
$env:OS_TEST_IMAGE='fiap-os:local'
$env:EXECUTION_TEST_IMAGE='fiap-execution:local'
$env:BILLING_TEST_IMAGE='fiap-billing:mercadopago'
dotnet test Distributed.slnx
```

## Etapa externa pendente

Cadastrar os dois GitHub Secrets de teste no repositório principal, atualizar o bootstrap IAM, publicar as alterações dos repositórios e executar o fluxo central apply. Depois, configurar a URL pública do webhook no painel Mercado Pago e realizar a compra de teste. O [guia operacional](plataforma-aws.md#credenciais-do-checkout-mercado-pago) descreve essa sequência. Nenhum segredo Mercado Pago foi criado com valor fictício na AWS ou no GitHub.
