# Estado da migração para microsserviços

Atualizado em 2026-11-02.

OS e Execution estão implementados e integrados por Kafka, com bancos exclusivos e execução local independente ou conjunta. O legado continua necessário para o fluxo existente. Billing está implementado com gateway mockado e Saga no OS; a retirada do legado ainda não foi iniciada.

## O que já foi feito

| Componente | Entregue |
| --- | --- |
| OS | API própria para clientes, veículos, abertura e consulta de ordens, histórico, aprovação/cancelamento administrativos ou pelo link privado do cliente e entrega administrativa. Usa PostgreSQL exclusivo e projeta as confirmações do Execution. |
| Execution | Serviço em repositório próprio, com MongoDB exclusivo, catálogo de serviços e peças/insumos, estoque, diagnóstico, orçamento por snapshots, execução e devolução de estoque no cancelamento. |
| Billing | Serviço próprio PostgreSQL, consultas JWT, cobrança por Kafka e gateway mockado que aprova imediatamente. OS coordena pagamento antes da execução e cancelamento com devolução de estoque por recusa. |
| Integração | Comunicação Kafka nos dois sentidos. Os casos de uso publicam por `IMessagingService`; os `BackgroundService` recebem com switch explícito e chamam casos de uso da Application. Há deduplicação transacional e confirmação do offset após o commit local. |
| Pacotes compartilhados | Chassi técnico e adaptador Kafka em repositório próprio. Os contratos de cada produtor ficam em seu pacote `Application.Messaging`. Execution.Application.Messaging 0.1.0 está publicado; OS 0.2.0 e Billing 0.1.0 usam pacotes candidatos locais até publicação explícita. |
| Autenticação das APIs | OS mantém usuários, cadastro/login e emissão JWT; OS, Execution e Billing validam o token administrativo. Issuer e audience são configuráveis; a chave vem de `Jwt__SigningKey`. Cadastro permanece anônimo por decisão do projeto. |
| Execução e verificação | Soluções, imagens e testes próprios. Composes independentes e ambiente integrado com Kafka compartilhado. Testes HTTP com bancos e broker reais e cenários distribuídos com as três imagens verificam pagamento, execução, acompanhamento, entrega e compensação. |

O OS permanece no repositório principal, junto do legado e do frontend. Execution e chassi possuem repositórios separados. Não há compartilhamento de bancos nem referência ao código-fonte de outro serviço.

## O que permanece pendente

- **Gateway real:** implementar Mercado Pago no lugar do MockPaymentGateway. Cobrança por Kafka, confirmação antes da execução e Saga no OS com compensação por recusa já estão implementadas. Estorno, cancelamento após aprovação e novas tentativas ficam fora deste incremento.
- **Frontend:** adaptar as chamadas e as telas para os novos serviços. A interface atual continua consumindo a API legada.
- **Infraestrutura e tráfego:** preparar e implantar OS, Execution e Billing no ambiente de destino, incluindo bancos, Kafka, segredos, observabilidade e roteamento. A infraestrutura AWS existente ainda corresponde ao legado; o tráfego não foi migrado.
- **Retirada do legado:** após cobrir e validar o fluxo completo nos novos serviços, remover o backend legado e os artefatos de teste, build e deploy que só atendem a ele.
- **Dados:** os novos serviços podem começar com bancos novos de demonstração; não é necessária a importação do histórico legado. A limpeza dos dados e a retirada do banco legado ainda não foram executadas e ficam para a transição definitiva.

## Limite conhecido da mensageria

A publicação é direta, dentro do caso de uso, antes do commit local. Não há atomicidade entre banco e Kafka: uma entrega seguida de falha no commit pode exigir reconciliação manual. O padrão atual não usa Outbox, DLQ ou retry de aplicação. Consulte o [padrão de mensageria](mensageria.md) para o comportamento em falhas.

## Documentos relacionados

- [OS Service](../../src/Services/OrdensServico/README.md).
- [Execution e integração com OS](execution.md).
- [Arquitetura do legado e infraestrutura existente](README.md).
- [Histórico de validação do Execution](../operacao/validacao-execution.md). Os relatórios registram o estado de cada entrega; este documento consolida o estado atual.

Consulte [Billing e validação](../operacao/validacao-billing.md). Os novos contratos são candidatos locais até publicação explícita no NuGet.

Guia de [autenticação administrativa no OS](autenticacao-os.md): contas novas no PostgreSQL OS, emissão compatível e limites conhecidos.

[Acompanhamento e entrega no OS](acompanhamento-e-entrega.md) implementados; frontend continua sem migração.
