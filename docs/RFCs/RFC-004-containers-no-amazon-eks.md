# RFC-004 — Execução de containers no Amazon EKS

- **Status:** Aceita
- **Data:** 2026-08-23
- **Última revisão:** 2026-11-14
- **Escopo:** plataforma de execução dos microsserviços

## Contexto

APIs e frontend precisam de imagens reproduzíveis, implantação declarativa, health checks e escala independente. Kubernetes faz parte do escopo do projeto.

## Decisão

Executar OS, Execution, Billing e frontend no EKS. Dockerfiles geram imagens separadas; ECR mantém imagens por componente. Deployments e Services ClusterIP usam namespaces distintos. APIs declaram probes, recursos e HPAs independentes de 1 a 3 réplicas. Nginx serve o frontend e encaminha os prefixos das APIs.

Infra mantém Load Balancer Controller, Metrics Server, External Secrets e observabilidade. Kafka e MongoDB usam StatefulSets e volumes próprios. O perfil dos nós e a capacidade declarada estão no [guia da plataforma](../operacao/plataforma-aws.md).

## Alternativas consideradas

ECS/Fargate ou App Runner exigiriam outra integração. EC2 com Compose transferiria rollout e recuperação à automação própria. Funções isoladas não correspondem ao modelo adotado para estas APIs e SPA.

## Consequências

Kubernetes exige operação de cluster e add-ons. HPA depende de capacidade disponível e não garante redundância com mínimo de uma réplica. Namespaces organizam cargas; NetworkPolicies protegem dependências específicas e não constituem isolamento universal. O perfil de laboratório exige implantação e validação antes da demonstração.

## Evidências

- [Dockerfile OS](../../Dockerfile).
- [Kubernetes OS](../../k8s/os/README.md).
- [Manifestos dos microsserviços](RFC-016-manifestos-kubernetes-microsservicos.md).
- [Entrega ECR/EKS](RFC-017-actions-ecr-eks.md).
