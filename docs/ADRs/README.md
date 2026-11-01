# Architecture Decision Records (ADRs)

Os ADRs preservam decisões arquiteturais vigentes e suas consequências. Use o [template](TEMPLATE.md) para novos registros.

## Catálogo

| ADR | Status | Decisão |
| --- | --- | --- |
| [ADR-001](ADR-001-monolito-em-camadas.md) | Aceita | Manter um monólito em camadas com domínio no centro. |
| [ADR-002](ADR-002-comunicacao-rest-json.md) | Aceita | Usar HTTP/JSON síncrono com REST pragmático e mapear `Result<T>` para HTTP. |
| [ADR-003](ADR-003-persistencia-por-repositorios.md) | Aceita | Isolar EF Core por contratos, repositórios e mapeamento explícito. |
| [ADR-004](ADR-004-preservacao-do-historico.md) | Aceita | Preservar histórico com snapshots de itens e exclusão lógica. |
| [ADR-005](ADR-005-topologia-de-entrada-na-aws.md) | Aceita | Publicar API Gateway diante de VPC Link e ALB interno. |
| [ADR-006](ADR-006-hpa-do-backend-por-cpu.md) | Aceita | Escalar o backend com HPA entre 1 e 10 réplicas por CPU. |
| [ADR-007](ADR-007-gestao-centralizada-de-segredos.md) | Aceita | Sincronizar AWS Secrets Manager com Kubernetes por External Secrets. |
| [ADR-008](ADR-008-estados-terraform-e-repositorios.md) | Aceita | Separar responsabilidades em repositórios e estados Terraform dependentes. |
| [ADR-009](ADR-009-observabilidade-com-opentelemetry.md) | Aceita | Exportar telemetria com OpenTelemetry Collector para New Relic. |
| [ADR-010](ADR-010-ambiente-efemero-orientado-a-custo.md) | Aceita | Tratar a infraestrutura atual como ambiente descartável de demonstração. |
| [ADR-011](ADR-011-extracao-incremental-os-monorepo.md) | Aceita | Extrair o OS por responsabilidade em monorepo temporário, preservando o legado. |
| [ADR-012](ADR-012-dados-historico-os.md) | Aceita | PostgreSQL exclusivo, soft delete e histórico independente de cadastros ativos. |
| [ADR-013](ADR-013-chassi-em-repositorio-proprio.md) | Aceita | Manter chassi técnico e publicação NuGet em repositório independente. |
| [ADR-014](ADR-014-contratos-mensageria.md) | Aceita | Compartilhar interface técnica, separar adaptador Kafka e distribuir contratos por produtor. |
| [ADR-015](ADR-015-execution-mongodb.md) | Aceita | Persistir Execution em MongoDB próprio, com transações locais, snapshots e controle de concorrência. |
| [ADR-016](ADR-016-integracao-os-execution.md) | Aceita | Confirmar decisões administrativas por eventos Execution e manter projeção transacional no OS. |

Os ADRs iniciais formalizam decisões predominantes comprovadas pela implementação. Desvios conhecidos são explicitados no próprio registro como limitações ou dívidas de conformidade, sem serem apresentados como capacidades já atendidas.

A data original identifica o registro; a última revisão indica a atualização do texto e das evidências. As alternativas são avaliações retrospectivas, não atas de discussões históricas.
