# Configurar o dashboard FIAP - Backend no New Relic

Este guia instala o dashboard a partir do [JSON reutilizável](dashboards/fiap-backend.template.json). Ele reúne tráfego HTTP, latência, erros, ordens de serviço e recursos do Kubernetes em uma página com 12 widgets: dez gráficos e dois separadores.

## Preparar a telemetria

É necessário ter acesso à conta de destino no New Relic e permissão para criar dashboards e consultar seus dados. O ID numérico da conta é diferente da chave de ingestão e da user API key. Nenhuma chave deve ser colocada no JSON.

- **API e negócio:** configure o OpenTelemetry Collector conforme o [guia de observabilidade](observabilidade.md). O serviço esperado é `fiap-tech-challenge-backend`.
- **Kubernetes:** instale a integração `nri-bundle` conforme o [guia da infraestrutura](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/src/ObservabilityConfig/README.md#cpu-e-memória-do-kubernetes). O Collector da aplicação, sozinho, não fornece os eventos `K8sNodeSample` e `K8sContainerSample` usados aqui.
- **Atividade:** faça chamadas à API e percorra o fluxo de uma ordem. As métricas de duração só aparecem quando as etapas terminam. Aguarde os intervalos de coleta e exportação.

Com apenas o Docker Compose local, os gráficos da aplicação podem funcionar; os três gráficos de infraestrutura dependem da coleta de um cluster Kubernetes.

## Personalizar o JSON

O template usa `accountIds: [0]` nas dez consultas. **Substitua todos esses valores pelo ID numérico da conta de destino antes da importação.** O zero é apenas um marcador e não representa uma conta utilizável.

| Campo ou filtro | Valor do template | Como configurar |
| --- | --- | --- |
| `accountIds` | `[0]` | ID da conta que recebe os dados de cada consulta. |
| `service.name` nas consultas | `fiap-tech-challenge-backend` | Nome exportado pela API em `OTEL_SERVICE_NAME`. |
| `clusterName` nas consultas | `fiap-eks-cluster` | Nome real enviado pela integração Kubernetes. |
| `namespaceName` nas consultas de memória | `fiap-backend`, `fiap-frontend` | Namespaces que deseja acompanhar. |
| `name` do dashboard e da página | `FIAP - Backend` | Título desejado. |
| `permissions` | `PRIVATE` | Visibilidade inicial privada; ajuste na importação conforme o público desejado. |

Os nomes de serviço, cluster e namespaces são configurações do projeto, mantidas como exemplo. Os gráficos de memória incluem backend e frontend; o gráfico de CPU acompanha os nós inteiros do cluster.

Para gerar uma cópia pronta para importar, execute este PowerShell na raiz do repositório da aplicação. A saída fica na pasta temporária, fora dos arquivos versionados:

```powershell
$accountId = [long](Read-Host 'ID numérico da conta New Relic de destino')
if ($accountId -le 0) { throw 'Informe um ID de conta positivo.' }

$dashboard = Get-Content -LiteralPath docs/operacao/dashboards/fiap-backend.template.json -Raw | ConvertFrom-Json
foreach ($page in $dashboard.pages) {
    foreach ($widget in $page.widgets) {
        foreach ($query in $widget.rawConfiguration.nrqlQueries) {
            $query.accountIds = @($accountId)
        }
    }
}

$outputPath = Join-Path ([System.IO.Path]::GetTempPath()) 'fiap-backend.import.json'
$dashboard | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $outputPath -Encoding utf8
Write-Output $outputPath
```

Se os nomes do ambiente forem diferentes, edite os filtros na cópia gerada. Em ambientes com dados distribuídos entre contas, ajuste `accountIds` individualmente por consulta.

## Importar no New Relic

1. Abra o New Relic e acesse **All capabilities → Dashboards**.
2. Clique em **Import dashboard**.
3. Cole o conteúdo da cópia JSON com os IDs preenchidos.
4. Escolha a conta proprietária do dashboard e as permissões de acesso.
5. Clique em **Save** e abra o dashboard criado.

A conta proprietária escolhida na importação não pode ser alterada depois; as permissões podem. Confira também a conta de dados em cada consulta, definida por `accountIds`. O procedimento está na [documentação oficial de importação](https://docs.newrelic.com/docs/query-your-data/explore-query-data/dashboards/dashboards-charts-import-export-data/).

O template começa privado. A opção de edição por todos da conta corresponde a um compartilhamento interno mais amplo; um link público externo é uma configuração separada. Veja a [gestão de dashboards e compartilhamento](https://docs.newrelic.com/docs/query-your-data/explore-query-data/dashboards/manage-your-dashboard/).

## Entender os gráficos

| Widget | O que mostra | Como interpretar |
| --- | --- | --- |
| Requisições por minuto | Taxa da contagem de `http.server.request.duration`. | Volume de requisições HTTP instrumentadas por minuto. |
| Tempo Médio de Resposta | Média de `http.server.request.duration` multiplicada por 1.000. | Converte segundos em milissegundos; não representa p95 ou p99. |
| Erros HTTP 5xx (%) | Percentual das requisições com `http.response.status_code >= 500`. | Erros 4xx não entram no numerador. Sem tráfego, ausência de valor não significa 0% de erros. |
| Requisições Mais Lentas | Média e contagem agrupadas por método e rota, limitadas a dez grupos. | Compara médias por rota, não requisições individuais. |
| Ordens Criadas | Soma de `oficina.ordens.criadas` em série temporal. | Contagem registrada pela aplicação; não importa o histórico do banco. |
| Tempo Médio por Etapa (min) | Média de `oficina.ordens.etapa.duracao` dividida por 60 e agrupada por `etapa`. | Tempo decorrido de etapas concluídas, incluindo espera. |
| Evolução De Tempo Médio por Etapa (s) | Mesma duração em segundos, em intervalos de um minuto. | Mostra a evolução das médias por etapa. |
| CPU (%) | `allocatableCpuCoresUtilization` de `K8sNodeSample`, por nó. | Utilização relativa à CPU alocável do nó; inclui outras cargas do cluster. |
| Memória por contêiner (MiB) | `memoryWorkingSetBytes / 1048576`. | Working set por namespace, pod e contêiner nos dois namespaces filtrados. |
| Memória (%) | `memoryWorkingSetUtilization`, somente onde `memoryLimitBytes > 0`. | Utilização relativa ao limite configurado do contêiner. Contêineres sem limite ficam fora. |

As consultas NRQL completas estão no JSON, em `pages[].widgets[].rawConfiguration.nrqlQueries[].query`. Para alterar um gráfico, abra sua edição e ajuste a consulta, os filtros ou a visualização.

As consultas HTTP e de infraestrutura usam `SINCE 1 hour ago`; a média de etapas em minutos usa `SINCE 24 hours ago`. Ordens criadas não fixa `SINCE`. O seletor de tempo do dashboard pode substituir o intervalo da consulta: confira o período efetivo ao comparar gráficos. [Referência sobre intervalos de consulta](https://docs.newrelic.com/docs/nrql/using-nrql/query-time-range/).

As etapas de negócio são `diagnostico`, `execucao` e `finalizacao`. Finalização mede o tempo entre finalizar e entregar a ordem. O painel fornecido não inclui falhas de integração, logs, alertas ou monitor externo de disponibilidade; consulte as [métricas de negócio](metricas-negocio.md) para as consultas complementares.

## Validar e diagnosticar

No Query Builder, selecione a conta de destino e execute cada consulta abaixo separadamente:

```sql
FROM Metric SELECT uniques(metricName)
WHERE service.name = 'fiap-tech-challenge-backend'
SINCE 30 minutes ago
```

```sql
FROM K8sNodeSample, K8sContainerSample
SELECT count(*) FACET eventType(), clusterName
SINCE 30 minutes ago
```

```sql
FROM Metric SELECT count(http.server.request.duration)
WHERE service.name = 'fiap-tech-challenge-backend'
FACET http.response.status_code
SINCE 30 minutes ago
```

| Sintoma | Verificação |
| --- | --- |
| Todos os gráficos vazios | Confira os dez `accountIds`, permissões de consulta e intervalo selecionado. |
| HTTP sem dados | Confira nome do serviço, chegada de `http.server.request.duration` e logs do Collector. Gere tráfego na API. |
| Ordens ou etapas sem dados | Crie uma ordem e conclua as etapas. Confira os nomes `oficina.*` e aguarde a exportação. |
| Kubernetes sem dados | Confira a instalação de `nri-bundle`, a licença de ingestão e os valores reais de `clusterName` e `namespaceName`. |
| Memória percentual sem algumas séries | Confira se os contêineres possuem limite de memória e eventos no período. |
| Erros 5xx sem série ou resultado inesperado | Confira se o atributo de status HTTP está presente e se há requisições na janela. |

O dashboard estará validado quando os gráficos esperados para o ambiente receberem dados após uma jornada de teste e os filtros apontarem para a conta, serviço e cluster corretos. A criação do dashboard não instala a instrumentação nem configura alertas.

## O que foi removido da exportação

As dez referências ao ID da conta original foram substituídas por `0`. Não foram encontrados nomes de usuário, e-mails, chaves de API ou GUIDs de entidades preenchidos no arquivo fornecido. A permissão original de leitura e escrita compartilhadas foi substituída por `PRIVATE` para que o importador escolha o compartilhamento. O gráfico de memória em MiB recebeu um título; consultas e posições foram preservadas.

O JSON versionado é um template e precisa de personalização. A validação local cobre estrutura e remoção do identificador original; a importação e a ingestão devem ser verificadas na conta de destino.
