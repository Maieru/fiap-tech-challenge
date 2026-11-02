# Observabilidade com New Relic

Para importar o painel de aplicação e Kubernetes, siga o [guia de configuração do dashboard](dashboard-new-relic.md), com um template JSON sem identificadores da conta original.

As APIs .NET e o frontend podem exportar telemetria ao OpenTelemetry Collector. O Collector exporta traces, métricas e logs por OTLP/HTTP com TLS para o New Relic. O frontend usa `fiap-tech-challenge-frontend`; os serviços backend têm nomes próprios. O nome `fiap-tech-challenge-backend` nos painéis existentes se refere ao monólito retirado e não comprova instrumentação dos novos serviços.

## Ambiente local integrado

O Compose da raiz inicia frontend, OS, Execution e Billing sem exigir Collector ou licença New Relic. Para operação normal, execute `docker compose up -d --build` na raiz e acesse http://localhost:5173. O frontend tenta exportar telemetria administrativa por `/otlp`; sem Collector disponível, a exportação falha, sem bloquear as APIs.

Para habilitar exportação local, preencha `NEW_RELIC_LICENSE_KEY` no ambiente ou no `.env` da raiz (opção documentada em `.env.example`) e execute:

```powershell
docker compose -f compose.observability.yaml up -d
```

O Collector publica 4317/4318 no host. O frontend integrado usa `FRONTEND_OTLP_UPSTREAM` (padrão `http://host.docker.internal:4318`); OS/Execution usam `EXECUTION_ENABLE_OTLP=true` e `OTEL_EXPORTER_OTLP_ENDPOINT`, configurados no `.env` da raiz, para exportar via OTLP gRPC. Recrie os serviços afetados após alterar variáveis de ambiente. Não execute outro Collector nas mesmas portas. Esse roteiro não comprova ingestão remota; valide os sinais na conta configurada.

A licença é uma chave de ingestão (license key), não uma user API key. Para conta EU, use `NEW_RELIC_OTLP_ENDPOINT=https://otlp.eu01.nr-data.net`. A chave existe apenas no ambiente do Collector: nunca a coloque em variáveis `VITE_*` ou no navegador. O arquivo opcional exige a chave; a stack de negócio não. O Compose antigo foi removido, e arquivos `.env` locais anteriores não foram apagados.

## Privacidade do acompanhamento

O frontend registra origem e caminho, sem a query, no evento de inicialização. Páginas abertas em `/acompanhar-ordem/{id}` não inicializam telemetria; requisições públicas de acompanhamento são excluídas da instrumentação mesmo após navegação SPA. Tokens em mensagens de erro são filtrados. Nginx omite queries dos logs e desliga logs nas rotas de token; o Vite filtra tokens dos erros do proxy. A página usa `no-store`/`no-referrer`.

O [relatório local](validacao-frontend.md) registra a verificação dos logs OS/Nginx com dados sintéticos. API Gateway/ALB e demais camadas de nuvem ainda exigem validação após implantação; a sanitização do frontend não comprova a privacidade de toda a cadeia nem a correlação ponta a ponta.

## Kubernetes / AWS

A configuração remota de laboratório está no repositório `FiapTechChallengeInfra`:

1. Configure o GitHub Actions secret `NEW_RELIC_LICENSE_KEY` com a chave de ingestão no repositório que dispara o workflow (`FiapTechChallengeInfra` para execução direta; o orquestrador de criação legado foi retirado). Os workflows de aplicação e destruição repassam esse secret ao Terraform.
2. Aplique o estágio `infra/kubernetes-addons`. O Terraform cria o secret `fiap-newrelic-license` e sua versão com o JSON `{"license-key":"..."}` a partir da variável sensível e obrigatória `new_relic_license_key`. Para execução local, forneça `TF_VAR_new_relic_license_key` no ambiente antes de executar `terraform plan`/`apply` (também no `destroy`). Não é necessário cadastrar o valor manualmente no AWS Secrets Manager.
3. Se sua conta for EU, ajuste `NEW_RELIC_OTLP_ENDPOINT` em `k8s/observability/application/otel-collector-deployment.yaml` antes do deploy.
4. Aplique `infra/kubernetes-configs` e faça o deploy atualizado das aplicações. O ExternalSecret cria `newrelic-license` no namespace `fiap-observability`; o Collector lê a chave via `secretKeyRef`. Sem o valor no Secrets Manager, o Collector aguardará o Secret e não iniciará.
5. Verifique `kubectl -n fiap-observability get externalsecret newrelic-license` e `kubectl -n fiap-observability rollout status deployment/otel-collector`.

Os workflows do repositório de infraestrutura definem a aplicação desses estágios; o orquestrador de criação no repositório da aplicação foi retirado. O valor é gravado durante o estágio de add-ons, antes de configurar o Collector. A variável sensível oculta a chave na saída normal do Terraform, mas o valor fica armazenado no estado e no plano salvo (`tfplan`); restrinja o acesso ao backend e aos artifacts do workflow. O plano de `kubernetes-configs` remove os Deployments, Services e ConfigMaps antigos de Grafana, Prometheus, Loki e Jaeger. Se precisar do histórico ou de dashboards personalizados, exporte-os antes de aplicar: eles não são importados automaticamente pelo New Relic.

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

## Cobertura e validação

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

O exporter OTLP envia registros estruturados, mas isso não comprova que todos os logs do legado são JSON; seu serviço de e-mail ainda usa saída de console. A padronização e a prova de correlação ponta a ponta continuam pendentes. A migração do frontend implementou o tratamento de URLs privadas descrito acima; ingestão e privacidade na borda AWS ainda precisam ser verificadas.

O contador de falhas de e-mail não substitui um alerta de falha no processamento da ordem: erros de validação, persistência e execução precisam de cobertura própria. A ausência de eventos de falha não é evidência de funcionamento do alerta.
