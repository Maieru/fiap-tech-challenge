# ADR-007 — Gestão centralizada de segredos

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** segurança e configuração
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

API e observabilidade precisam receber segredos sem incluí-los em manifests ou imagens.

## Decisão

Usar `GitHub Secrets → Terraform → Secrets Manager → External Secrets → Kubernetes Secret`.

- Conexão do banco e chave JWT são segredos separados, sincronizados a cada hora no namespace do backend e consumidos por `envFrom`.
- A licença New Relic segue o mesmo mecanismo no namespace de observabilidade.
- External Secrets usa EKS Pod Identity, com acesso aos ARNs necessários.
- A Lambda Authorizer lê o segredo do banco diretamente do Secrets Manager por sua role e endpoint VPC; não usa External Secrets.
- ConfigMaps contêm apenas configuração não sensível. Pipelines acessam a AWS por OIDC.

## Alternativas consideradas

- **Segredos manuais ou injetados no deploy:** acoplariam distribuição e rotação às pipelines.
- **Sealed Secrets ou CSI Driver:** exigiriam outro mecanismo de gestão e consumo.

## Consequências

- Segredos remotos ficam centralizados, com autorização IAM.
- Atualizar o Kubernetes Secret não reinicia os pods; valores em variáveis de ambiente exigem restart.
- A rotação JWT precisa coordenar pods e o efeito sobre tokens existentes.
- State e plano Terraform contêm dados sensíveis; o plano é retido por um dia. Criptografia não substitui acesso restrito e auditoria.
- Roles atuais têm acesso S3 amplo; segredos usam `recovery_window_in_days = 0`.
- Os arquivos `appsettings` ainda incluem chaves JWT de fallback no artefato. Essa dívida exige remoção do fallback e falha de inicialização remota sem segredo válido.
- Kubernetes Secrets também dependem de RBAC e proteção do armazenamento do cluster.

Reconsiderar quando houver necessidade de rotação sem restart ou outro modelo de gestão de chaves.

## Evidências e relações

- [ExternalSecrets da aplicação](../../k8s/backend/secrets.yaml)
- [Consumo no deployment](../../k8s/backend/deployment.yaml)
- [ConfigMap não sensível](../../k8s/backend/config-maps.yaml)
- [External Secrets e Pod Identity](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/kubernetes-addons/external-secrets.tf)
- [Segredo do banco](https://github.com/Maieru/fiap-tech-challenge-db/blob/main/infra/database/secret-manager.tf)
- [RFC-003 — Autenticação JWT e BCrypt](../RFCs/RFC-003-autenticacao-jwt-e-bcrypt.md)
- [RFC-005 — Terraform e OIDC](../RFCs/RFC-005-terraform-e-github-actions-oidc.md)
- [Segredo New Relic](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/kubernetes-addons/newrelic.tf)
- [Acesso da Lambda ao Secrets Manager](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/serverless/networking.tf)
