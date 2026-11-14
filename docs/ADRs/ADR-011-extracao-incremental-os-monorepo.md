# ADR-011 — Responsabilidade e isolamento do OS Service

- **Status:** Aceita
- **Data:** 2026-10-31
- **Última revisão:** 2026-11-14
- **Escopo:** limites do OS Service

## Contexto

O ciclo da oficina distribui responsabilidades entre OS, Execution e Billing. O OS precisa evoluir com domínio, aplicação, testes, imagem e banco próprios.

## Decisão

Manter no OS clientes, veículos, abertura e consulta de ordens, histórico, autenticação administrativa e coordenação da Saga. Execution controla diagnóstico, catálogo, estoque e execução; Billing controla cobrança e pagamento.

OS e frontend ficam neste repositório. Execution, Billing e Chassis possuem repositórios próprios. Não referenciar assemblies ou bancos de negócio de outro serviço. Compartilhar somente recursos técnicos e contratos versionados por NuGet.

## Alternativas consideradas

Compartilhar módulos de negócio criaria coordenação entre releases. Consultar diretamente bancos de outros serviços romperia a autonomia. Contratos e eventos explícitos mantêm os limites verificáveis.

## Consequências

Cada serviço aplica migrations e valida seus próprios dados. Coordenação entre serviços tem consistência eventual. Build e testes independentes não substituem cenários distribuídos ou validação de implantação.

## Evidências

- [Solução OS](../../Backend.slnx).
- [Dados e histórico OS](ADR-012-dados-historico-os.md).
- [Chassi independente](ADR-013-chassi-em-repositorio-proprio.md).
- [Estado da aplicação](../arquitetura/estado-da-migracao.md).
