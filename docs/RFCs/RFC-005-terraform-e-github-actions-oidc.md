# RFC-005 — Terraform e GitHub Actions com OIDC

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** infraestrutura como código e entrega contínua
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

Rede, cluster, banco, add-ons, gateway e autorizador precisam de provisionamento reproduzível. As pipelines devem usar credenciais AWS temporárias.

## Decisão

Usar Terraform e GitHub Actions com OIDC:

- Terraform declara recursos AWS, RDS, add-ons e configurações Kubernetes compartilhadas.
- Manifests da aplicação são aplicados por `kubectl`; o controller cria o ALB a partir dos Ingresses.
- O estado `serverless` cria a infraestrutura da Lambda; o repositório serverless publica seu código.
- Estados usam chaves próprias no S3, criptografia e locking por lockfile.
- Workflows reutilizáveis executam `fmt`, `validate`, `plan` e aplicam o mesmo plano gerado.
- GitHub Actions assume roles por `sts:AssumeRoleWithWebIdentity`.
- A aplicação orquestra a ordem descrita na ADR-008.

O apply é automático após o plan. Não há etapa de aprovação humana declarada nos workflows.

## Alternativas consideradas

- **CloudFormation/CDK ou Pulumi:** exigiriam migrar a automação AWS, Kubernetes e Helm existente.
- **Provisionamento manual:** perderia repetibilidade e revisão por plano.
- **Access keys persistentes:** exigiriam gestão e rotação de credenciais duradouras.

## Consequências

- Planos tornam alterações verificáveis, e OIDC evita access keys AWS no GitHub.
- Estados e workflows remotos exigem coordenação entre repositórios.
- Referências `@main` podem mudar sem versão imutável.
- Roles de infraestrutura têm permissões amplas; ausência de aprovação manual permite apply automático.
- O bucket tem `force_destroy = true` e não declara versionamento.
- State e artefatos de plano podem conter segredos; devem ter acesso restrito, conforme a ADR-007.

Reconsiderar por requisitos de aprovação, segregação de funções, GitOps ou mudança da plataforma de infraestrutura.

## Evidências e relações

- [CI/CD atual e melhorias de homologação/produção](../operacao/ci-cd.md)
- [Orquestrador de implantação](../../.github/workflows/initialize-and-deploy.yml)
- [Guia da infraestrutura](https://github.com/Maieru/fiap-tech-challenge-infra/tree/main/infra)
- [Workflow Terraform reutilizável](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/.github/workflows/terraform-stage.yml)
- [Configuração OIDC](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/bootstrap/github-auth.tf)
- [ADR-008 — Estados Terraform e repositórios](../ADRs/ADR-008-estados-terraform-e-repositorios.md)
