# Roteiro de migração do frontend

Atualizado em 2026-11-02. Este documento descreve trabalho pendente, baseado nos contratos existentes. O frontend continua no legado; a migração anteriormente cancelada não foi executada. A atualização da documentação não altera telas, proxy ou chamadas.

OS, Billing e Execution já atendem ao ciclo de backend até a entrega. Os nove cenários distribuídos registrados validam HTTP/Kafka entre os serviços, mas não substituem os testes do navegador. Veja o [estado consolidado](estado-da-migracao.md) para publicação, CI e deploy.

## Distribuição das chamadas

| Responsabilidade do frontend | Serviço e prefixo |
| --- | --- |
| Cadastro/login administrativo, clientes e veículos | OS: `/api/os/v1` |
| Abertura/lista/consulta de ordens, histórico, aprovação/cancelamento e entrega | OS: `/api/os/v1/ordens` |
| Emissão do link e acompanhamento/decisões do cliente | OS: `/api/os/v1/ordens/{id}/acesso-cliente` e `/api/os/v1/ordens/acompanhamento/{id}` |
| Catálogo de serviços e peças/insumos, estoque | Execution: `/api/execution/v1/servicos` e `/api/execution/v1/pecas-insumos` |
| Diagnóstico, itens da execução, conclusão e finalização | Execution: `/api/execution/v1/ordens` |
| Consultar cobrança da ordem | Billing: `/api/billing/v1/ordens/{id}/cobranca` |

Não existe criação manual de cobrança pela UI: a aprovação solicita pagamento por Kafka. Os detalhes administrativos podem combinar consultas HTTP ao OS, Execution e Billing; as APIs não devem acessar bancos alheios para produzir uma resposta equivalente ao legado. O status operacional e a entrega exibidos na lista devem seguir o OS, incluindo durante divergências transitórias da projeção.

## Diferenças encontradas no frontend atual

| Código ou comportamento atual | Adaptação necessária |
| --- | --- |
| `services/api.ts` tem uma única base e todas as chamadas usam as rotas legadas | Separar clientes/endpoints por serviço, preservando o contrato comum de erros e a sessão administrativa. |
| Detalhes esperam uma ordem única com arrays e totais do legado | Mapear a ordem OS e a resposta Execution `{ execucao, valorTotal }`. Mostrar orçamento/pagamento a partir das fontes correspondentes; não inventar itens, valores ou total zero quando uma projeção ainda não chegou. |
| Ações assumem que aprovação muda imediatamente para execução | Tratar 202, decisão registrada e etapa da Saga; consultar novamente até as confirmações. Distinguir espera por pagamento, autorização e compensação. |
| O link é obtido de `detalhes.token` e depende de CPF | Emitir por `POST /ordens/{id}/acesso-cliente` com JWT. Montar a URL da página do frontend com o token retornado; `acompanhamentoUrl` do OS é uma rota relativa da API que retorna JSON. Nova emissão invalida o link anterior e deve ser apresentada como troca do link. |
| Acompanhamento espera `id`, nome do cliente e dados do veículo | O novo DTO usa `ordemServicoId`, status/datas, orçamento e etapa do pagamento; não retorna nome, CPF, contatos ou dados do veículo. Ajustar a tela ao contrato mínimo. |
| Aprovação/cancelamento administrativos usam o token legado da ordem | Usar JWT nas rotas administrativas. Nas rotas de acompanhamento, exigir o token privado e usar o DTO próprio da decisão do cliente. |
| A página pública atual apenas consulta e atualiza o acompanhamento | Adicionar aprovação e cancelamento pelo link conforme os estados permitidos, com confirmação de decisão e tratamento de 202/409. |
| Conclusão de serviço usa somente o ID do item na rota | Execution exige `PUT /ordens/{id}/servicos/{itemId}/concluir`; conservar os dois identificadores. |
| Formulário cria cliente, veículo e ordem em uma chamada `/com-cliente-veiculo` | Não há endpoint conjunto no OS. Adaptar para etapas e tratar falhas parciais sem duplicar cadastros; um novo caso de uso conjunto exigiria implementação e contrato explícitos. |
| Lista/detalhe oferecem exclusão da ordem por DELETE | Não há DELETE de ordens no OS. Retirar essa ação da integração atual; não trocar exclusão por cancelamento silenciosamente. Cancelamento tem regras próprias e preserva o histórico. |
| Qualquer 401 no Axios apaga a sessão administrativa | Separar o cliente público: token privado inválido deve mostrar erro de acesso sem encerrar a sessão administrativa do operador. |

As divergências foram verificadas em `src/FIAP.TechChallenge.Fase1.Frontend/src/services`, `src/types/ordemServico.ts` e nas páginas de formulário, lista, detalhes e acompanhamento. Não são endpoints adicionais já entregues.

