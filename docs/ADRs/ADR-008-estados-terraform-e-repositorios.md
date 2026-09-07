# ADR-008 — Separação de repositórios e estados Terraform

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** organização da plataforma e infraestrutura como código
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

Aplicação, plataforma, banco e autorizador têm responsabilidades distintas. Seus recursos precisam ser criados e removidos na ordem das dependências.

## Decisão

| Repositório | Responsabilidade |
| --- | --- |
| `fiap-tech-challenge` | Aplicação, imagens, manifests e orquestração |
| `fiap-tech-challenge-infra` | Bootstrap, rede/EKS/ECR, add-ons, observabilidade, gateway e infraestrutura serverless |
| `fiap-tech-challenge-db` | RDS e segredo da conexão |
| `fiap-tech-challenge-serverless` | Código, testes e deploy da Lambda Authorizer |

A criação segue:

```text
bootstrap → aws-resources → database → kubernetes-addons
→ kubernetes-configs → aplicações/Ingresses → api-gateway
→ serverless → deploy do código da Lambda
```

Aplicações/Ingresses e deploy do código da Lambda são etapas de entrega, não estados Terraform. Os demais estágios têm chaves próprias no mesmo bucket S3, com criptografia e lockfile. Dependências são consumidas por outputs de `terraform_remote_state`.

## Alternativas consideradas

- **Repositório e state únicos:** simplificariam coordenação, mas ampliariam planos e impacto de falhas.
- **State por recurso:** criaria dependências demais para operar.
- **Workspaces por componente:** confundiriam separação de ambientes com responsabilidades.

## Consequências

- Planos menores facilitam revisão, mas outputs e workflows acoplam os repositórios.
- O bootstrap inicial exige criar o bucket antes de migrar seu próprio estado.
- Consumir outputs por remote state requer acesso ao estado de origem; não é isolamento de segredos.
- Chaves diferentes separam locks, mas não isolam armazenamento ou IAM: o bucket tem `force_destroy = true`, sem versionamento declarado, e as roles de infraestrutura são amplas.
- Destruição remove serverless antes do gateway, Ingresses antes do controller e dependentes antes do banco/cluster.
- Mudanças de outputs devem ser coordenadas com seus consumidores.

Reconsiderar quando a coordenação entre repositórios impedir mudanças seguras ou surgirem ambientes com ciclos independentes.

## Evidências e relações

- [Orquestrador principal](../../.github/workflows/initialize-and-deploy.yml)
- [Guia e ordem dos estados](https://github.com/Maieru/fiap-tech-challenge-infra/tree/main/infra)
- [Backend do estado de banco](https://github.com/Maieru/fiap-tech-challenge-db/blob/main/infra/database/backend.tf)
- [Workflows do banco](https://github.com/Maieru/fiap-tech-challenge-db/tree/main/.github/workflows)
- [RFC-005 — Terraform e GitHub Actions com OIDC](../RFCs/RFC-005-terraform-e-github-actions-oidc.md)
- [ADR-010 — Ambiente efêmero orientado a custo](ADR-010-ambiente-efemero-orientado-a-custo.md)
- [Criação de gateway e serverless](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/.github/workflows/apply-edge-infrastructure.yml)
