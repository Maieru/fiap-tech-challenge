# Oficina Frontend (MVP)

Frontend administrativo para oficina mecânica, implementado com:

- React
- Vite
- TypeScript
- Tailwind CSS
- shadcn/ui
- React Router
- Axios

## Estrutura de pastas

```txt
src/
  components/
    common/
    ui/
  contexts/
  hooks/
  layouts/
  lib/
  pages/
    auth/
    dashboard/
    clientes/
    veiculos/
    servicos/
    pecas-insumos/
    ordens-servico/
  routes/
  services/
  types/
```

## Funcionalidades implementadas

- Login administrativo com JWT.
- Persistência de sessão em `localStorage`.
- Interceptor Axios com `Authorization: Bearer <token>`.
- Logout automático em `401`.
- Rotas protegidas.
- Layout administrativo com sidebar, header e conteúdo responsivo.
- CRUD de:
  - Clientes
  - Veículos
  - Serviços
  - Peças/Insumos (com entrada de estoque)
- Ordens de serviço:
  - Listagem
  - Criação (cliente/veículo existente ou novo + itens)
  - Detalhes com orçamento e timeline
  - Avanço de status (Recebida -> Entregue)
- Feedback visual com toasts (sucesso/erro) e estados de carregamento.

## Configuração

1. Copie o arquivo de exemplo:

```powershell
Copy-Item .env.example .env
```

2. Ajuste a URL da API no `.env`:

```env
VITE_API_BASE_URL=http://localhost:5251/api
```

## Subindo tudo com Docker Compose

Na raiz do repositório, copie `src/.env.example` para `src/.env` e preencha `NEW_RELIC_LICENSE_KEY`, conforme o [guia de observabilidade](../../docs/operacao/observabilidade.md). Esse arquivo é separado do `.env` do frontend. Depois execute:

```bash
docker compose -f src/docker-compose.yml up --build
```

Servicos disponiveis:

- Frontend: `http://localhost:5173`
- API: `http://localhost:8080`
- PgAdmin: `http://localhost:5050`
- Postgres: `localhost:5432`

No build Docker padrão, sem `VITE_API_BASE_URL` definida, o frontend usa `/api` na mesma origem; o Nginx encaminha as chamadas ao backend configurado em `API_UPSTREAM`. O `.env` do frontend também entra no contexto de build: remova a definição local de `VITE_API_BASE_URL` ou use `/api` antes de construir a imagem. Essa é uma configuração de build, não uma variável de execução do contêiner.

## Execucao local (sem Docker)

Requer Node.js 22.12 ou superior na linha 22, ou uma versão posterior compatível com o Vite. Execute na pasta `src/FIAP.TechChallenge.Fase1.Frontend`, com a API disponível no endereço configurado no `.env`:

```bash
npm install
npm run dev
```

Aplicação disponível em: `http://localhost:5173`

## OpenTelemetry

O frontend gera traces para carregamento da pagina, requisicoes HTTP (`fetch` e Axios/XHR) e erros nao tratados. Em desenvolvimento, o Vite encaminha `/otlp` para o Collector em `http://localhost:4318`; no Docker e no Kubernetes, esse encaminhamento e feito pelo Nginx.

Configuracoes opcionais de build:

```env
VITE_OTEL_SERVICE_NAME=fiap-tech-challenge-frontend
VITE_OTEL_EXPORTER_URL=/otlp/v1/traces
VITE_OTEL_LOGS_EXPORTER_URL=/otlp/v1/logs
VITE_APP_VERSION=1.0.0
```

Para visualizar traces e logs, configure o New Relic seguindo o [guia de observabilidade](../../docs/operacao/observabilidade.md) e procure o serviço `fiap-tech-challenge-frontend`.

## Build de produção

```bash
npm run build
npm run preview
```

## Pontos de adaptação rápida

- Cliente HTTP: `src/services/api.ts`
- Serviços por módulo: `src/services/*.service.ts`
- Tipos de dados: `src/types/*`
- Rotas da app: `src/routes/AppRouter.tsx`

Se o backend mudar payloads/rotas, os ajustes ficam concentrados em `services` e `types`.
