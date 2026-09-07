# RFC-001 — Adoção da AWS como provedor de nuvem

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** infraestrutura e operação
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

O ambiente remoto precisa integrar containers, banco, imagens, segredos, entrada HTTP e automação. A implementação já usa AWS; esta RFC formaliza essa escolha.

## Decisão

Adotar AWS em `us-east-1` para o laboratório remoto:

- EKS e ECR para containers; RDS PostgreSQL para dados.
- API Gateway, VPC Link e ALB para entrada; Lambda para o autorizador de ordens.
- Secrets Manager para segredos, IAM/OIDC para identidade e S3 para estados Terraform.
- CloudWatch para logs do gateway. A telemetria da aplicação é enviada ao New Relic.

O ambiente local usa Docker Compose. A aplicação e o banco podem executar sem AWS; exportar telemetria ao New Relic depende da configuração desse serviço.

## Alternativas consideradas

- **Azure ou Google Cloud:** exigiriam substituir rede, identidade, módulos e pipelines já implementados, sem benefício demonstrado no escopo.
- **Hospedagem própria ou multicloud:** aumentariam a operação e a coordenação entre componentes.

## Consequências

- Serviços compartilham rede, identidade e provisionamento por código.
- A infraestrutura fica acoplada à AWS, com região e alguns identificadores fixos.
- EKS, RDS e entrada HTTP geram custos mesmo com pouco uso.
- O perfil atual é descartável, conforme a ADR-010; a escolha do provedor não garante durabilidade ou alta disponibilidade.

Reconsiderar por custo, residência de dados, disponibilidade ou necessidade comprovada de outro provedor.

## Evidências e relações

- [Visão de arquitetura](../arquitetura/README.md)
- [README principal](../../README.md)
- [Infraestrutura AWS](https://github.com/Maieru/fiap-tech-challenge-infra/tree/main/infra)
- [RFC-004 — Containers no Amazon EKS](RFC-004-containers-no-amazon-eks.md)
- [RFC-005 — Terraform e GitHub Actions com OIDC](RFC-005-terraform-e-github-actions-oidc.md)
- [ADR-005 — Topologia de entrada na AWS](../ADRs/ADR-005-topologia-de-entrada-na-aws.md)
