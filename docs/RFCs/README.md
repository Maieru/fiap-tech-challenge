# Request for Comments (RFCs)

As RFCs documentam escolhas técnicas relevantes já aceitas, suas alternativas e a decisão da equipe. Discussões ainda abertas permanecem em issues ou pull requests. Use o [template](TEMPLATE.md) depois da aprovação.

## Catálogo

| RFC | Status | Decisão |
| --- | --- | --- |
| [RFC-001](RFC-001-escolha-da-nuvem-aws.md) | Aceita | Adotar AWS em `us-east-1` como nuvem principal. |
| [RFC-002](RFC-002-postgresql-no-amazon-rds.md) | Aceita | Usar PostgreSQL com EF Core/Npgsql e Amazon RDS. |
| [RFC-003](RFC-003-autenticacao-jwt-e-bcrypt.md) | Aceita | Autenticar credenciais locais com BCrypt e access token JWT. |
| [RFC-004](RFC-004-containers-no-amazon-eks.md) | Aceita | Executar os containers de aplicação no Amazon EKS. |
| [RFC-005](RFC-005-terraform-e-github-actions-oidc.md) | Aceita | Provisionar com Terraform e operar pipelines AWS por OIDC. |

## Regra para RFCs aceitas

As RFCs `001` a `005` formalizam retrospectivamente escolhas já presentes no código e na infraestrutura. Novas RFCs entram neste catálogo somente depois da aprovação da decisão.

As alternativas são avaliações retrospectivas, não atas de discussões históricas. A data original identifica o registro; a última revisão indica a atualização do texto e das evidências. O template usa **Decisão**, pois o catálogo contém escolhas aceitas, não recomendações pendentes.
