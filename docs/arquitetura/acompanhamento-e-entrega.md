# Acompanhamento privado e entrega da ordem

O backend distribuído permite que o cliente consulte status, datas, total, itens do orçamento e etapa do pagamento, aprove a execução e solicite cancelamento antes da aprovação. A entrega é administrativa. O frontend continua consumindo o legado; esta implementação não migra telas nem envia links automaticamente.

## Acesso administrativo e link do cliente

Faça cadastro/login no OS para obter um JWT administrativo, conforme [autenticação no OS](autenticacao-os.md). Com esse JWT:

```http
POST /api/os/v1/ordens/{id}/acesso-cliente
Authorization: Bearer <jwt>
```

A resposta 200 contém `ordemServicoId`, `token` e `acompanhamentoUrl`, uma URL relativa. Combine-a com a origem pública da API OS e compartilhe de forma privada. Não use o endereço do frontend atual: a rota retorna JSON. A operação pode ser repetida para invalidar o link anterior. O token só aparece nessa resposta, permanece válido até a próxima emissão e não é recuperável pelo banco, que guarda somente seu hash.

| Operação do cliente, sem JWT | Rota |
| --- | --- |
| Consultar ordem e orçamento | `GET /api/os/v1/ordens/acompanhamento/{id}?token=<token>` |
| Aprovar orçamento | `PUT /api/os/v1/ordens/acompanhamento/{id}/aprovar-execucao?token=<token>` |
| Pedir cancelamento antes da aprovação | `PUT /api/os/v1/ordens/acompanhamento/{id}/cancelar?token=<token>` |

Token ausente, inválido, revogado ou de outra ordem retorna 401, inclusive quando a ordem não existe. Aprovar/cancelar retorna 202 após gravar a decisão e publicar no Kafka; o status operacional aguarda confirmação do Execution. A decisão repetida não republica. A decisão oposta retorna 409. Aprovação dispara pagamento no Billing e só depois libera Execution. Recusa do pagamento compensa o estoque pelo fluxo existente.

A consulta não expõe CPF, contatos, IDs de cliente/veículo ou gateway. O orçamento é uma projeção imutável do evento enviado pelo Execution. Ordens antigas podem retornar `orcamento: null` e não recebem itens inventados. Valores são decimais exatos, incluindo zero.

O link concede acesso às três operações: trate-o como uma senha. As respostas possuem `no-store` e `no-referrer`. Use HTTPS e configure proxies/observabilidade para não registrar query strings. Não inclua o token em evidências ou capturas de logs.

## Entrega administrativa

```http
PUT /api/os/v1/ordens/{id}/entregar
Authorization: Bearer <jwt>
```

Somente `Finalizada` (5) pode avançar para `Entregue` (6). A resposta 200 inclui a data UTC de entrega. Repetições, inclusive concorrentes, preservam essa data e um único histórico `OrdemEntregue`. Recebida, em diagnóstico, aguardando pagamento/aprovação, em execução e cancelada não podem ser entregues. Sem JWT retorna 401.

```mermaid
flowchart LR
    A[Aprovação do cliente ou administrador] --> B[Aguardando pagamento]
    B --> C[Pagamento confirmado]
    C --> D[Execution inicia e conclui]
    D --> E[OS Finalizada: 5]
    E -->|Administrador registra entrega| F[OS Entregue: 6]
```

A Saga financeira permanece `Concluida` (3) após a entrega. Execution mantém sua execução `Finalizada` e Billing mantém a cobrança confirmada: entrega não gera nova cobrança, evento ou movimentação de estoque. Reentrega tardia de confirmação do Execution não regride o OS.

## Evidências

`EntregaEOrcamentoTests` cobre invariantes e snapshots. `OsMessagingTests` usa HTTP, JWT, PostgreSQL e Kafka reais para testar acesso por ordem, rotação, hash persistido, decisões, entrega concorrente e eventos tardios. `FluxoDistribuido.feature`, no Execution, inclui aprovação pelo link seguida de pagamento, execução e entrega, além de cancelamento pelo link com devolução única de estoque.

Resultados da validação desta implementação ficam em `artifacts/customer-delivery` no OS e `artifacts/customer-delivery-distributed` no Execution. Consulte também a [Saga](saga-pagamento.md), a [ADR-019](../ADRs/ADR-019-entrega-acesso-cliente.md) e a [RFC-012](../RFCs/RFC-012-entrega-acompanhamento-os.md).

Validação final: 265 testes OS sem imagem, 2 testes de imagem e 9 cenários distribuídos aprovados; cobertura de linhas OS de 90,9%. Evidências consolidadas na [validação ponta a ponta](../operacao/validacao-ponta-a-ponta.md).
