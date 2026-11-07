# Oficina Frontend — microsserviços

React, TypeScript, Vite, Tailwind e Axios. O frontend usa OS para autenticação, clientes, veículos, ordens e acompanhamento; Execution para catálogo/estoque e execução; Billing para consulta da cobrança. Não depende do monólito.

## Ambiente local com Docker

Na raiz de `FiapTechChallengeFase1`, mantenha os checkouts Execution e Billing ao lado, copie `.env.example` para `.env` e preencha as opções JWT e senhas conforme o README principal. Execute:

```powershell
docker compose up -d --build
```

- Frontend: [localhost:5173](http://localhost:5173)
- OS: porta 8081; Execution: 8082; Billing: 8083.

Cadastre um usuário na tela de login. Os bancos novos não importam usuários do monólito. O JWT emitido pelo OS é aceito pelos três serviços, que precisam compartilhar issuer, audience e chave.

O navegador acessa `/api/os/v1`, `/api/execution/v1` e `/api/billing/v1` na origem do frontend; Nginx encaminha as chamadas. Kong não é necessário. O Compose e o código do monólito foram retirados; use o Compose da raiz. Recursos e dados AWS antigos não foram apagados por essa retirada.

## Desenvolvimento com Vite

Requer Node 22.12+ (ou versão posterior compatível). Com as três APIs rodando:

```powershell
Copy-Item .env.example .env
npm ci --ignore-scripts
npm run dev
```

O Vite usa por padrão OS `http://localhost:8081`, Execution `http://localhost:8082` e Billing `http://localhost:8083`. `OS_UPSTREAM`, `EXECUTION_UPSTREAM` e `BILLING_UPSTREAM` substituem esses destinos no `.env`. `API_GATEWAY_UPSTREAM` direciona os três prefixos a um gateway, inclusive Kong caso já exista. Upstreams têm protocolo, host e eventual stage, sem barra final ou `/api/.../v1`.

A antiga `VITE_API_BASE_URL` é ignorada. Não é necessário configurar CORS nas APIs para este modo same-origin. `npm run preview` serve o build estático e não fornece o proxy das APIs: para validar a distribuição, use a imagem Nginx.

## Nuvem com API Gateway

A mesma imagem Docker usa variáveis de execução:

| Variável | Conteúdo |
| --- | --- |
| `OS_UPSTREAM` | Origem HTTP(S) do OS ou gateway separado |
| `EXECUTION_UPSTREAM` | Origem HTTP(S) do Execution ou gateway separado |
| `BILLING_UPSTREAM` | Origem HTTP(S) do Billing ou gateway separado |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Collector OTLP HTTP, por exemplo `http://collector:4318` |
| `UPSTREAM_CA_CERTIFICATE` | Caminho do bundle PEM de CAs confiáveis; padrão: raízes do sistema |

A imagem usa `npm ci --ignore-scripts` e Nginx sem root (UID/GID 101), ouvindo na porta interna **8080**. O Compose continua publicando 5173. No EKS de laboratório, a entrada pública usa HTTPS e os upstreams internos usam HTTP: APIs em 8080 e Collector em 4318. Não há Secret de CA obrigatório. Chamadas a um upstream HTTPS futuro continuam validando cadeia e hostname; para CA privada, monte um bundle somente leitura e configure `UPSTREAM_CA_CERTIFICATE`. Ver [pré-requisitos Kubernetes](../../k8s/README.md) e [ADR-022](../../docs/ADRs/ADR-022-kubernetes-microsservicos.md).

Há duas topologias possíveis:

1. API Gateway serve a origem pública: Gateway → VPC Link → ALB → Nginx → três Services internos. O ConfigMap em `k8s/frontend` contém os nomes esperados; ajuste aos Services provisionados. O Ingress de microsserviços tem prioridade 5, antes do `/api` legado de prioridade 10. O caminho é preservado.
2. Frontend tem origem própria e encaminha a um gateway separado: atribua a origem do gateway aos três upstreams. Não use esse modo se o gateway encaminha a mesma requisição de volta ao Nginx, pois isso cria um ciclo.

Para hospedagem puramente estática, o build pode usar `VITE_API_GATEWAY_URL=https://gateway.example.com` ou bases completas `VITE_OS_API_BASE_URL`, `VITE_EXECUTION_API_BASE_URL`, `VITE_BILLING_API_BASE_URL`. Esse modo exige CORS no gateway, permitindo a origem do frontend, os verbos usados e Authorization/Content-Type/traceparent. A imagem padrão usa bases relativas e exclui `.env*` do contexto; configurar `VITE_*` no ambiente de um contêiner já construído não altera o JavaScript.

Os manifests preparam o roteamento, mas as APIs, seus Services/bancos/Kafka e segredos precisam ser implantados antes do rollout. Esta migração não executa deploy nem modifica o tráfego AWS existente. O gateway deve deixar consulta/decisões do cliente passarem com o token privado até o OS; o authorizer legado por CPF não se aplica a esses caminhos. Logs de API Gateway/ALB também devem omitir queries.

## Fluxos adaptados

- Cadastro de cliente, veículo e ordem é sequencial; IDs de etapas confirmadas são reutilizados após falha. Timeout após commit ainda exige conferir o cadastro, pois o backend não oferece chave de idempotência.
- Lista/status/datas e entrega vêm do OS. Itens e totais administrativos vêm do Execution; cobrança vem do Billing. Execução ainda não criada é uma pendência, com valores indisponíveis.
- Aprovação/cancelamento retornam 202. A tela mostra espera por pagamento, autorização ou compensação e atualiza a cada três segundos enquanto visível. A execução só é liberada quando confirmada.
- Conclusão usa IDs da ordem e do item no Execution. Entrega muda o OS para Entregue; Execution permanece Finalizada.
- Não há DELETE de ordens no OS; a ação foi retirada.
- Emissão/troca de link é explícita e invalida o anterior. O link aparece para cópia manual caso a área de transferência não esteja disponível. O cliente consulta orçamento e aprova/cancela pelo token privado; um link inválido não encerra a sessão administrativa.
- Acompanhamento não inicia telemetria; chamadas privadas são excluídas da instrumentação e o token é filtrado dos erros Vite. Nginx omite queries nos logs e envia `no-store`/`no-referrer`.

## Verificação

Resultados locais e limites conhecidos: [relatório de verificação](../../docs/operacao/validacao-frontend.md). O CI ganhou um job de frontend com testes, lint do escopo migrado, build e imagem; sua execução remota ainda não foi realizada nesta sessão.

```powershell
npm test
npm run build
npm run lint
```

Os testes Node/Vite verificam configuração de gateway, DTOs, rotas, JWT compartilhado, expiração, 401 público/administrativo e ausência transitória da projeção. O lint global ainda tem falhas anteriores em componentes UI, AuthContext e telas de listagem; o conjunto alterado pode ser conferido com:

```powershell
npx eslint src/services src/types/ordemServico.ts src/pages/ordens-servico src/observability vite.config.ts
```

Smoke HTTP cria somente dados sintéticos e exige stack isolada. Na raiz do repositório, com imagens backend disponíveis e Docker Compose 2.24.4+:

```powershell
docker compose -p fiap-frontend-verification -f compose.yaml -f src/FIAP.TechChallenge.Fase1.Frontend/tests/compose.smoke.yaml up -d --build
cd src/FIAP.TechChallenge.Fase1.Frontend
$env:FRONTEND_SMOKE_ISOLATED = 'true'
npm run test:smoke
```

Portas isoladas: frontend 15173, APIs 18081–18083. O nome de projeto separa rede e volumes. O smoke verifica o ciclo completo e grava credenciais/links temporários em `artifacts/frontend/smoke-browser.json` (ignorado pelo Git) para inspeção no navegador. Ao terminar, remova apenas a stack de teste com o mesmo comando de Compose substituindo `up -d --build` por `down -v`.

## Teste de segurança da imagem

Na raiz, com PowerShell 7 e Docker:

```powershell
docker build -t fiap-frontend:sonar-remediation src/FIAP.TechChallenge.Fase1.Frontend
./scripts/Test-FrontendSecurity.ps1
```

O teste cria uma CA temporária apenas nos containers, verifica usuário sem root, proxy HTTPS, preservação de Authorization e rejeição de CA/hostname inválidos. Remove seus containers, rede e certificados ao terminar.

## Observabilidade

Vite e Nginx encaminham `/otlp` ao Collector. O Compose integrado não exige New Relic: configure o Collector separadamente se quiser exportar traces/logs administrativos. Opções de build: `VITE_OTEL_SERVICE_NAME`, `VITE_OTEL_EXPORTER_URL`, `VITE_OTEL_LOGS_EXPORTER_URL`, `VITE_APP_VERSION`.

Decisões e limites: [guia da migração](../../docs/arquitetura/migracao-frontend.md), [ADR-020](../../docs/ADRs/ADR-020-frontend-microsservicos-mesma-origem.md), [RFC-013](../../docs/RFCs/RFC-013-proxy-frontend-microsservicos.md). Referências do proxy: [Vite](https://vite.dev/config/server-options#server-proxy), [Nginx](https://nginx.org/en/docs/http/ngx_http_proxy_module.html#proxy_pass) e [ordenação do ALB](https://kubernetes-sigs.github.io/aws-load-balancer-controller/latest/guide/ingress/annotations/#group.order).
