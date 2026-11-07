# Migração do frontend para microsserviços

Atualizado em 2026-11-02. O frontend foi adaptado para OS, Execution e Billing e incluído no Compose integrado. Não há fallback ao monólito. A implementação e a verificação local não representam implantação dos serviços na AWS. A [retirada posterior do código do monólito](retirada-monolito.md) não apaga recursos ou dados legados.

## Distribuição das chamadas

| Responsabilidade | Serviço e prefixo |
| --- | --- |
| Login/cadastro administrativo, clientes e veículos | OS: `/api/os/v1` |
| Abertura/lista/consulta, aprovação/cancelamento administrativo, entrega | OS: `/api/os/v1/ordens` |
| Emissão/troca de link | OS: `POST /api/os/v1/ordens/{id}/acesso-cliente` |
| Acompanhamento/decisões pelo token privado | OS: `/api/os/v1/ordens/acompanhamento/{id}` |
| Catálogo, estoque e execução | Execution: `/api/execution/v1` |
| Cobrança por ordem | Billing: `/api/billing/v1/ordens/{id}/cobranca` |

`services/api.ts` fornece três clientes administrativos com JWT e um público sem JWT. Um 401 público não apaga a sessão administrativa. Login mantém o DTO `token`, `tipoToken`, `expiresInSeconds`. As três APIs compartilham issuer, audience e chave; usuários antigos não são importados.

## Contratos e estados

Lista, status e datas vêm do OS, inclusive entrega. Detalhes compõem a ordem OS e `{ execucao, valorTotal }` do Execution. Snapshots dos itens não incluem seus totais, calculados para apresentação; o total geral continua vindo do Execution. Cobrança é consultada no Billing de forma independente.

404 do Execution significa execução ainda não criada: a tela aguarda e não apresenta total zero. 404 da cobrança significa ainda não emitida; erros técnicos são exibidos como indisponibilidade. Aprovação/cancelamento retornam 202 e não alteram visualmente o status para um destino ainda não confirmado. As páginas consultam a cada três segundos enquanto visíveis, sem sobrepor consultas periódicas. Exibem espera por pagamento, autorização, cancelamento ou compensação. As ações operacionais conferem o estado do OS e Execution.

Conclusão de serviço inclui IDs da ordem e do item. Entrega é administrativa no OS, após Finalizada; OS fica Entregue (6) enquanto Execution mantém Finalizada (5). Não há criação manual de cobrança nem DELETE de ordens.

Cliente, veículo e ordem são criados em etapas; IDs confirmados são guardados no formulário e selecionados para uma nova tentativa. Se uma etapa falhar, as anteriores não são recriadas. Se uma requisição terminar em timeout após commit, ainda é necessário conferir o cadastro: as APIs não possuem chave de idempotência.

## Acesso do cliente e privacidade

Emissão de link é explícita, autenticada e informa que substitui o anterior. A URL usa a rota da página React e o token retornado, não a rota JSON relativa do backend. Caso a área de transferência falhe, o endereço permanece visível para cópia. Consulta administrativa não emite nem recupera tokens.

O acompanhamento usa o DTO mínimo do OS: `ordemServicoId`, descrição, status/datas, orçamento e etapa de pagamento. Não consulta dados pessoais ou Execution/Billing com credenciais administrativas. Permite aprovar/cancelar antes da aprovação, confirma a decisão e trata 202/conflito sem prometer processamento concluído.

Páginas abertas por link não inicializam OpenTelemetry; requisições públicas são excluídas da instrumentação mesmo após navegação SPA. URL de logs administrativos omite query, erros são filtrados e o Vite remove tokens de mensagens do proxy. Nginx não registra queries e desliga logs das rotas de token. A página e APIs usam `no-store`/`no-referrer`. API Gateway/ALB e outros proxies implantados também precisam omitir queries; isso não foi configurado remotamente por esta mudança.

## Local e nuvem

A [ADR-022](../ADRs/ADR-022-kubernetes-microsservicos.md) acrescenta os manifestos das APIs: Services ClusterIP em HTTP 8080, HPA de 1 a 3 réplicas, credenciais exclusivas por banco e OpenTelemetry ativo para o Collector compartilhado. A entrada pública continua HTTPS; não há mais exigência de TLS/CA privada entre Nginx e as APIs nesta etapa. Consulte o [roteiro Kubernetes](../../k8s/README.md).

Vite encaminha os três prefixos para portas 8081/8082/8083; Nginx usa upstreams configuráveis em execução. Kong não é obrigatório localmente. A mesma imagem atende à nuvem, atrás de API Gateway e ALB, com upstreams internos. Os manifests do frontend incluem um Ingress dos três prefixos com prioridade anterior ao `/api` legado. Os nomes dos Services no ConfigMap devem corresponder aos serviços provisionados antes do rollout.

Outra topologia pode encaminhar Nginx para um gateway separado; ele não pode devolver a mesma chamada ao Nginx. Hospedagem puramente estática pode definir gateway/bases públicas no build e exige CORS na borda. Os arquivos `.env*` locais são excluídos do build Docker. `VITE_API_BASE_URL` do legado é ignorada.

Comandos, variáveis e smoke isolado estão no [README do frontend](../../src/FIAP.TechChallenge.Fase1.Frontend/README.md). Decisões: [ADR-020](../ADRs/ADR-020-frontend-microsservicos-mesma-origem.md) e [RFC-013](../RFCs/RFC-013-proxy-frontend-microsservicos.md).

## Verificação e limites

Resultados e evidências da sessão: [verificação local do frontend](../operacao/validacao-frontend.md).

- Testes Node/Vite: bases same-origin/gateway, composição, estados ausentes, rotas, JWT, expiração e isolamento de 401 público/administrativo.
- Build TypeScript/Vite e imagem Linux com `npm ci`; configuração Nginx verificada com `nginx -t`.
- Smoke HTTP através do Nginx contra três APIs/Kafka/bancos em rede e volumes exclusivos: cadastro/login, catálogo/estoque, abertura, diagnóstico, orçamento, rotação de link, aprovação/pagamento mockado, conclusão, entrega e cancelamento idempotente com devolução de estoque.
- Verificação no navegador de login, lista e detalhes, incluindo orçamento público e entrega. A primeira inspeção detectou limite de página do Execution (100), corrigido nas consultas dos seletores.
- O lint global tem erros anteriores em componentes UI, AuthContext e outras listagens; o lint dos serviços, tipos, páginas de ordens, observabilidade e Vite alterados passa.

Pendências: provisionar/implantar os serviços e suas dependências na AWS, validar roteamento/JWT/privacidade na borda real, trocar pagamento mockado por gateway real e executar a retirada dos recursos/dados legados quando os critérios de produção forem atendidos. O código e a automação exclusiva do monólito já foram retirados. Os fluxos de indisponibilidade prolongada/compensação por recusa continuam dependendo dos cenários distribuídos backend; o smoke frontend usa o mock de aprovação existente.
