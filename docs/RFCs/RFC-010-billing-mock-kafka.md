# RFC-010 — Billing, gateway mockado e contratos Kafka

- **Status:** Aceita
- **Data:** 2026-11-01

## Decisão técnica

.NET 10, EF Core/PostgreSQL exclusivo, contratos OS 0.2.0 e Billing 0.1.0. Envelope v1 e UUID da ordem como chave Kafka. Solicitação no tópico oficina.os.billing.requests.v1, respostas em oficina.billing.events.v1. Contratos permanecem DTOs sem dependências concretas. OS mantém assinaturas dos eventos Execution e acrescenta Billing; Execution preserva contratos atuais.

IPaymentGateway.ChargeAsync recebe cobrança, ordem, valor, moeda e chave estável por ordem. MockPaymentGateway aprova imediatamente com identificador determinístico. Recusa é resultado de negócio e falha técnica propaga exceção. Valor decimal é persistido como numeric sem arredondamento. Unicidade por ordem, lock transacional por ordem no Billing e FOR UPDATE no OS protegem concorrência. Deduplicação e alterações compartilham transação; publicação precede commit; offset posterior ao commit.

O diagnóstico grava total e MessageId original no OS. JSON da submáquina de pagamento persiste na linha da ordem, sob o mesmo Version/lock, sem armazenar itens do Execution. Compatibilidade aditiva no schema; nenhuma cobrança retroativa. Consultas e resposta 202 expõem Pagamento/Etapa.

## Alternativas e consequências

Sem acesso a bancos de outros serviços, comunicação HTTP interna, consumidor genérico, retry de aplicação ou DLQ. A publicação direta admite mensagens órfãs. Gateway real deverá conservar idempotência e definir tratamento de resultado incerto. Não há garantias de atomicidade distribuída.

## Verificação

Unidades, HTTP com JWT real, PostgreSQL/Kafka via Testcontainers e BDD de três imagens. Candidatos NuGet podem ser restaurados por feed local explícito; publicação de contratos precede consumidores no CI. Ambientes integrados compartilham apenas Kafka. Consulte o README Billing e relatório de validação.

## Validação dos campos monetários na entrada

Antes de desserializar e delegar, Billing exige `valor` no pedido e OS exige `valorTotal` no diagnóstico e `valor` nos resultados Billing. Os campos devem existir como número JSON representável por decimal; não há coerção de ausência, null ou string para zero. Zero explícito permanece permitido. Mensagens inválidas seguem a parada do host com offset pendente. A validação é local às fronteiras consumidoras; não altera contratos NuGet nem schema dos bancos.
