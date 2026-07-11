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

Execute a partir da raiz do repositorio (`src`):

```bash
docker compose up --build
```

Servicos disponiveis:

- Frontend: `http://localhost:5173`
- API: `http://localhost:8080`
- PgAdmin: `http://localhost:5050`
- Postgres: `localhost:5432`

No modo Docker, o frontend ja e buildado com `VITE_API_BASE_URL=http://localhost:8080/api`.

## Execucao local (sem Docker)

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

Para visualizar os traces, suba o ambiente de observabilidade e acesse o Jaeger em `http://localhost:16686`, selecionando o servico `fiap-tech-challenge-frontend`.

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
