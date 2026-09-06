# Métricas de negócio no New Relic

O Domain define `MetricasNegocio` em `Observability`, usando System.Diagnostics.Metrics e System.Transactions. A Application registra as medições nos casos de uso. Essa escolha mantém a estrutura simples e dispensa a referência da Infrastructure à Application, mas faz o Domain conhecer essas APIs do .NET. Infrastructure registra o Meter `Fiap.TechChallenge.Negocio` no OpenTelemetry e exporta via OTLP pelo Collector existente. Compose e Kubernetes já configuram temporalidade delta. Não é necessário instalar o agente New Relic ou expor /metrics.

| Métrica | Instrumento | Atributos |
| --- | --- | --- |
| oficina.ordens.criadas | Counter | nenhum |
| oficina.ordens.etapa.duracao | Histogram, segundos | etapa: diagnostico, execucao, finalizacao |
| oficina.integracoes.falhas | Counter | integracao: email |

A criação é registrada no caso de uso compartilhado pelos três fluxos, após salvar. Quando existe TransactionScope, a emissão espera a confirmação da transação; rollback não incrementa o contador. As durações também são registradas depois da persistência e respeitam transações ambientes.

- Diagnóstico: DataEnvioAprovacao - DataInicioDiagnostico.
- Execução: DataFinalizacao - DataInicioExecucao.
- Finalização: DataEntrega - DataFinalizacao (tempo aguardando retirada).

RegistrarEtapaConcluida recebe o enum `StatusOrdemServico` (EmDiagnostico, EmExecucao, Finalizada), indicando o status cujo período terminou. O mapeamento explícito mantém os atributos exportados em minúsculas, preservando as consultas existentes.

As médias incluem apenas etapas concluídas no período e representam tempo decorrido, incluindo espera. Não medem a duração HTTP. IDs de ordens, dados de clientes e mensagens de exceção não são atributos das métricas.

MeteredMailService envolve IMailService e conta uma falha quando o resultado indica erro/false ou quando ocorre uma exceção, preservando o retorno ou a exceção. OperationCanceledException não é contada. Não existem retries no serviço atual; se forem adicionados, devem ficar dentro do serviço envolvido para contar apenas a falha final. O MailService atual apenas escreve no console e retorna sucesso: não envia e-mail real. Outras integrações devem chamar RegistrarFalhaIntegracao com um nome fixo quando forem implementadas.

## Dashboard

No Query Builder, execute as consultas e adicione os gráficos ao mesmo dashboard. Ajuste o filtro de serviço se o nome configurado for diferente. O seletor de período do dashboard pode substituir SINCE/UNTIL; preserve os limites de dias completos no gráfico diário.

### Ordens criadas por dia (sete dias completos)

```sql
FROM Metric
SELECT sum(oficina.ordens.criadas) AS 'Ordens criadas'
WHERE service.name = 'fiap-tech-challenge-backend'
SINCE 7 days ago UNTIL today
TIMESERIES 1 day
WITH TIMEZONE 'America/Sao_Paulo'
```

### Tempo médio por etapa (minutos)

```sql
FROM Metric
SELECT average(oficina.ordens.etapa.duracao) / 60 AS 'Tempo médio em minutos'
WHERE service.name = 'fiap-tech-challenge-backend'
FACET etapa
SINCE 30 days ago
```

### Falhas por integração

```sql
FROM Metric
SELECT sum(oficina.integracoes.falhas) AS 'Falhas'
WHERE service.name = 'fiap-tech-challenge-backend'
FACET integracao
SINCE 7 days ago
```

## Validação após deploy

Crie uma ordem e percorra diagnóstico, aprovação, finalização e entrega. Aguarde pelo menos um intervalo de exportação (normalmente um minuto) e consulte:

```sql
FROM Metric SELECT uniques(metricName)
WHERE service.name = 'fiap-tech-challenge-backend'
AND metricName LIKE 'oficina.%'
SINCE 30 minutes ago
```

A métrica de falhas só aparecerá após uma falha observada; ausência de série não comprova sucesso. Para testar o caminho de falha sem um provedor real, os testes de MeteredMailService simulam resultados malsucedidos e exceções usando MeterListener.

As métricas começam na instrumentação, sem importar histórico do banco. São telemetria operacional: quedas antes da exportação podem perder medições, e exportações próximas da meia-noite podem deslocar a atribuição diária. O banco continua sendo a fonte para contabilidade exata.

Referências: [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/dotnet/metrics/getting-started-console/) e [métricas OTLP no New Relic](https://docs.newrelic.com/docs/opentelemetry/best-practices/opentelemetry-best-practices-metrics/).
