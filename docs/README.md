# Documentação técnica

Comece pela [visão da arquitetura](arquitetura/README.md). Os guias estão separados por assunto:

| Área | Documentos |
| --- | --- |
| Arquitetura | [Componentes e camadas](arquitetura/README.md), [sequências](arquitetura/sequencias.md), [banco de dados e ER](arquitetura/banco-de-dados.md) |
| Operação | [CI/CD](operacao/ci-cd.md), [observabilidade](operacao/observabilidade.md), [métricas de negócio](operacao/metricas-negocio.md), [configuração do dashboard New Relic](operacao/dashboard-new-relic.md) |
| Decisões arquiteturais | [Índice de ADRs](ADRs/README.md) |
| Escolhas técnicas | [Índice de RFCs](RFCs/README.md) |

## Organização

```text
docs/
├── README.md
├── arquitetura/
│   ├── README.md
│   ├── sequencias.md
│   └── banco-de-dados.md
├── operacao/
│   ├── ci-cd.md
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
