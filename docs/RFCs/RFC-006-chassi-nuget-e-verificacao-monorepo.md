# RFC-006 — Chassi NuGet e verificação dos componentes

- **Status:** Aceita
- **Data:** 2026-10-31
- **Última revisão:** 2026-11-14
- **Escopo:** dependências técnicas, soluções e CI

## Contexto

Os serviços precisam compartilhar recursos técnicos sem compartilhar regras de negócio, domínio ou banco. A validação deve acompanhar o código de cada componente.

## Decisão

Distribuir o chassi técnico e o adaptador Kafka em pacotes NuGet do repositório Chassis. Fixar versões explícitas em `PackageReference`. Contratos de eventos pertencem a seus produtores e são distribuídos em pacotes separados.

`Backend.slnx` reúne os nove projetos OS. O CI principal verifica backend e frontend; Chassis, Execution e Billing mantêm seus próprios builds, testes e publicação. As soluções organizam projetos por componente e testes.

Preservar NUnit/Moq para testes unitários e PostgreSQL/Kafka reais em suítes de integração isoladas. Usar BDD Reqnroll nos fluxos apropriados. A cobertura exige pelo menos 80% de linhas por componente, consolidada por ReportGenerator; Sonar e `ci-required` mantêm os gates de qualidade. A categoria `ContainerImage` identifica testes comportamentais da imagem.

## Alternativas consideradas

Copiar bibliotecas entre repositórios criaria divergências. Compartilhar projetos de negócio pelo chassi romperia a autonomia. Compensar cobertura entre serviços esconderia componentes sem verificação suficiente.

## Consequências e evidências

Pacotes candidatos podem ser validados localmente, mas build com feed local não comprova publicação ou restore em runner limpo. Credenciais ausentes e quality gate reprovado não são sucesso.

- [CI e dependências](../operacao/monorepo-ci.md).
- [Soluções e CI](RFC-014-solucoes-e-ci.md).
- [Chassi independente](../ADRs/ADR-013-chassi-em-repositorio-proprio.md).
