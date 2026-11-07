# ADR-024 — Plataforma efêmera AWS dos microsserviços

- **Status:** Aceita
- **Data:** 2026-11-07

## Contexto

OS, Execution e Billing já possuem aplicações, contratos, imagens e manifests próprios. Faltavam bancos, Kafka, segredos/permissões e um ciclo integrado de provisionamento/retirada. Após a conta recusar DocumentDB por restrição do Free Plan, o usuário escolheu MongoDB no EKS. RDS continua gerenciado; o laboratório é destruído diariamente, sem preservar dados.

## Decisão

Uma instância RDS privada atende bancos/usuários separados de OS e Billing. Execution usa MongoDB 8.0 em replica set rs0 de um membro, com autenticação, TLS/CA própria, EBS criptografado e gate real de transações antes do rollout. Kafka é um StatefulSet KRaft de um nó com EBS, acesso interno e NetworkPolicy. Não há alta disponibilidade nesses componentes de laboratório.

O repositório principal é a entrada única do ciclo completo. Infra mantém recursos compartilhados e os StatefulSets Kafka/MongoDB no estágio messaging; DB mantém RDS, segredos/certificados dos bancos e inicialização; aplicações mantêm suas migrations e workloads. Preservar bootstrap/state/roles das pipelines, remover os recursos operacionais e dados diariamente. Não recriar monólito/Lambda e não migrar dados históricos.

## Alternativas

DocumentDB gerenciado foi a escolha inicial, inviável no plano atual da conta. O usuário escolheu operar MongoDB no EKS em vez de habilitar o plano pago. Aurora PostgreSQL não substitui a API MongoDB existente. MSK acrescentaria custo/integração de autenticação; o broker interno reaproveita a configuração Kafka existente. Dois RDS isolariam capacidade/ciclo de vida, com custo maior.

## Consequências e evidências

Compartilhar a instância PostgreSQL compartilha capacidade e disponibilidade, sem compartilhar credenciais/bancos. MongoDB exige operação do replica set, TLS e armazenamento pela equipe. O gate mantém rollback, concorrência e deduplicação. StatefulSet/EBS não fornecem redundância; volume permanece na mesma AZ e é apagado no destroy. Credenciais e chaves privadas geradas também residem no state sensível.

A implementação inclui estágios Terraform, SecretStores por role, Jobs de inicialização/migrations, --verify-persistence e o orquestrador com smoke. Validação AWS ainda exige execução após publicação/bootstrap; não há evidência remota nesta decisão. Ver [guia operacional](../operacao/plataforma-aws.md) e [RFC-018](../RFCs/RFC-018-orquestracao-plataforma-aws.md).
