# ADR-019 — Entrega e acesso do cliente à ordem no OS

- Status: Aceita
- Data: 2026-11-02

## Contexto

O fluxo distribuído terminava na finalização da execução. A entrega e o acompanhamento pelo cliente ainda dependiam do legado. A migração do frontend foi cancelada; esta entrega cobre as APIs. O cliente deve consultar, aprovar e cancelar antes da aprovação por link privado, conforme decisão do projeto.

## Decisão

OS controla a entrega administrativa, após `Finalizada`, com data UTC e histórico idempotente. O estado passa a `Entregue` (6). Execution permanece `Finalizada` (5): entrega física não altera trabalho, cobrança ou estoque. Não há evento Kafka novo para essa transição.

Um administrador autenticado emite um token aleatório de 256 bits por ordem. O OS armazena apenas SHA-256, valida o link dentro dos casos de uso e limita seu escopo àquela ordem. Uma nova emissão substitui a anterior. O token permanece válido até a troca; não há expiração automática neste incremento. Quem possui o link pode consultar, aprovar e cancelar antes da aprovação. Não depende de CPF, JWT administrativo ou Lambda Authorizer.

OS projeta os snapshots do `DiagnosticoConcluido` em seu próprio banco para apresentar o orçamento ao cliente. Execution continua dono dos itens e preços. A projeção é imutável, validada contra o total recebido e não recalcula a cobrança. Dados antigos sem snapshot retornam `orcamento: null`; não há preenchimento inventado nem cobrança retroativa.

## Alternativas consideradas

- Consultar Execution por HTTP: adicionaria dependência síncrona e autorização entre serviços.
- Usar documento do cliente como segredo: informação pessoal previsível não comprova acesso à ordem.
- Reutilizar JWT administrativo: daria ao cliente privilégios excessivos e acoplaria os dois tipos de acesso.
- Publicar entrega no Execution: não há responsabilidade operacional naquele serviço que precise dessa informação neste incremento.

## Consequências e evidências

Rotação e decisões usam a mesma trava transacional da ordem. Aprovação/cancelamento reaproveitam a Saga existente. Respostas do cliente não expõem CPF, IDs de cliente/veículo ou identificadores do gateway. Token inválido, ausente ou de outra ordem retorna 401. As respostas não devem ser armazenadas em cache; o link é um segredo e deve ser entregue por canal privado.

Implementação: `AcompanhamentoController`, casos de uso separados, `AcessoClienteService`, migration `EntregaEAcompanhamentoCliente`. Evidências e instruções em [acompanhamento e entrega](../arquitetura/acompanhamento-e-entrega.md). Testes HTTP verificam isolamento, rotação e idempotência; BDD verifica aprovação e cancelamento pelo link com os três serviços reais.
