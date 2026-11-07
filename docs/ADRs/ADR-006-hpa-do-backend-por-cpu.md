# ADR-006 — HPA do backend baseado em CPU

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** escalabilidade e disponibilidade
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

A API não mantém sessão local e o cluster possui Metrics Server. CPU oferece uma métrica inicial simples para ajustar réplicas.

## Decisão

Este registro descreve a política histórica do monólito. Para OS, Execution e Billing, a [ADR-022](ADR-022-kubernetes-microsservicos.md) define HPAs independentes de 1 a 3 réplicas, mantendo CPU alvo de 70%. Os arquivos abaixo permanecem somente como inventário legado.

Usar HPA `autoscaling/v2` somente no backend:

| Parâmetro | Valor |
| --- | --- |
| Réplicas | 1 a 10 |
| Alvo de CPU | 70% do request |
| Request por pod | CPU `100m`; memória `128Mi` |
| Limit por pod | CPU `500m`; memória `512Mi` |

O frontend mantém uma réplica. Esses valores são parâmetros iniciais do laboratório, sem capacidade ou SLO comprovados.

## Alternativas consideradas

- **Réplicas fixas:** não responderiam à variação de carga.
- **Memória:** não foi escolhida como sinal inicial de demanda.
- **Latência ou fila:** exigiriam métricas e componentes adicionais.
- **Escala vertical:** não ofereceria redundância entre pods.

## Consequências

- A escala reage à CPU usando recursos nativos do Kubernetes.
- O mínimo de uma réplica não oferece redundância; não há autoscaler de nós para garantir capacidade para dez pods.
- CPU não representa necessariamente espera por banco ou I/O; mais pods podem sobrecarregar o RDS.
- HPA não resolve concorrência nem idempotência das operações.
- Requests de CPU e ausência de sessão local são necessários para manter esta política. Ajustes devem ser apoiados por métricas e testes de carga.

Reconsiderar se CPU não acompanhar a demanda, os nós limitarem a escala ou houver requisito de alta disponibilidade.

## Evidências e relações

- [Manifest do HPA](../../legacy/aws/k8s/backend/hpa.yaml)
- [Recursos e probes do backend](../../legacy/aws/k8s/backend/deployment.yaml)
- [Teste de carga](https://github.com/Maieru/fiap-tech-challenge/blob/067aef498eb1fcf7104bf678915ad3d7058ba089/src/LoadTests/jornada-usuario.js)
- [Metrics Server](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/kubernetes-addons/metrics-server.tf)
- [RFC-004 — Containers no EKS](../RFCs/RFC-004-containers-no-amazon-eks.md)
