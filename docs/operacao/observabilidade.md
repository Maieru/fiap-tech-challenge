# Observabilidade com New Relic

Backend (.NET) e frontend (browser via proxy Nginx `/otlp`) enviam telemetria ao OpenTelemetry Collector. O Collector exporta traces, métricas e logs por OTLP/HTTP com TLS para o New Relic. A instrumentação existente e os nomes `fiap-tech-challenge-backend` e `fiap-tech-challenge-frontend` são preservados; o frontend emite traces e logs, e o backend também emite métricas.

## Ambiente local

1. Na pasta `src`, copie `.env.example` para `.env`.
2. Preencha `NEW_RELIC_LICENSE_KEY` com uma chave de ingestão (license key) da sua conta. Não use uma user API key. O arquivo `.env` é ignorado pelo Git.
3. O endpoint padrão é US (`https://otlp.nr-data.net`). Para conta EU, use `https://otlp.eu01.nr-data.net` em `NEW_RELIC_OTLP_ENDPOINT`.
4. Execute `docker compose up -d --build --remove-orphans` na pasta `src`. A opção remove os contêineres da stack anterior deste projeto Compose, sem remover volumes do banco.
5. Abra o frontend em `http://localhost:5173` e faça chamadas à API. Aguarde ao menos um minuto para a exportação periódica de métricas.

A chave existe apenas no ambiente do Collector: nunca a coloque em variáveis `VITE_*` ou no navegador. O Compose exige uma chave não vazia para iniciar.

## Kubernetes / AWS

A configuração remota de laboratório está no repositório `FiapTechChallengeInfra`:

1. Configure o GitHub Actions secret `NEW_RELIC_LICENSE_KEY` com a chave de ingestão no repositório que dispara o workflow (`FiapTechChallengeFase1` para o orquestrador; `FiapTechChallengeInfra` para execução direta). Os workflows de aplicação e destruição repassam esse secret ao Terraform.
2. Aplique o estágio `infra/kubernetes-addons`. O Terraform cria o secret `fiap-newrelic-license` e sua versão com o JSON `{"license-key":"..."}` a partir da variável sensível e obrigatória `new_relic_license_key`. Para execução local, forneça `TF_VAR_new_relic_license_key` no ambiente antes de executar `terraform plan`/`apply` (também no `destroy`). Não é necessário cadastrar o valor manualmente no AWS Secrets Manager.
3. Se sua conta for EU, ajuste `NEW_RELIC_OTLP_ENDPOINT` em `k8s/observability/application/otel-collector-deployment.yaml` antes do deploy.
4. Aplique `infra/kubernetes-configs` e faça o deploy atualizado das aplicações. O ExternalSecret cria `newrelic-license` no namespace `fiap-observability`; o Collector lê a chave via `secretKeyRef`. Sem o valor no Secrets Manager, o Collector aguardará o Secret e não iniciará.
5. Verifique `kubectl -n fiap-observability get externalsecret newrelic-license` e `kubectl -n fiap-observability rollout status deployment/otel-collector`.

O workflow existente já aplica os estágios na ordem correta. O valor é gravado durante o estágio de add-ons, antes de configurar o Collector. A variável sensível oculta a chave na saída normal do Terraform, mas o valor fica armazenado no estado e no plano salvo (`tfplan`); restrinja o acesso ao backend e aos artifacts do workflow. O plano de `kubernetes-configs` remove os Deployments, Services e ConfigMaps antigos de Grafana, Prometheus, Loki e Jaeger. Se precisar do histórico ou de dashboards personalizados, exporte-os antes de aplicar: eles não são importados automaticamente pelo New Relic.

Para rotacionar a chave, atualize `NEW_RELIC_LICENSE_KEY` e reaplique `kubernetes-addons`; o Terraform cria uma nova versão do secret. Depois, aguarde a sincronização do ExternalSecret (até 1h) e execute `kubectl -n fiap-observability rollout restart deployment/otel-collector`, pois variáveis de ambiente não são atualizadas em pods existentes. O Terraform atual usa recovery_window_in_days = 0 para esse segredo, sem janela de recuperação configurada.

## Verificação no New Relic

Procure os serviços pelo nome na interface de entidades/APM e consulte no Query Builder:

```sql
FROM Span SELECT count(*) FACET service.name SINCE 30 minutes ago
FROM Log SELECT count(*) FACET service.name SINCE 30 minutes ago
FROM Metric SELECT uniques(metricName) WHERE service.name = 'fiap-tech-challenge-backend' SINCE 30 minutes ago
```

Para diagnóstico, use `docker compose logs otel-collector` ou `kubectl -n fiap-observability logs deployment/otel-collector`. Erros 401/403 indicam problema de chave/conta; confira também região e saída HTTPS na porta 443. O Collector usa lotes de até 256 registros, gzip, limite de memória e retentativas. A fila é em memória: reinícios podem perder dados pendentes. Se receber 413, reduza o lote ou o tamanho dos registros (o limite de ingestão é por bytes).

A API passa a exportar métricas exclusivamente por OTLP; o endpoint `/metrics` e o exporter Prometheus foram removidos. O Collector envia telemetria da aplicação. A coleta de CPU/memória e estado do Kubernetes é declarada separadamente pelo chart nri-bundle no estágio kubernetes-configs. Dashboards Grafana e recursos exclusivos do agente New Relic Browser não são migrados automaticamente.

Referência: [configuração oficial OTLP do New Relic](https://docs.newrelic.com/docs/opentelemetry/best-practices/opentelemetry-otlp/).

## Cobertura e validação da Fase 3

A configuração versionada não comprova ingestão nem alertas ativos. Validar os itens abaixo após deploy:

| Requisito | Evidência a produzir |
| --- | --- |
| Latência da API | Requisições de teste visíveis nos traces e métricas HTTP, com unidade, janela e percentis identificados no painel. |
| CPU e memória Kubernetes | Dados de nós/pods/containers do cluster correto; seguir as [consultas e validação da integração](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/src/ObservabilityConfig/README.md#cpu-e-memória-do-kubernetes). |
| Healthchecks | Liveness responde com processo vivo; readiness fica indisponível quando o banco não pode ser acessado. Validar falhas somente em ambiente de teste. |
| Uptime | Monitor externo consulta a entrada pública e apresenta disponibilidade no período. Probes internas não substituem esse monitor, ainda não declarado. |
| Logs JSON e correlação | Inspecionar registros estruturados, identificadores de trace/span e correlação de uma requisição do navegador até a API e o banco, sem incluir CPF, token ou senha. |
| Alertas de ordens | Definir condição, janela, limiar e destinatário; provocar uma falha controlada no processamento e comprovar notificação e recuperação. Não há alerta declarado nos arquivos revisados. |
| Dashboards de negócio | Executar a jornada e conferir volume diário, duração por etapa e falhas de integração, conforme o [guia de métricas](metricas-negocio.md). |

O exporter OTLP envia registros estruturados, mas isso não comprova que todos os logs da aplicação são JSON. Não há configuração explícita de formatter JSON de console; o serviço de e-mail ainda usa saída de console. A padronização e a prova de correlação ponta a ponta continuam pendentes. O frontend também registra URL completa, que precisa ser sanitizada para não exportar tokens.

O contador de falhas de e-mail não substitui um alerta de falha no processamento da ordem: erros de validação, persistência e execução precisam de cobertura própria. A ausência de eventos de falha não é evidência de funcionamento do alerta.
