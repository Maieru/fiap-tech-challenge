# RFC-016 — Manifestos Kubernetes das APIs e integração OTLP

- **Status:** Aceita
- **Data:** 2026-11-02
- **Escopo:** configuração e operação no EKS

## Contexto e decisão

Materializar a [ADR-022](../ADRs/ADR-022-kubernetes-microsservicos.md) com YAMLs de Deployment, ClusterIP, ConfigMap, ExternalSecrets e HPA por API. Usar os nomes DNS esperados pelo frontend, agora em HTTP 8080. Namespace e SecretStore continuam sob o estado Terraform de infraestrutura.

ConfigMaps definem Production, broker parametrizado, issuer/audience e OTLP gRPC habilitado para o Collector em 4317. Segredos de banco são importados por propriedade explícita, sem extração em massa. JWT usa um segredo separado; nenhuma API recebe licença New Relic.

Manter requests 100m/128Mi, limits 500m/512Mi, armazenamento efêmero 64Mi/256Mi e usuário não root 1654 das imagens .NET. Startup/liveness usam o endpoint live, readiness usa ready; tolerância inicial 300 segundos, timeout de readiness 5 segundos e janela de encerramento 60 segundos.

HPA usa autoscaling/v2, mínimo 1, máximo 3, CPU 70% e estabilização de redução 300 segundos. O Deployment não fixa replicas. PostgreSQL usa ApplyMigrations=false; o operador prepara o schema antes do rollout e Execution inicializa seus índices conforme o código existente.

Reutilizar os pipelines de traces, métricas e logs do Collector; configurar o endpoint US/EU pela variável `new_relic_otlp_endpoint` do estágio kubernetes-configs. O frontend aponta OTLP/HTTP a 4318, usa CAs do sistema caso um upstream futuro seja HTTPS e não exige mais Secret de CA privada.

## Alternativas consideradas

Helm e overlays por ambiente acrescentariam uma camada sem necessidade nesta entrega única para EKS. Os YAMLs seguem o padrão existente e são preparados em cópia local com imagem imutável, broker e identidade JWT do ambiente. Jobs de migrations e pipelines permanecem fora desta etapa.

A etapa posterior [RFC-017](RFC-017-actions-ecr-eks.md), aceita em 2026-11-07, implementa as Actions ECR/EKS e a preparação automática das cópias YAML. Jobs de migrations continuam fora do escopo.

## Consequências e validação

A análise Sonar mantém os manifests no escopo. A regra `kubernetes:S5332` tem exceção somente em `k8s/frontend/config-maps.yaml` e `k8s/os/config-maps.yaml`, onde o HTTP interno das APIs/OTLP materializa a limitação aceita na ADR-022. A exceção não torna esses trechos criptografados e não se aplica a outras regras ou arquivos. Removê-la quando o TLS interno for implementado. As permissões OIDC da action central ficam nos jobs que executam operações AWS; o workflow não recebe escrita global.

O operador valida todos os placeholders, sincroniza segredos e confere métricas do HPA antes do aceite. Alterações em variáveis exigem rollout restart. YAML válido não comprova permissão IAM, grants no banco, alcance Kafka ou ingestão remota.

Validar manifests e referências, testes existentes do frontend e configuração Nginx. No ambiente, confirmar probes, JWT entre as três APIs, escala sob carga e traces/métricas/logs no New Relic. Ver o [roteiro integrado](../../k8s/README.md).
