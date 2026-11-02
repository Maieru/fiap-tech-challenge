# ADR-009 — Observabilidade com OpenTelemetry e New Relic

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** observabilidade
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-11-02

## Contexto

A equipe precisa correlacionar chamadas, falhas e métricas sem manter vários backends de observabilidade no cluster. A configuração atual usa New Relic.

## Decisão

Instrumentar com OpenTelemetry e enviar telemetria pelo Collector:

```text
API (traces, métricas e logs) → Collector → New Relic
Navegador (traces e logs) → Nginx /otlp/* → Collector → New Relic
```

- O backend instrumenta ASP.NET Core, EF Core, HttpClient, runtime, processo e métricas de negócio.
- O frontend instrumenta carregamento, fetch, XHR e erros não tratados na área administrativa. A proteção dos links privados segue a [ADR-020](ADR-020-frontend-microsservicos-mesma-origem.md): páginas abertas pelo link não inicializam telemetria, e chamadas públicas são excluídas da instrumentação.
- O Collector exporta os três sinais por OTLP/HTTPS, com licença recebida por segredo.
- No EKS, o chart `nri-bundle` complementa a coleta da infraestrutura Kubernetes.
- Access logs do API Gateway permanecem no CloudWatch por 7 dias.
- A configuração local do Collector também exporta para New Relic; a stack integrada não exige sua execução. Jaeger, Loki, Prometheus e Grafana não compõem mais esse fluxo de exportação.

## Alternativas consideradas

- **Somente console:** não atenderia à correlação e consulta centralizadas.
- **Stack OSS autogerenciada:** exigiria manter coleta, armazenamento e visualização no cluster.
- **Instrumentação exclusiva do fornecedor:** aumentaria o acoplamento; OpenTelemetry preserva o protocolo de exportação.

## Consequências

- Reduz a operação de backends locais, com dependência da conta, conectividade, custo e retenção do New Relic.
- Destruir o cluster interrompe a coleta, mas não apaga automaticamente a telemetria já exportada.
- As configurações do Collector local e remoto são cópias e podem divergir.
- `/otlp/*` aceita ingestão sem autenticação específica.
- O evento de inicialização do frontend registra origem e caminho, sem query. Tokens em erros são filtrados; Nginx omite queries dos logs e Vite filtra tokens dos erros de proxy. Isso não comprova sanitização de todos os dados sensíveis ou privacidade na borda remota, que ainda exige validação após implantação.
- Readiness não depende da telemetria; envio por filas em memória não garante entrega durável.

Reconsiderar por custo, volume, retenção, disponibilidade ou requisitos de privacidade.

## Evidências e relações

- [Instrumentação do backend](https://github.com/Maieru/fiap-tech-challenge/blob/067aef498eb1fcf7104bf678915ad3d7058ba089/src/FIAP.TechChallenge.Fase1.Infrastructure/Observability/OpenTelemetryConfiguration.cs)
- [Instrumentação do frontend](../../src/FIAP.TechChallenge.Fase1.Frontend/src/observability/telemetry.ts)
- [Verificação local do frontend e dos logs](../operacao/validacao-frontend.md)
- [Configuração local do Collector](../../src/ObservabilityConfig/otel-collector-config.yaml)
- [Recursos de observabilidade no EKS](https://github.com/Maieru/fiap-tech-challenge-infra/tree/main/k8s/observability)
- [Configuração compartilhada](https://github.com/Maieru/fiap-tech-challenge-infra/tree/main/src/ObservabilityConfig)
- [Integração Kubernetes do New Relic](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/kubernetes-configs/newrelic.tf)
- [Métricas de negócio](../operacao/metricas-negocio.md)
