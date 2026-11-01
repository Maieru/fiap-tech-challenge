# Evidências — integração OS/Execution

Execução local em 31/10/2026, .NET SDK 10.0.401, PowerShell 7 e Docker Linux. NUnit/Moq nas unidades; testes HTTP e Kafka com JWT, PostgreSQL, MongoDB replica set e broker reais, sem substituir os casos de uso.

| Suíte | Resultado | Cobertura de linhas |
| --- | --- | --- |
| Legado | 703 aprovados | 95,4% (3500/3668) |
| OS | 133 domínio + 57 aplicação + 43 integração aprovados | 90,7% (1602/1766) |
| Imagem OS | 1 aprovado | Não medida no processo do container |
| Execution | 97 domínio + 45 aplicação + 36 integração aprovados | 96,1% (1407/1463) |
| BDD entre imagens OS/Execution | 2 aprovados | Não medida nos processos dos containers |

Gate de 80% verificado separadamente com ReportGenerator 5.5.12. Coleta Cobertura/OpenCover; exclusão de testes/código gerado. O backfill manual permanece na métrica. Migrations atualizadas e teste de upgrade do esquema anterior preservam IDs, histórico e datas, sem duplicar OrdemCriada.

Casos adicionais cobrem ação pendente concorrente, flag/ambiente de autorização, auditoria, confirmação/recusa, rollback de histórico/Inbox, duplicidades, broker indisponível, DLQ/lacunas/replay e consulta após inativação de cliente/veículo. A separação de `FluxoOrdemRepository`/`PostgresOutbox` e a retirada dos controles de sequência/versão do domínio foram validadas pela mesma suíte.

Artefatos locais ignorados em `artifacts/execution-migration`: `os-simplified`, `coverage-acceptance`, `legacy-final`, `legacy-coverage-final`, `os-image-acceptance`, `os-openapi.json`. O Execution mantém seus próprios relatórios, BDD e [evidências](https://github.com/Maieru/fiap-tech-challenge-execution/blob/main/docs/verificacao.md).

Imagens construídas, Compose/overlay executados com rede e volumes descartáveis, readiness/liveness 200 nas duas APIs. Volumes anteriores e monólito preservados. `dotnet ef migrations has-pending-model-changes` não encontrou diferenças. actionlint validou o workflow novo. Resultados locais não comprovam análise Sonar/Actions remota; consulte o pipeline do commit correspondente.
