# RFC-004 — Execução de containers no Amazon EKS

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** plataforma de execução
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

API ASP.NET Core e SPA React precisam de imagens reproduzíveis, implantação declarativa, health checks e escala do backend. Kubernetes faz parte do escopo do projeto.

## Decisão

Executar API e frontend no Amazon EKS:

- Dockerfiles multi-stage geram imagens separadas no ECR.
- Deployments e services `ClusterIP` usam namespaces distintos.
- O backend declara startup, liveness e readiness probes, recursos e HPA.
- O frontend usa Nginx e uma réplica.
- Load Balancer Controller, Metrics Server e External Secrets integram a plataforma.
- Collector e integração Kubernetes do New Relic usam namespace de observabilidade.

O node group usa `t3.small`, com mínimo 1, desejado 3 e máximo 4 nós. O autorizador de ordens executa separadamente em Lambda.

## Alternativas consideradas

- **ECS/Fargate ou App Runner:** exigiriam substituir manifests e integrações Kubernetes adotadas.
- **EC2 com Compose:** transferiria rollout, recuperação e escala para automação própria.
- **Lambda para toda a aplicação:** não corresponde ao modelo de hospedagem escolhido para API e SPA.

## Consequências

- Kubernetes oferece rollout, probes e HPA, com custo de manter cluster, add-ons e versões.
- Não há autoscaler de nós, PodDisruptionBudget ou NetworkPolicy nos manifests atuais. Namespaces organizam cargas, mas não isolam tráfego.
- Os nós usam sub-redes públicas; o endpoint do control plane permite acesso público e privado.
- O deploy usa a role de infraestrutura com acesso administrativo ao cluster, embora exista uma role de aplicação limitada aos namespaces.
- Uma réplica mínima não garante alta disponibilidade; os parâmetros são do laboratório.

Reconsiderar se custo e operação superarem o benefício ou surgirem requisitos de escala a zero e disponibilidade não atendidos.

## Evidências e relações

- [Dockerfile do backend](../../src/FIAP.TechChallenge.Fase1.API/Dockerfile)
- [Dockerfile do frontend](../../src/FIAP.TechChallenge.Fase1.Frontend/Dockerfile)
- [Guia Kubernetes](../../k8s/README.md)
- [Configuração do EKS](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/aws-resources/eks.tf)
- [ADR-005 — Topologia de entrada](../ADRs/ADR-005-topologia-de-entrada-na-aws.md)
- [ADR-006 — HPA do backend](../ADRs/ADR-006-hpa-do-backend-por-cpu.md)
