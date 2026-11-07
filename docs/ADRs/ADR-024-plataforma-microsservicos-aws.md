# ADR-024 — Plataforma efêmera AWS dos microsserviços

- **Status:** Aceita
- **Data:** 2026-11-07

## Contexto

OS, Execution e Billing já possuem aplicações, contratos, imagens e manifests próprios. Faltavam bancos, Kafka, segredos/permissões e um ciclo integrado de provisionamento/retirada. A decisão do usuário é manter bancos gerenciados, executar Kafka no EKS e destruir o laboratório diariamente, sem preservar dados.

## Decisão

Uma instância RDS privada atende bancos/usuários separados de OS e Billing. Execution usa DocumentDB 5.0 com TLS e transações, com gate real de compatibilidade antes do rollout; desenvolvimento e testes existentes continuam usando MongoDB. Kafka é um StatefulSet KRaft de um nó com EBS, acesso interno e NetworkPolicy. Não há alta disponibilidade nesses componentes de laboratório.

O repositório principal é a entrada única do ciclo completo. Infra mantém recursos compartilhados e messaging; DB mantém bancos/segredos e inicialização; aplicações mantêm suas migrations e workloads. Preservar bootstrap/state/roles das pipelines, remover os recursos operacionais e dados diariamente. Não recriar monólito/Lambda e não migrar dados históricos.

## Alternativas

Tudo no EKS preservaria MongoDB e concentraria recursos, mas transferiria operação dos bancos à equipe; o usuário preferiu os bancos gerenciados. MSK acrescentaria custo/integração de autenticação; o broker interno reaproveita a configuração Kafka existente. Dois RDS isolariam capacidade/ciclo de vida, com custo maior.

## Consequências e evidências

Compartilhar a instância PostgreSQL compartilha capacidade e disponibilidade, sem compartilhar credenciais/bancos. DocumentDB tem diferenças reais em relação ao MongoDB; o gate bloqueia incompatibilidades, sem relaxar as invariantes de domínio. StatefulSet/EBS não fornecem redundância; volume permanece na mesma AZ. Credenciais geradas também residem no state sensível.

A implementação inclui estágios Terraform, SecretStores por role, Jobs de inicialização/migrations, --verify-persistence e o orquestrador com smoke. Validação AWS ainda exige execução após publicação/bootstrap; não há evidência remota nesta decisão. Ver [guia operacional](../operacao/plataforma-aws.md) e [RFC-018](../RFCs/RFC-018-orquestracao-plataforma-aws.md).
