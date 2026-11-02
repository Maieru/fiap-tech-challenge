# RFC-016 — Correção dos apontamentos Sonar

- **Status:** Aceita
- **Data:** 2026-11-02
- **Escopo:** frontend, OS, imagem, Kubernetes e verificação por CI

## Contexto

A consulta pública ao [projeto SonarQube Cloud](https://sonarcloud.io/summary/overall?id=Maieru_fiap-tech-challenge&branch=main) encontrou 20 issues não resolvidas e nenhum hotspot pendente de revisão. A última análise disponível era de 2026-10-31, revisão `ac6223e5b33463c18d7a6c26356eda657db98095`, anterior à retirada local do monólito e à reorganização do OS. O quality gate consultado retornou `NONE`, sem condições; isso não equivale a aprovação das mudanças atuais.

## Decisão

Corrigir as causas no código ativo e validar o comportamento. O catálogo abaixo relaciona todos os 20 registros consultados; a classificação reflete o checkout local, não o estado já publicado no Sonar.

| Regra | Quantidade | Tratamento |
| --- | ---: | --- |
| `kubernetes:S5332` | 3 | Dois registros no ConfigMap frontend: upstreams de nuvem passam a HTTPS com CA e hostname validados. O terceiro pertence ao backend arquivado. |
| `kubernetes:S6865` | 2 | Frontend desativa automount do token de ServiceAccount; backend antigo está arquivado. |
| `kubernetes:S6897` | 2 | Frontend define request/limit de armazenamento efêmero; backend antigo está arquivado. |
| `githubactions:S7637` | 5 | Duas Actions AWS do frontend fixadas por SHA completo. Os outros três registros pertencem aos workflows removidos do monólito. As demais Actions de build/análise/publicação também foram fixadas. |
| `githubactions:S8233` | 1 | Workflow antigo removido; o mesmo padrão ativo no publicador frontend foi corrigido com permissão OIDC no job. |
| `docker:S8543` | 1 | A migração anterior já substituiu `npm install` por `npm ci`, preservando o lockfile. |
| `docker:S6505` | 1 | Instalação Docker e CI usa `--ignore-scripts`, com build limpo validado. |
| `docker:S6471` | 1 | Runtime passa a Nginx unprivileged, UID/GID 101, porta 8080. Compose e Deployment acompanham a porta; Service continua em 80. |
| `typescript:S6850` | 1 | CardTitle renderiza explicitamente seus children dentro do heading. |
| `typescript:S6848` | 1 | Backdrop mobile passa a botão nativo com nome acessível. Controles recebem labels, estado expandido e vínculo com o menu; menu fechado fica inert. |
| `csharpsquid:S6781` | 1 | Fallback de chave JWT pertencia ao monólito removido. OS exige configuração de chave, sem fallback equivalente. |
| `csharpsquid:S2583` | 1 | Arquivo antigo removido; cópia do Result no OS agora rejeita Error nulo antes de ler Code. Testes verificam JSON com erro nulo/ausente e round-trip válido. |

Preservar os três apontamentos dos manifests arquivados como referência histórica: não aplicar esses manifests nem alterar seu conteúdo para aparentar segurança. Excluir `legacy/**` conforme a retirada já documentada na ADR-021. Manter análise de outras linguagens no scanner .NET (`scanAll=true`) para a infraestrutura ativa.

### Revisão do escopo solicitada em 2026-11-02

Retirar `src/FIAP.TechChallenge.Fase1.Frontend/**` do Sonar por solicitação explícita da equipe, usando `sonar.exclusions` e `sonar.test.exclusions`. A exclusão cobre código, testes, Dockerfile e Nginx dessa pasta; os manifests Kubernetes e workflows permanecem analisáveis. As correções anteriores são preservadas e o job frontend continua exigindo testes, lint, build e verificação da imagem no CI. A consequência é que o Sonar deixa de apresentar métricas e findings dos arquivos excluídos após a próxima análise dessa configuração. A tabela acima registra o tratamento das 20 issues consultadas antes desta revisão.

## Contrato de transporte e execução

Manter HTTP no Compose local e HTTPS para upstreams de nuvem. A árvore Kubernetes é um contrato de implantação futura: os serviços de negócio ainda não são provisionados por este repositório. URLs HTTPS exigem terminação TLS real e certificados com SAN correspondente; a mudança de URL não habilita TLS nos backends. O [guia Kubernetes](../../k8s/README.md) define portas, Secret obrigatório de CA e pré-requisitos do Collector.

Nginx valida certificados usando `UPSTREAM_CA_CERTIFICATE`, com raízes do sistema por padrão e bundle privado montado no Kubernetes. Verificação e SNI continuam habilitados. Não há bypass de validação TLS.

O pod frontend não usa credenciais Kubernetes. Executa sem root, sem elevação de privilégios, sem capabilities adicionais e com seccomp RuntimeDefault. CPU, memória e armazenamento efêmero são limitados.

## Alternativas consideradas

- Apenas encerrar issues remotamente: não corrige a causa nem valida o checkout.
- Excluir também toda a infraestrutura: descartado por retirar análise de arquivos ativos além do frontend solicitado. Para o frontend, a equipe optou pela exclusão descrita na revisão de escopo, mantendo as verificações próprias do CI.
- Aceitar HTTP interno como solução definitiva na nuvem: mantém tokens e dados em trânsito sem criptografia entre Nginx e upstream.
- Exigir gateway/certificados no desenvolvimento local: adiciona dependências desnecessárias ao fluxo local já existente.
- Executar Nginx como root para manter a porta 80 interna: desnecessário; Service e Compose podem publicar portas estáveis sobre 8080.

## Verificação e limites

Build Docker limpo aprovado com instalação sem lifecycle scripts. Testes frontend (9), build e lint dos arquivos alterados aprovados. Domínio OS: 141 testes aprovados, incluindo cinco novos cenários do Result.

`scripts/Test-FrontendSecurity.ps1` cria uma CA temporária e quatro containers isolados. Verifica UID 101, sintaxe Nginx, três prefixos API, Authorization, rota OTLP, CA desconhecida e hostname divergente. O teste remove seus recursos e não importa certificados no host. O CI executa o mesmo teste após construir a imagem.

Smoke HTTP completo aprovado na stack `fiap-frontend-verification`, com PostgreSQL, MongoDB e Kafka próprios: autenticação, três APIs via Nginx, diagnóstico, orçamento, rotação do link privado, pagamento, conclusão, entrega e cancelamento idempotente. A stack e seus volumes sintéticos foram removidos após a validação.

Inspeção no navegador em desktop e mobile: headings com conteúdo, abertura e fechamento do menu pelo teclado, estado expandido/colapsado e ausência dos links do menu fechado na árvore de acessibilidade. Capturas locais ficam em `artifacts/sonar-remediation` (ignorado). YAML de workflows/manifests e invariantes do Deployment/ConfigMap foram verificados estaticamente; nenhum recurso Kubernetes foi aplicado.

A atualização do dashboard depende da análise CI do commit que contiver estas mudanças. Nenhuma análise foi publicada por esta correção local, nenhum issue foi marcado como aceito/falso positivo e nenhum deploy AWS foi executado. A configuração TLS de nuvem depende do provisionamento descrito acima.

## Referências

- [Imagem oficial Nginx unprivileged](https://github.com/nginx/docker-nginx-unprivileged).
- [Escopo multilíngue do scanner .NET](https://docs.sonarsource.com/sonarqube-server/2025.5/analyzing-source-code/scanners/dotnet/configuring).
- [Preservação do legado](../arquitetura/retirada-monolito.md).
