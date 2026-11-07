# CI/CD e plataforma AWS

O CI permanece independente por repositório, com os gates de qualidade existentes. A entrega e a destruição agora são centralizadas no workflow manage-platform.yml do repositório principal, com workflows reutilizáveis em Infra, DB, OS, Execution e Billing.

Consulte o [guia operacional completo](plataforma-aws.md) para transição, bootstrap, variables/secrets, ordem de implantação, destruição diária, retomada e limites de segurança/durabilidade.

Publicação e deploy usam o mesmo SHA de origem, exigem CI aprovado no repositório do serviço e resolvem imagem por digest. O principal serializa todo o ciclo; as antigas entradas manuais de publicação/deploy e os orquestradores legados foram retirados. Contratos NuGet continuam com seus próprios workflows, sem provisionar recursos AWS.

Decisões: [ADR-024](../ADRs/ADR-024-plataforma-microsservicos-aws.md) e [RFC-018](../RFCs/RFC-018-orquestracao-plataforma-aws.md). A existência das configurações não comprova execução remota: registre evidências após o primeiro ciclo AWS.
