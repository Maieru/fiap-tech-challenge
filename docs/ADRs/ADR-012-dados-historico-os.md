# ADR-012 — PostgreSQL exclusivo e histórico independente dos cadastros ativos

- **Status:** Aceita
- **Data:** 2026-10-31
- **Escopo:** dados do OS Service

## Contexto

O novo serviço precisa gravar ordem e histórico atomicamente e preservar ordens após soft delete de cliente/veículo. O legado usa referências vivas e possui a limitação documentada na ADR-004. A equipe escolheu PostgreSQL para o OS; o requisito NoSQL será atendido posteriormente em outro contexto.

## Alternativas consideradas

- Reutilizar tabelas do monólito: mantém acesso cruzado e impede autonomia.
- Bloquear exclusões com ordens vinculadas: altera o comportamento de cadastro desejado.
- Criar snapshots completos de clientes/veículos: amplia a duplicação e o modelo sem necessidade nesta API inicial.
- Banco exclusivo com leitura histórica baseada nos dados da própria ordem: preserva as referências e mantém consultas independentes dos filtros cadastrais.

## Decisão

Usar PostgreSQL 18.3 exclusivo, migrations próprias e transação local para OS + primeiro histórico. Aplicar soft delete com `Ativo=false`, sem cascata. As atualizações preservam a flag e usam concorrência otimista. Índices únicos parciais protegem documentos e placas somente de registros ativos. Permitir novo cadastro de inativos com outro ID.

Consultar ordem e histórico sem navegação obrigatória para cadastros filtrados. Esses endpoints retornam os IDs originais e não reativam nem substituem referências por um novo cadastro com o mesmo documento/placa.

## Consequências

Dados inativos permanecem armazenados; exclusão física e reativação não são oferecidas. Consultar uma OS não recupera automaticamente dados pessoais atuais de um cliente inativo. A regra de status inicial é explicitada também no banco; novas transições exigirão evolução do domínio e migration correspondente.

## Evidências

- Migration e configuração em `src/Services/OrdensServico/FIAP.TechChallenge.OS.Infrastructure/Persistence`.
- Testes HTTP, BDD e rollback em `FIAP.TechChallenge.OS.Integration.Tests`.
- [Arquitetura](../arquitetura/fase4-incremento1.md).
