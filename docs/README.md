# Documentação técnica

Consulte o [estado da aplicação](arquitetura/estado-da-migracao.md) para acompanhar entregas e pendências. Para desenvolver, comece pelo [mapa do repositório e suas soluções](../README.md#estrutura-do-repositório). Os guias descrevem os serviços, suas integrações e a operação:

| Área | Documentos |
| --- | --- |
| Serviços e estado atual | [Estado atual](arquitetura/estado-da-migracao.md), [frontend migrado](arquitetura/migracao-frontend.md), [OS Service](operacao/os-service.md), [Execution](arquitetura/execution.md), [Billing e Saga](arquitetura/saga-pagamento.md), [chassi em repositório próprio](https://github.com/Maieru/fiap-tech-challenge-chassis) |
| Frontend local e nuvem | [Configuração e comandos](../src/FIAP.TechChallenge.Fase1.Frontend/README.md), [validação local](operacao/validacao-frontend.md), [pré-requisitos Kubernetes](../k8s/README.md), [ADR-020](ADRs/ADR-020-frontend-microsservicos-mesma-origem.md), [RFC-013](RFCs/RFC-013-proxy-frontend-microsservicos.md) |
| Qualidade e dependências | [CI do monorepo, consumo NuGet e Sonar](operacao/monorepo-ci.md), [histórico de validação do Execution](operacao/validacao-execution.md), [revisão Billing/Saga](operacao/revisao-billing-saga.md), [validação backend ponta a ponta](operacao/validacao-ponta-a-ponta.md), [validação do frontend](operacao/validacao-frontend.md) |
| Arquitetura dos serviços | [Componentes, dados e camadas](arquitetura/README.md), [sequências](arquitetura/sequencias.md), [banco de dados e ER](arquitetura/banco-de-dados.md) |
| Operação e AWS | [CI/CD e deploy](operacao/ci-cd.md), [observabilidade](operacao/observabilidade.md), [métricas de negócio](operacao/metricas-negocio.md), [configuração do dashboard New Relic](operacao/dashboard-new-relic.md) |
| Decisões arquiteturais | [Índice de ADRs](ADRs/README.md) |
| Mensageria e integração | [Padrão canônico](arquitetura/mensageria.md), [contratos OS](../src/FIAP.TechChallenge.OS.Application.Messaging/README.md), [integração OS/Execution](RFCs/RFC-009-integracao-os-execution.md), [Saga de pagamento](arquitetura/saga-pagamento.md), [autenticação OS](arquitetura/autenticacao-os.md), [acompanhamento e entrega](arquitetura/acompanhamento-e-entrega.md) |
| Organização dos projetos OS | [Guia de desenvolvimento](operacao/os-service.md), [projetos diretamente em src](RFCs/RFC-015-projetos-os-diretamente-src.md) |
| Pagamento externo | [Checkout Mercado Pago](RFCs/RFC-019-mercado-pago-checkout.md), [validação e limites](operacao/validacao-mercado-pago.md), [Saga](arquitetura/saga-pagamento.md) |
| Escolhas técnicas | [Índice de RFCs](RFCs/README.md) |

## Organização

```text
docs/
├── README.md
├── arquitetura/
│   ├── README.md
│   ├── estado-da-migracao.md
│   ├── migracao-frontend.md
│   ├── autenticacao-os.md
│   ├── acompanhamento-e-entrega.md
│   ├── sequencias.md
│   ├── banco-de-dados.md
│   ├── mensageria.md
│   ├── saga-pagamento.md
│   └── execution.md
├── operacao/
│   ├── ci-cd.md
│   ├── monorepo-ci.md
│   ├── os-service.md
│   ├── validacao-ponta-a-ponta.md
│   ├── validacao-frontend.md
│   ├── revisao-billing-saga.md
│   ├── validacao-billing.md
│   ├── validacao-execution.md
│   ├── revisao-execution-etapa1.md
│   ├── revisao-execution-etapa2.md
│   ├── revisao-execution-etapa3.md
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

- [Operação da plataforma AWS dos microsserviços](operacao/plataforma-aws.md).

## Por onde começar

- Para executar: siga o [README da raiz](../README.md#subir-os-execution-e-billing), incluindo pagamentos quando usar orçamento positivo.
- Para alterar uma API: consulte o [guia OS](operacao/os-service.md), os contratos e as decisões correspondentes.
- Para testar o ciclo sem compra externa: use a suíte distribuída no Execution; os relatórios locais distinguem simulação HTTP de pagamento real.
- Para operar AWS: comece pelo [guia da plataforma](operacao/plataforma-aws.md); confira bootstrap, variáveis, CI e o agendamento que descarta dados.

Relatórios em operacao registram verificações em momentos específicos. As fontes de verdade para comandos e parâmetros são Compose, projetos, workflows e catálogos de entrega da revisão em uso.
