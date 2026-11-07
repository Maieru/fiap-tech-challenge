# ADR-020 — Frontend distribuído por prefixos e mesma origem

- **Status:** Aceita
- **Data:** 2026-11-02
- **Escopo:** frontend, proxy, execução local e entrada em nuvem

## Contexto

O frontend consumia contratos do monólito. OS, Execution e Billing possuem APIs versionadas, JWT compartilhado e transições assíncronas por Kafka. O frontend deve operar com API Gateway na nuvem e sem exigir um gateway local.

## Decisão

Separar os clientes administrativos por responsabilidade, mantendo os prefixos `/api/os/v1`, `/api/execution/v1` e `/api/billing/v1`. Em desenvolvimento, Vite encaminha os prefixos; na imagem, Nginx encaminha cada prefixo a um upstream configurado em execução. A mesma imagem atende aos dois ambientes sem incorporar `.env` local. Na nuvem, o Gateway preserva os caminhos até a borda; ALB encaminha os prefixos ao Nginx, que acessa os serviços internos. Outra topologia pode apontar os três upstreams a um gateway separado. Não apontar Nginx a um gateway que retorna ao mesmo Nginx.

OS determina status administrativo, datas e entrega. Execution fornece itens e totais administrativos; Billing fornece cobrança. Ausência transitória da execução (404) é exibida como pendência, sem inventar total zero. Aprovação/cancelamento preservam o status até confirmação; a UI consulta periodicamente sem permitir ações incompatíveis com os estados das duas APIs.

O acompanhamento usa um cliente HTTP separado, sem JWT ou logout administrativo em 401. O DTO público vem somente do OS e não tenta recuperar dados pessoais no Execution. Emissão de link é explícita e informa que substitui o anterior. Links privados não iniciam telemetria; requisições públicas são excluídas da instrumentação. Queries são omitidas dos logs Nginx e tokens são filtrados dos erros do Vite.

## Alternativas consideradas

- Kong obrigatório localmente: acrescentaria serviço e configuração sem necessidade de plugins de gateway neste incremento.
- Chamadas diretas do navegador a três portas: exigiria CORS em todas as APIs e diferenças desnecessárias entre local e nuvem.
- BFF com agregação: acrescentaria serviço de negócio e implantação; a composição atual cabe na camada de serviços do frontend.
- Manter endpoints legados como fallback: esconderia falhas de migração e misturaria dados/bancos de ambientes diferentes.

## Consequências e evidências

Compose da raiz inclui frontend. `services/api.ts`, `ordensServico.service.ts` e `ordemServico.mapper.ts` concentram autenticação, roteamento e adaptação. Não há DELETE de ordens no OS, portanto essa ação foi removida. Cadastro é sequencial e preserva IDs confirmados para retomar falhas sem repetir etapas. Timeout após commit continua ambíguo, pois as APIs de cadastro não oferecem chave de idempotência.

Os manifests do frontend passam a requerer os três Services internos configurados no ConfigMap. Isso prepara a entrada na nuvem, mas não implanta OS, Execution, Billing ou suas dependências. O rollout exige provisionar esses serviços antes; a AWS não foi alterada por esta implementação.

A [ADR-022](ADR-022-kubernetes-microsservicos.md) acrescenta os manifestos das APIs e define HTTP interno em 8080, substituindo o contrato preparado de TLS interno em 8443. O frontend não depende mais de Secret de CA privada para esse fluxo.

Ver [guia da migração](../arquitetura/migracao-frontend.md) e [RFC-013](../RFCs/RFC-013-proxy-frontend-microsservicos.md).
