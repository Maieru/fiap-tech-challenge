# ADR-022 — Microsserviços no EKS com HTTP interno e credenciais isoladas

- **Status:** Aceita
- **Data:** 2026-11-02
- **Escopo:** OS, Execution, Billing, frontend e infraestrutura compartilhada

## Contexto

O frontend migrado já roteia os três prefixos de API, mas seus upstreams exigiam TLS interno ainda não habilitado nas APIs. Os serviços têm bancos independentes, consumidores Kafka e instrumentação OpenTelemetry. É necessário preparar implantação independente sem recriar o monólito.

## Decisão

Cada repositório mantém seus manifestos de aplicação; Infra mantém namespaces, SecretStores e observabilidade. OS usa `k8s/os`, Execution e Billing usam `k8s`. A pasta vazia `k8s/backend` do OS é retirada; o inventário em `legacy/aws/k8s/backend` é preservado.

A entrada pública mantém HTTPS no API Gateway. ALB/Nginx e os Services internos usam HTTP; os Services são ClusterIP em 8080 e não ganham exposição pública própria. Esta decisão substitui o contrato preparado de HTTPS interno em 8443. A rede privada não criptografa esses trechos; essa limitação é aceita para o laboratório. A migração futura para TLS exigirá certificados e confiança entre os componentes, sem desativar validação de certificados no Nginx.

Cada banco tem um usuário, senha e segredo próprios; ExternalSecrets importam apenas a connection string correspondente. Somente a chave JWT é compartilhada entre as APIs, com issuer/audience consistentes. A licença New Relic permanece nos componentes de observabilidade.

Cada API usa HPA de 1 a 3 réplicas por CPU a 70% do request, sem `spec.replicas` no Deployment. Atualizações usam RollingUpdate com maxUnavailable 0 e maxSurge 1. Migrations PostgreSQL são pré-requisito de deploy; não são executadas nas réplicas. MongoDB mantém a inicialização existente e exige suporte a transações.

As APIs exportam traces, métricas e logs via OTLP gRPC ao Collector central. O frontend usa OTLP/HTTP no mesmo Collector. O Collector exporta via HTTPS ao New Relic. Métricas de infraestrutura continuam na integração Kubernetes existente; o metrics-server fornece as métricas dos HPAs.

## Alternativas consideradas

- TLS na API ou sidecar: exige gestão de certificados internos; adiado para manter esta etapa concentrada nos manifestos.
- Um banco/usuário compartilhado: elimina o isolamento de credenciais e permissões pretendido.
- Réplicas fixas: não atende à escala solicitada.
- Migrations em cada inicialização: pode competir entre réplicas; substituído por preparação do schema antes do rollout.
- Collector por serviço: duplicaria infraestrutura e distribuição da licença.

## Consequências e evidências

A [ADR-023](ADR-023-entrega-servicos-ecr-eks.md), de 2026-11-07, acrescenta publicação ECR e deploy EKS por repositório, com roles OIDC próprias e renderização automática; as dependências e migrations continuam externas.

O mínimo de uma réplica não garante alta disponibilidade. Escala e surge dependem da capacidade dos nodes; o paralelismo Kafka depende das partições. CPU não mede backlog. Atualizações de schema precisam ser compatíveis com a coexistência temporária das versões no rollout; rollback de imagem não desfaz migrations.

Os manifestos usam placeholders para imagem/broker/JWT e exigem segredos, grants e IAM provisionados separadamente. Não alteram contratos HTTP ou de mensagens e não provisionam bancos ou Kafka.

Evidências: [OS](../../k8s/os/README.md), [roteiro integrado e validação local](../../k8s/README.md) e [RFC-016](../RFCs/RFC-016-manifestos-kubernetes-microsservicos.md). A implantação real e a ingestão no New Relic exigem validação no ambiente; nenhuma aplicação no EKS é executada nesta entrega.
