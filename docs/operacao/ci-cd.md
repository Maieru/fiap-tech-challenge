# Repositórios e CI/CD

O código e a automação exclusiva do monólito foram retirados. Este repositório verifica OS e frontend; os outros serviços e a infraestrutura possuem repositórios próprios. A configuração de nuvem do frontend está preparada, mas a implantação dos microsserviços e a troca de tráfego continuam pendentes.

## Automação atual da aplicação

| Workflow | Responsabilidade |
| --- | --- |
| `monorepo-ci.yml` | PR/push/manual: testes frontend, build, lint do escopo migrado, imagem; testes/cobertura/imagem OS; análise Sonar e gate agregado. |
| `sonarqube.yml` | Reutilizável: importa cobertura OS e analisa a solução backend atual, com quality gate. |
| `publish-os-messaging.yml` | Valida e publica contratos OS por tag e política NuGet própria. |
| `build-frontend-image.yaml` | Reutilizável: constrói/publica imagem frontend no ECR quando chamado por um workflow autorizado. Não provisiona os serviços. |
| `destroy-expensive-infrastructure.yml` | Destruição agendada/manual já existente, preservada sem alteração nesta retirada. |

`ci-required` exige `frontend`, `component` e `quality`. O CI não publica automaticamente os serviços nem aplica manifests. Resultados locais e existência dos arquivos não comprovam execução remota aprovada. Consulte o [guia de CI/dependências](monorepo-ci.md).

Os workflows `initialize-and-deploy.yml`, `build-backend-image.yml` e `deploy-applications.yml` foram removidos porque construíam e implantavam o monólito. A versão anterior está no commit de recuperação identificado no [guia de retirada](../arquitetura/retirada-monolito.md). Não há substituição automática desse orquestrador por um deploy incompleto dos microsserviços.

## Responsabilidades entre repositórios

- Aplicação principal: OS, frontend, documentação e seus checks.
- Execution e Billing: imagens, testes e contratos de cada produtor.
- Chassi: biblioteca/adaptador técnico, testes e publicação próprios.
- Infraestrutura: VPC, EKS, add-ons, configurações, Gateway e infraestrutura serverless.
- Banco: RDS legado e credenciais existentes; novos bancos exigem provisionamento por responsabilidade.
- Serverless: código/publicação do authorizer legado, cuja retirada é uma etapa operacional separada.

A configuração AWS usa OIDC nos workflows existentes. Segredos, estados, proteção de branches e execução remota não foram alterados por remover arquivos deste checkout.

## Próximo fluxo de deploy

O novo processo deve versionar imagens e coordenar provisionamento de OS/Execution/Billing, bancos exclusivos, Kafka, segredos e observabilidade antes de publicar o frontend e os caminhos de Gateway/ALB. Os [manifests frontend](../../k8s/README.md) não criam essas dependências. A topologia de entrada segue a [ADR-020](../ADRs/ADR-020-frontend-microsservicos-mesma-origem.md); a retirada do código segue a [ADR-021](../ADRs/ADR-021-retirada-codigo-monolito.md).

O deploy deve registrar versões, validar saúde/autenticação/links privados e definir recuperação antes de migrar o tráfego. Promoção entre homologação e produção, isolamento de estados/recursos/segredos e validação remota continuam sendo trabalho de infraestrutura/entrega; o CI atual não equivale a essa promoção.

## Retirada operacional do legado

Os manifests antigos foram arquivados em [legacy/aws](../../legacy/aws/README.md). Não foi executada remoção de Kubernetes, Lambda, RDS, imagens remotas, volumes, dados ou segredos. A política de retenção e o inventário dos recursos exclusivos/compartilhados precisam anteceder a retirada operacional. O workflow de destruição agendada foi preservado e não foi executado nesta sessão.

O [estado da migração](../arquitetura/estado-da-migracao.md) distingue esses trabalhos da retirada já concluída do código e da automação exclusiva do monólito.
