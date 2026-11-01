# Documentação técnica

Para desenvolver, comece pelo [mapa do repositório e suas soluções](../README.md#estrutura-do-repositório). Os guias estão separados por assunto e distinguem o legado do primeiro incremento da Fase 4:

| Área | Documentos |
| --- | --- |
| Componentes da Fase 4 | [Chassi em repositório próprio](https://github.com/Maieru/fiap-tech-challenge-chassis), [OS Service](../src/Services/OrdensServico/README.md), [arquitetura e reaproveitamento](arquitetura/fase4-incremento1.md) |
| Qualidade e dependências | [CI do monorepo, consumo NuGet e Sonar](operacao/monorepo-ci.md), [evidências e limites](operacao/evidencias-fase4-incremento1.md) |
| Arquitetura do legado | [Componentes e camadas](arquitetura/README.md), [sequências](arquitetura/sequencias.md), [banco de dados e ER](arquitetura/banco-de-dados.md) |
| Operação do legado/AWS | [CI/CD e deploy](operacao/ci-cd.md), [observabilidade](operacao/observabilidade.md), [métricas de negócio](operacao/metricas-negocio.md), [configuração do dashboard New Relic](operacao/dashboard-new-relic.md) |
| Decisões arquiteturais | [Índice de ADRs](ADRs/README.md) |
| Contratos Kafka — etapa 1 | [Contratos OS](../src/Services/OrdensServico/FIAP.TechChallenge.OS.Application.Messaging/README.md), [RFC-007](RFCs/RFC-007-contratos-mensageria-kafka.md), [padrão canônico](arquitetura/mensageria.md) |
| Execution — etapa 2 | [Arquitetura e limites](arquitetura/execution.md), [ADR-015](ADRs/ADR-015-execution-mongodb.md), [RFC-008](RFCs/RFC-008-execution-mongodb-kafka.md) |
| Escolhas técnicas | [Índice de RFCs](RFCs/README.md) |

## Organização

```text
docs/
├── README.md
├── arquitetura/
│   ├── README.md
│   ├── sequencias.md
│   ├── banco-de-dados.md
│   ├── mensageria.md
│   ├── execution.md
│   └── fase4-incremento1.md
├── operacao/
│   ├── ci-cd.md
│   ├── monorepo-ci.md
│   ├── evidencias-fase4-incremento1.md
│   ├── evidencias-fase4-incremento1.json
│   ├── observabilidade.md
│   ├── metricas-negocio.md
│   ├── dashboard-new-relic.md
│   └── dashboards/
│       └── fiap-backend.template.json
├── ADRs/
│   ├── README.md
│   ├── TEMPLATE.md
│   └── ADR-*.md
└── RFCs/
    ├── README.md
    ├── TEMPLATE.md
    └── RFC-*.md
```

## Como manter

- **Guias:** descrevem o funcionamento, a operação e as limitações atuais.
- **RFCs:** registram escolhas técnicas relevantes; **ADRs**, decisões arquiteturais duradouras.
- Os catálogos contêm somente decisões aceitas. Propostas ficam em issues ou pull requests.
- Use o template correspondente, inclua evidências e atualize o índice ao adicionar uma decisão.
- Correções preservam a data original e informam a última revisão. Mudanças de decisão ganham um novo registro vinculado ao anterior.
- Registros retrospectivos descrevem a implementação existente; suas alternativas não representam atas de discussões históricas.