## Autenticação e acesso do cliente

O login é `POST /api/os/v1/usuarios/login`; a resposta mantém `token`, `tipoToken` e `expiresInSeconds`. O cadastro administrativo continua anônimo por decisão do projeto. OS, Execution e Billing validam o mesmo JWT, com issuer/audience/chave iguais; não são necessários três logins. Novos bancos não importam usuários legados. Veja [autenticação](autenticacao-os.md).

O acompanhamento usa token aleatório por ordem, separado do JWT. A validação ocorre no OS para consulta, aprovação e cancelamento. O token só é retornado na emissão, não fica disponível na consulta administrativa e vale até a troca. Links antigos não são convertidos. A UI não deve emitir um novo link automaticamente ao consultar detalhes, pois isso revogaria o anterior.

O frontend atualmente registra `window.location.href` em `observability/telemetry.ts` e possui instrumentação automática HTTP. Antes de usar o link novo, remover o token de URLs registradas em logs, traces, analytics e erros; configurar também o access log do proxy para omitir a query. As respostas do OS usam `no-store` e `no-referrer`; a página de acompanhamento e sua telemetria precisam preservar esse tratamento do segredo.

## Proxy e execução local

A stack integrada publica OS 8081, Execution 8082 e Billing 8083. O frontend ainda não faz parte dela. O Nginx atual encaminha `/api/` a um único `API_UPSTREAM`; o Vite só possui proxy de OTLP, e as três APIs não configuram CORS para o navegador acessar suas portas diretamente.

O roteiro recomendado é expor os três prefixos por um proxy na mesma origem do frontend, tanto no desenvolvimento quanto no container. Isso exige ajustar Vite/Nginx e Compose; essa configuração ainda não existe. Não introduz um serviço de negócio ou BFF. Se houver acesso por origens diferentes, configurar CORS explicitamente nas APIs antes de testar no navegador.

A infraestrutura API Gateway/ALB/Lambda continua legada. Adaptar o proxy local não migra as rotas AWS nem autoriza o authorizer a consultar os bancos dos novos serviços.

## Ordem de implementação

1. Confirmar os pacotes e builds limpos; preparar proxy e configuração dos três serviços, sem depender do cache de candidatos locais no CI.
2. Adaptar clientes HTTP, tipos, login e cadastros. Confirmar que um JWT do OS funciona nas três APIs.
3. Adaptar abertura/lista/detalhe, juntando os dados necessários e tratando a execução ainda não projetada após a abertura.
4. Adaptar diagnóstico, orçamento, catálogo, estoque e conclusão dos serviços no Execution.
5. Integrar aprovação e estados do pagamento/compensação, leitura da cobrança e entrega no OS. Atualizar por consulta periódica controlada ou ação de atualizar; não exigir WebSocket inexistente.
6. Adaptar emissão/rotação do link e tela do cliente, incluindo decisões e remoção do token da telemetria.
7. Validar no navegador e no container do frontend com os três serviços. Só depois preparar migração de tráfego e desativação do legado.

## Critérios de aceite da migração

- Cadastro/login pelo OS; expiração do JWT e 401 administrativo tratados. Link inválido não apaga a sessão do operador.
- Clientes, veículos, catálogo e estoque operam nas respectivas APIs, sem chamadas residuais às rotas legadas.
- Abertura e diagnóstico chegam aos serviços sem tratar o atraso de Kafka como recurso permanentemente inexistente.
- Aprovação mostra espera por pagamento/autorização. Com Billing parado, não libera visualmente a execução; ao voltar, a tela converge para o status confirmado.
- Finalização e entrega ficam visíveis; OS `Entregue` e Execution `Finalizada` são apresentados sem contradição ao usuário.
- Link válido consulta orçamento e permite decisões; link trocado ou de outra ordem é rejeitado. Nenhum token é registrado em telemetria ou logs do proxy.
- Cancelamento antes da aprovação devolve estoque uma única vez. Recusa e compensação usam o gateway substituto dos testes, sem adicionar endpoints de simulação em produção.
- Cadastro em etapas permite retomar falhas parciais; exclusão de ordens não chama endpoint inexistente.
- Testes de build e navegação do frontend passam; os cenários distribuídos e os checks dos serviços continuam aprovados. Mercado Pago real e deploy AWS têm validação própria posterior.

Referências: [estado da migração](estado-da-migracao.md), [Saga](saga-pagamento.md), [acompanhamento e entrega](acompanhamento-e-entrega.md), [CI/dependências](../operacao/monorepo-ci.md) e [evidências](../operacao/validacao-ponta-a-ponta.md).
