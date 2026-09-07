# ADR-002 — Comunicação síncrona por HTTP/JSON e REST pragmático

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** integração entre frontend e backend
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

A SPA precisa consultar e alterar dados da oficina com resposta imediata. Não há broker de mensagens no fluxo de negócio.

## Decisão

Usar HTTP/JSON síncrono sob `/api`, com recursos CRUD e ações como `iniciar-diagnostico` e `aprovar-execucao`.

- A SPA usa Axios, timeout de 20 segundos e JWT Bearer nas chamadas autenticadas.
- Controllers traduzem `Result<T>` para HTTP pelo mapeamento compartilhado.
- API Gateway e ALB encaminham as chamadas; algumas rotas também usam autorização no gateway, conforme a ADR-005.
- Novos erros de negócio usam `ErrorCode` e o mapeamento comum.

## Alternativas consideradas

- **GraphQL ou gRPC:** exigiriam outro contrato sem necessidade atual.
- **Mensageria:** acrescentaria consistência eventual a operações que precisam de resposta imediata.
- **Frontend acessando o banco:** contornaria regras e controles da API.

## Consequências

- O contrato é simples para navegadores e ferramentas HTTP.
- Cliente, API e dependências precisam estar disponíveis durante a chamada.
- Notificações ainda são enviadas durante a requisição; não há entrega assíncrona durável.
- Erros de model binding e exceções não tratadas não usam necessariamente o envelope de `Result<T>`.
- Retentativas de comandos exigem idempotência; o contrato ainda não tem versão no caminho.

Reconsiderar com consumidores externos, operações longas ou necessidade de eventos duráveis.

## Evidências e relações

- [Controllers da API](../../src/FIAP.TechChallenge.Fase1.API/Controllers)
- [Mapeamento de resultados para HTTP](../../src/FIAP.TechChallenge.Fase1.API/Extensions/ResultExtensions.cs)
- [Cliente HTTP do frontend](../../src/FIAP.TechChallenge.Fase1.Frontend/src/services/api.ts)
- [Configuração da API](../../src/FIAP.TechChallenge.Fase1.API/Program.cs)
- [ADR-005 — Topologia de entrada na AWS](ADR-005-topologia-de-entrada-na-aws.md)
