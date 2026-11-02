# Verificação local do frontend migrado

Data: 2026-11-02. Esta verificação não representa execução do CI remoto ou deploy AWS.

## Resultados

| Verificação | Resultado |
| --- | --- |
| `npm test` | 9 testes aprovados: endpoints locais/gateway/stage, privacidade de logs, DTOs, entrega preservada, projeção ausente, JWT compartilhado, expiração, isolamento de 401 e rotas dos comandos. |
| `npm run build` | TypeScript e Vite aprovados. Permanecem avisos de tamanho do bundle e base Browserslist antiga. |
| Imagem Linux | Build com Node 22 Alpine, `npm ci` e Nginx aprovado. Lockfile sincronizado em ambiente Linux para incluir dependência opcional ausente no lock anterior. |
| `nginx -t` | Aprovado na imagem final. |
| ESLint do código afetado | Serviços, tipos de ordem, páginas de ordens, observabilidade e configuração Vite aprovados. |
| ESLint global | Falhas preexistentes em `badge.tsx`, `button.tsx`, `AuthContext.tsx` e outras listagens. Não foram desativadas regras para ocultar os erros. |
| YAML/Compose | CI e manifests Kubernetes parseados; Compose isolado versionado validado com `docker compose config --quiet`. |
| Vite sem gateway | Login OS e consultas em OS, Execution e Billing retornaram 200 através dos três prefixos. |
| Nginx sem gateway | Smoke real aprovado contra três APIs, Kafka e bancos isolados. |
| Ambiente final 5173 | Página responde 200; consultas sem JWT nos três prefixos respondem 401, confirmando proxy e autenticação. APIs de desenvolvimento permaneceram em execução. |

## Smoke real e isolamento

O projeto de teste `fiap-frontend-verification` usou rede e volumes exclusivos, frontend na porta 15173 e APIs em 18081–18083. Dados sintéticos foram criados somente nessa stack. O script `tests/smoke.mjs` executou:

1. Cadastro/login administrativo no OS e compartilhamento do JWT nas três APIs.
2. Criação de cliente/veículo, serviço e peça com estoque.
3. Abertura da ordem, criação assíncrona da execução, diagnóstico e orçamento com snapshots.
4. Emissão e rotação de link; token anterior rejeitado com 401; orçamento público de R$ 150 sem dados pessoais.
5. Aprovação pública com 202, pagamento mockado no Billing, início confirmado e cobrança consultável.
6. Conclusão do item com IDs da ordem/item, finalização e entrega; OS em 6, Execution em 5.
7. Segunda ordem cancelada pelo link; repetição idempotente e quantidade de estoque conferida para evitar dupla devolução.
8. Cabeçalhos `no-store` e `no-referrer` na página pública.

Logs do OS e Nginx foram inspecionados sem imprimir tokens; não continham query `token=`. A stack isolada foi removida após a verificação, preservando o ambiente de desenvolvimento.

## Navegador

Login, dashboard, lista, detalhes e acompanhamento foram abertos contra as APIs reais. A primeira inspeção encontrou consulta de 300 itens no Execution, cujo limite é 100; as consultas dos seletores foram corrigidas. A segunda confirmou detalhes com nomes, itens, cobrança e datas.

O formulário foi exercitado com cadastro novo e placa inválida: cliente permaneceu selecionado após o erro; corrigir a placa permitiu criar veículo e ordem sem recriar o cliente. O acompanhamento exibiu orçamento e aprovação com estado pendente, convergindo para Em execução após confirmação. Foram inspecionados desktop e viewport móvel de 390 px; tabelas têm rolagem horizontal no celular.

Durante essa sessão, o OS isolado encerrou após `KafkaException: Unknown broker error` no commit de offset do consumidor. A UI mostrou indisponibilidade, e o serviço de teste foi reiniciado manualmente; a consulta voltou e mostrou a confirmação. Isso evidencia a limitação existente dos consumidores sem reinício automático; não foi alterada nesta migração. O smoke completo anterior havia passado. Não se atribui à UI a recuperação do processo backend.

Capturas locais ficam em `artifacts/frontend/ordem-cadastro-desktop.jpg` e `artifacts/frontend/acompanhamento-{desktop,mobile}.jpg` (ignoradas pelo Git).

## Limites

Gateway de pagamento permanece mockado. A configuração de gateway público, CORS caso necessário, Services Kubernetes e roteamento de nuvem precisam ser verificados após provisionamento real. Esta sessão não executou CI remoto, Sonar, publicação, deploy ou migração de dados/tráfego. Os testes backend existentes não foram repetidos, pois a mudança não alterou os contratos ou a implementação dos serviços.
