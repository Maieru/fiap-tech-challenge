# Repositórios e CI/CD

OS/frontend, Execution e Billing têm CI e entrega nos próprios repositórios. O CI roda em PR/push/manual; publicação ECR e deploy EKS têm início manual em main e exigem CI aprovado para o SHA exato. Esta configuração não comprova execução remota das Actions ou implantação AWS.

## Automação da aplicação

| Workflow | Responsabilidade |
| --- | --- |
| monorepo-ci.yml (OS) / ci.yml (Execution e Billing) | Testes, cobertura, imagem, Sonar e gate ci-required; inclui testes das ferramentas de entrega. |
| sonarqube.yml | Quality gate e importação de cobertura. |
| publish-image.yml | Manual: valida CI do commit e publica/reutiliza tag imutável sha-<commit> no ECR, registrando digest. No OS, selecionar os ou frontend. |
| deploy-eks.yml | Manual: valida CI, resolve imagem por digest, renderiza cópias YAML, faz dry-run, sincroniza ExternalSecrets e acompanha rollout no EKS. |
| build-frontend-image.yaml | Compatibilidade reutilizável: delega ao publish-image para frontend, com CI aprovado. Não publica latest. O secret ACTION_ROLE_ARN deve ser a nova role publish do OS. |
| publish-os-messaging.yml / publish-messaging.yml | Publica contratos segundo a política NuGet de cada produtor. |
| destroy-expensive-infrastructure.yml | Destruição agendada/manual legada, preservada e não executada nesta implementação. |

No OS, ci-required exige frontend, component, quality e delivery. Execution/Billing exigem verify, quality e delivery. O pipeline de CI não recebe roles de entrega: somente os workflows de publicação/deploy podem solicitar OIDC.

## Preparação e operação

Siga o [guia de entrega](../../scripts/delivery/README.md), incluindo as Actions variables AWS_PUBLISH_ROLE_ARN, AWS_DEPLOY_ROLE_ARN, AWS_REGION, KAFKA_BOOTSTRAP_SERVERS, JWT_ISSUER e JWT_AUDIENCE. Os três últimos são parâmetros públicos; senhas/chave JWT/licença continuam nos Secrets do ambiente.

O Infra declara roles OIDC separadas por operação e repositório, ECRs próprios das APIs e access entries/RBAC dos namespaces. Aplicar bootstrap → aws-resources → kubernetes-configs antes de configurar as Actions. Os novos grupos de deploy não leem Secrets nem administram RBAC ou namespaces; a role OS atende também frontend.

Disponibilizar bancos/usuários exclusivos, Kafka/tópicos, segredos e IAM do operador, Collector/New Relic e metrics-server. Aplicar migrations compatíveis de OS/Billing antes do rollout. As Actions não provisionam essas dependências nem executam migrations.

O operador publica o commit após CI aprovado e depois executa deploy. image_sha vazio seleciona o commit da execução; um SHA anterior permite reentregar imagem já publicada e aprovada. O deploy usa os manifestos do commit atual e registra ambas as referências. Anotações no Pod template renovam configuração/segredos mesmo sem troca da imagem; HPAs mantêm controle de replicas.

As execuções são serializadas por componente e não cancelam deploy ativo. Aplicações de múltiplos recursos não são atômicas; falha não dispara rollback automático. Uma imagem anterior só deve ser usada com schema e manifestos compatíveis. Promoção hml/prod, ambientes isolados e políticas adicionais de aprovação continuam fora desta etapa única de laboratório.

## Responsabilidades e validação

- Aplicações: imagens, manifestos, testes e seus workflows ECR/EKS.
- Infra: rede, EKS, ECR, OIDC/IAM, RBAC, add-ons, Collector, Gateway e infraestrutura serverless.
- Banco: provisionamento dos bancos e segredos/grants conforme responsabilidade; o RDS legado não substitui os bancos exclusivos.
- Chassi: bibliotecas técnicas e contratos; Serverless: código/publicação do authorizer legado.

A revisão de 2026-11-07 identificou e tratou a ausência de ECRs/roles dos novos serviços, o escopo antigo de namespaces da identidade de aplicação e a necessidade de renovação dos pods ao reaplicar configuração. A política IAM do operador para os novos segredos de banco permanece um pré-requisito explícito, conforme o plano dos manifestos.

Validação local: testes Node das ferramentas, actionlint dos workflows, kubeconform dos YAMLs renderizados e Terraform fmt/validate. IAM efetivo, execução GitHub, conectividade, autenticação, links privados, escala e ingestão New Relic precisam de evidências após o deploy.

Decisões: [ADR-023](../ADRs/ADR-023-entrega-servicos-ecr-eks.md), [RFC-017](../RFCs/RFC-017-actions-ecr-eks.md) e [roteiro Kubernetes](../../k8s/README.md).

## Retirada operacional do legado

Os workflows initialize-and-deploy.yml, build-backend-image.yml e deploy-applications.yml do monólito continuam retirados. A versão histórica está indicada no [guia de retirada](../arquitetura/retirada-monolito.md). Os manifests antigos permanecem em [legacy/aws](../../legacy/aws/README.md).

Não foi executada remoção de Kubernetes, Lambda, RDS, imagens remotas, volumes, dados ou segredos. Inventário e política de retenção devem anteceder a retirada operacional. O [estado da migração](../arquitetura/estado-da-migracao.md) distingue esse trabalho da retirada do código já concluída.
