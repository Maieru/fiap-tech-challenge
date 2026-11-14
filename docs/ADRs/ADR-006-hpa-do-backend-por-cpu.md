# ADR-006 — HPA das APIs baseado em CPU

- **Status:** Aceita
- **Data:** 2026-08-23
- **Última revisão:** 2026-11-14
- **Escopo:** escalabilidade e disponibilidade

## Contexto

As APIs não mantêm sessão local e o cluster dispõe de Metrics Server. CPU oferece um sinal inicial para ajustar as réplicas de cada serviço.

## Decisão

Usar HPA `autoscaling/v2` independente para OS, Execution e Billing, com 1 a 3 réplicas e CPU alvo de 70% do request. Manter requests e limits nos Deployments. O frontend mantém uma réplica.

## Alternativas consideradas

Réplicas fixas não respondem à demanda. Memória, latência e fila exigiriam critérios e métricas adicionais. Escala vertical não oferece redundância entre pods.

## Consequências

Uma réplica mínima não oferece redundância. A capacidade dos nós limita a escala, e CPU não representa necessariamente espera por banco ou I/O. Mais pods podem aumentar a pressão sobre bancos e Kafka. Os parâmetros do laboratório não constituem SLO ou capacidade comprovada.

## Evidências e relações

- [HPA OS](../../k8s/os/hpa.yaml).
- [Deployment OS](../../k8s/os/deployment.yaml).
- [Microsserviços no EKS](ADR-022-kubernetes-microsservicos.md).
