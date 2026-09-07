# ADR-004 — Preservação do histórico operacional

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** domínio e dados
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

Alterações no catálogo não devem mudar valores e descrições de ordens já registradas. O histórico implementado é parcial, sem auditoria de todas as alterações.

## Decisão

- Copiar descrição, identificação, valor unitário e quantidade dos serviços, peças e insumos ao vinculá-los à ordem.
- Calcular os totais com esses snapshots, sem atualizar os itens quando o catálogo mudar.
- Manter estado atual e timestamps das etapas instrumentadas da ordem; cancelamento ainda não tem timestamp próprio.
- Excluir logicamente com `Ativo = false` e ocultar inativos por filtros globais do EF Core.
- Reservar exclusão física e consultas que ignoram filtros a fluxos explícitos.

## Alternativas consideradas

- **Consultar sempre o catálogo atual:** alteraria retroativamente os dados da ordem.
- **Exclusão física comum:** poderia comprometer referências e histórico.
- **Auditoria completa ou event sourcing:** ampliariam o modelo além do requisito atual.

## Consequências

- Snapshots preservam o orçamento, com duplicação intencional de dados.
- Registros inativos continuam armazenados, mas não aparecem nas consultas normais.
- Cliente e veículo são referências vivas, sem snapshot. Inativá-los pode impedir o acompanhamento da ordem.
- Retenção física não garante acesso ao histórico nem define regras de anonimização, apagamento ou reativação.
- Fluxos históricos precisam decidir como tratar cadastros inativos: bloquear a inativação, consultar o histórico explicitamente ou criar snapshots.

Reconsiderar quando houver exigência de auditoria completa, retenção ou apagamento de dados.

## Evidências e relações

- [Fluxo da ordem de serviço](../../src/FIAP.TechChallenge.Fase1.Domain/Entities/OrdemServico.cs)
- [Snapshot de serviço](../../src/FIAP.TechChallenge.Fase1.Domain/Entities/ServicoDaOrdemDeServico.cs)
- [Snapshot de peça ou insumo](../../src/FIAP.TechChallenge.Fase1.Domain/Entities/PecaOuInsumoDaOrdemDeServico.cs)
- [Filtros globais](../../src/FIAP.TechChallenge.Fase1.Infrastructure/Persistence/AppDbContext.cs)
- [Testes de soft delete](../../src/FIAP.TechChallenge.Fase1.Infaestructure.Tests/Persistence/SoftDeleteQueryFilterTests.cs)
