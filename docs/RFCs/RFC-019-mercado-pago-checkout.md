# RFC-019 — Checkout hospedado Mercado Pago

- Status: Aceita
- Data: 2026-11-08

## Contexto

O Billing aprovava imediatamente através de um mock. As mensagens de solicitação contêm ordem, valor e moeda, sem dados de cartão. A integração deve oferecer pagamento externo com alterações mínimas e preservar os contratos da Saga.

## Alternativas consideradas

Checkout próprio exigiria coleta de dados do pagador e tokenização no frontend. Manter o mock não comprovaria a integração externa. O checkout hospedado permite coletar os dados no Mercado Pago e reutilizar o detalhe administrativo existente.

## Decisão

Usar Checkout Pro pela Orders API com HttpClient. Criar a order com o UUID da ordem de serviço como referência e chave de idempotência; guardar seu identificador e checkoutUrl na cobrança pendente. Confirmar valor zero localmente com `sem-cobranca-<UUID>`. Preservar o decimal sem arredondamento: rejeições de formato pelo provedor são falhas técnicas, não pagamentos recusados.

O webhook público valida HMAC-SHA256 com o segredo de assinatura, consulta a order no provedor e verifica referência, identificador, moeda e valor. `processed/accredited` confirma; `failed` e `canceled` recusam; outros estados não concluem. O bloqueio por ordem protege a transição. Cobranças concluídas não mudam com notificações repetidas ou atrasadas. Os eventos existentes usam o ID da cobrança como MessageId estável e preservam a correlação e causação da solicitação original. GatewayPaymentId identifica a order externa neste fluxo.

Guardar Access Token e segredo de assinatura nos GitHub Actions Secrets do repositório principal. O workflow central os encaminha somente ao estágio aws-resources; Terraform cria o contêiner do segredo, e AWS CLI grava seu JSON por arquivo temporário removido ao final. Nenhuma credencial Mercado Pago passa por variáveis ou estado Terraform. External Secrets disponibiliza os valores exclusivamente ao namespace Billing.

O link é exibido apenas no detalhe administrativo, abre em nova aba e aproveita a atualização periódica existente. Não há SDK de cartão, nova autenticação de cliente, estorno, recriação de checkout ou reconciliação automática.

## Consequências e limites

A confirmação deixa de ser imediata. Credenciais ausentes interrompem o apply antes do provisionamento; destroy não depende delas. A primeira validação usa credenciais e comprador de teste. O endpoint público do webhook deve ser cadastrado no painel da aplicação Mercado Pago depois do deploy. A API externa é a mesma para teste e produção; as credenciais selecionam o ambiente.

A migration adiciona somente campos anuláveis; cobranças antigas são preservadas. Banco e Kafka continuam sem transação distribuída: publicação antes do commit pode resultar em reentrega do mesmo evento. O MessageId estável permite deduplicação pelos consumidores. Erros de consulta/publicação/commit geram 503 para reentrega do webhook, sem gravar uma recusa técnica.

## Evidências e validação

Implementação em Billing: gateway HTTP, validação de assinatura, caso de uso de atualização, endpoint webhook e migration MercadoPagoCheckout. Infraestrutura: segredo sem versão Terraform, script de sincronização e IAM restrito por namespace. Frontend: link na tela administrativa existente.

Testes cobrem payload/idempotência, estados do provedor, assinatura, valor zero, pendência, duplicatas, divergências e falhas técnicas. Testes PostgreSQL/Kafka verificam concorrência e rollback. BDD utiliza um servidor HTTP descartável e webhook assinado; não realiza pagamentos externos. Resultados efetivamente executados devem ser consultados no CI/relatório de entrega, sem presumir validação AWS ou compra de teste.

## Referências

- [Criar order e obter checkout_url](https://www.mercadopago.com.br/developers/pt/docs/checkout-pro-orders/create-order?scope=prod)
- [Notificações](https://www.mercadopago.com.br/developers/en/docs/checkout-pro-orders/notifications?scope=prod)
- [Estados da order](https://www.mercadopago.com.br/developers/en/docs/checkout-pro-orders/payment-management/status/order-status?scope=prod)
