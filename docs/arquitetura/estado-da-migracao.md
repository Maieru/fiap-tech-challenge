# Estado da migração para microsserviços

Atualizado em 2026-11-01.

OS e Execution estão implementados e integrados por Kafka, com bancos exclusivos e execução local independente ou conjunta. O legado continua necessário para o fluxo existente. Billing e a retirada do legado ainda não foram iniciados.

## O que já foi feito

| Componente | Entregue |
| --- | --- |
| OS | API própria para clientes, veículos, abertura e consulta de ordens, histórico, aprovação e cancelamento administrativos. Usa PostgreSQL exclusivo e projeta as confirmações do Execution. |
| Execution | Serviço em repositório próprio, com MongoDB exclusivo, catálogo de serviços e peças/insumos, estoque, diagnóstico, orçamento por snapshots, execução e devolução de estoque no cancelamento. |
| Integração | Comunicação Kafka nos dois sentidos. Os casos de uso publicam por `IMessagingService`; os `BackgroundService` recebem com switch explícito e chamam casos de uso da Application. Há deduplicação transacional e confirmação do offset após o commit local. |
| Pacotes compartilhados | Chassi técnico e adaptador Kafka em repositório próprio. Os contratos de cada produtor ficam em seu pacote `Application.Messaging`. Ambos os pacotes de contratos estão publicados no NuGet na versão 0.1.0. |
| Autenticação das APIs | OS e Execution validam JWT administrativo. Issuer e audience são configuráveis; a chave vem de `Jwt__SigningKey`. A emissão do token continua no legado. |
| Execução e verificação | Soluções, imagens e testes próprios. Composes independentes e ambiente integrado com Kafka compartilhado. Testes HTTP com bancos e broker reais e cenários distribuídos com as duas imagens verificam o fluxo OS/Execution. |

O OS permanece no repositório principal, junto do legado e do frontend. Execution e chassi possuem repositórios separados. Não há compartilhamento de bancos nem referência ao código-fonte de outro serviço.

## O que permanece pendente

- **Billing e pagamentos:** implementar o serviço de cobrança e a integração com Mercado Pago, definir seus contratos e integrar o pagamento ao ciclo da ordem. A coordenação entre aprovação, pagamento e execução, incluindo Saga e compensações, ainda precisa ser definida e implementada.
- **Funcionalidades ainda no legado:** dar destino ao cadastro/login de usuários e à emissão de JWT, migrar o acompanhamento e a aprovação pelo cliente e completar a entrega da ordem no novo fluxo.
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
