# ADR-004 — Preservação do histórico operacional

- **Status:** Aceita
- **Data:** 2026-08-23
- **Última revisão:** 2026-11-14
- **Escopo:** domínio e dados dos microsserviços

## Contexto

Mudanças no catálogo não devem alterar os valores de um orçamento já registrado. Inativar um cliente ou veículo não deve ocultar a ordem nem seu histórico.

## Decisão

Execution mantém snapshots dos serviços, peças e insumos vinculados à execução. OS persiste a referência e o total do orçamento, o pagamento e os eventos relevantes do histórico. Clientes e veículos usam exclusão lógica; ordens e histórico permanecem consultáveis pelos seus próprios IDs.

Não inventar um orçamento ausente nem iniciar cobrança sem referência e total válidos. A consistência de ordem e histórico é local ao banco OS.

## Alternativas consideradas

Consultar apenas o catálogo atual alteraria valores contratados. Excluir fisicamente cadastros no fluxo comum comprometeria referências. Event sourcing ampliaria o modelo e a operação além do que está implementado.

## Consequências

Snapshots duplicam dados intencionalmente. A retenção física não define política de anonimização, apagamento ou reativação. O histórico operacional não constitui auditoria completa de todas as alterações.

## Evidências e relações

- [Dados e bancos](../arquitetura/banco-de-dados.md).
- [Ordem de serviço OS](../../src/FIAP.TechChallenge.OS.Domain/Entities/OrdemServico.cs).
- [Configuração relacional](../../src/FIAP.TechChallenge.OS.Infrastructure/Persistence/AppDbContext.cs).
- [Dados e histórico OS](ADR-012-dados-historico-os.md).
- [Execution](../arquitetura/execution.md).
