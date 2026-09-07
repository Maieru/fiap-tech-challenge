# ADR-009 — Observabilidade com OpenTelemetry e New Relic

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** observabilidade
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

A equipe precisa correlacionar chamadas, falhas e métricas sem manter vários backends de observabilidade no cluster. A configuração atual usa New Relic.

## Decisão

Instrumentar com OpenTelemetry e enviar telemetria pelo Collector:

```text
API (traces, métricas e logs) → Collector → New Relic
Navegador (traces e logs) → Nginx /otlp/* → Collector → New Relic
```

- O backend instrumenta ASP.NET Core, EF Core, HttpClient, runtime, processo e métricas de negócio.
- O frontend instrumenta carregamento, fetch, XHR e erros não tratados.
- O Collector exporta os três sinais por OTLP/HTTPS, com licença recebida por segredo.
- No EKS, o chart `nri-bundle` complementa a coleta da infraestrutura Kubernetes.
- Access logs do API Gateway permanecem no CloudWatch por 7 dias.
- A configuração local também exporta para New Relic; Jaeger, Loki, Prometheus e Grafana não compõem mais o fluxo atual.

## Alternativas consideradas

- **Somente console:** não atenderia à correlação e consulta centralizadas.
- **Stack OSS autogerenciada:** exigiria manter coleta, armazenamento e visualização no cluster.
- **Instrumentação exclusiva do fornecedor:** aumentaria o acoplamento; OpenTelemetry preserva o protocolo de exportação.

## Consequências

- Reduz a operação de backends locais, com dependência da conta, conectividade, custo e retenção do New Relic.
- Destruir o cluster interrompe a coleta, mas não apaga automaticamente a telemetria já exportada.
- As configurações do Collector local e remoto são cópias e podem divergir.
- `/otlp/*` aceita ingestão sem autenticação específica.
- O frontend registra `window.location.href` completo, que pode conter tokens. A sanitização de URLs e dados sensíveis continua pendente.
- Readiness não depende da telemetria; envio por filas em memória não garante entrega durável.

Reconsiderar por custo, volume, retenção, disponibilidade ou requisitos de privacidade.

## Evidências e relações

- [Instrumentação do backend](../../src/FIAP.TechChallenge.Fase1.Infrastructure/Observability/OpenTelemetryConfiguration.cs)
- [Instrumentação do frontend](../../src/FIAP.TechChallenge.Fase1.Frontend/src/observability/telemetry.ts)
- [Configuração local do Collector](../../src/ObservabilityConfig/otel-collector-config.yaml)
- [Recursos de observabilidade no EKS](https://github.com/Maieru/fiap-tech-challenge-infra/tree/main/k8s/observability)
- [Configuração compartilhada](https://github.com/Maieru/fiap-tech-challenge-infra/tree/main/src/ObservabilityConfig)
- [Integração Kubernetes do New Relic](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/kubernetes-configs/newrelic.tf)
- [Métricas de negócio](../operacao/metricas-negocio.md)
