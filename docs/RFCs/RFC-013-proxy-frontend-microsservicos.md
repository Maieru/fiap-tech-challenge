# RFC-013 — Proxy configurável e contratos do frontend

- **Status:** Aceita
- **Data:** 2026-11-02
- **Escopo:** integração React/Vite/Nginx com OS, Execution e Billing

## Contexto

Uma base HTTP única e os DTOs do monólito não atendiam às rotas e respostas dos serviços extraídos.

## Decisão

Usar instâncias Axios administrativas para OS, Execution e Billing com interceptadores comuns de JWT/401 e uma instância pública isolada. As bases padrão são relativas à origem. `VITE_API_GATEWAY_URL` configura um gateway público no build; `VITE_*_API_BASE_URL` permite bases completas independentes. A antiga `VITE_API_BASE_URL` não participa da integração nova.

Vite carrega `OS_UPSTREAM`, `EXECUTION_UPSTREAM`, `BILLING_UPSTREAM` ou `API_GATEWAY_UPSTREAM` no servidor. Nginx usa os três upstreams por template de ambiente, sem rebuild. Upstreams incluem protocolo, host e eventual prefixo de stage, sem barra final; não incluem `/api/.../v1`. Proxy mantém caminho, query, verbo e Authorization. HTTPS usa SNI e Host do upstream. Configuração same-origin evita CORS; hospedagem estática com gateway em outra origem exige CORS no gateway.

A imagem usa `npm ci` com lockfile sincronizado e exclui `.env*` do contexto. Consultas de confirmação ocorrem a cada três segundos em páginas visíveis, com uma requisição periódica por vez e limpeza no unmount. Erros de leitura não são transformados em sucesso nem em recurso ausente, exceto 404 explicitamente esperado para criação assíncrona da execução/cobrança.

## Alternativas consideradas

- Acrescentar framework de cache/polling: não é necessário para as duas telas afetadas.
- Configurar CORS em cada serviço: desnecessário com proxy na mesma origem.
- Reconstruir a imagem por ambiente: o Nginx já suporta upstreams em execução.

## Consequências e evidências

`npm test` usa Node test runner e o carregador do Vite para verificar contratos de integração, composição de DTOs, roteamento, JWT e isolamento dos links. Build TypeScript/Vite e imagem Nginx verificam os caminhos de distribuição. O smoke HTTP deve usar stack com volumes e rede exclusivos; ver instruções no README do frontend. Testes backend não precisam ser repetidos quando seus contratos não foram alterados.

Ver [ADR-020](../ADRs/ADR-020-frontend-microsservicos-mesma-origem.md).
