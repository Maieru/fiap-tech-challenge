# Kubernetes — OS e frontend

A pasta os mantém a API OS em fiap-os; frontend mantém Nginx e Ingresses em fiap-frontend. Não há workload ativo do monólito; fontes históricas permanecem em legacy/aws.

manage-platform.yml publica e entrega usando o mesmo SHA, CI aprovado e imagem por digest. Namespace, SecretStore/role, RBAC e CA AWS pertencem ao Infra. OS recebe somente sua credencial PostgreSQL e JWT; monta /certs/global-bundle.pem para validar TLS do RDS.

A entrega sincroniza ExternalSecrets, executa Job --migrate-only antes do OS e acompanha rollout. O Job usa somente a conexão do banco e encerra sem HTTP/JWT/consumidores. Database__ApplyMigrations fica false nos pods normais. Placeholders dos YAMLs são substituídos em cópias temporárias pelas ferramentas de entrega.

Frontend é entregue após as três APIs e cria o ALB interno compartilhado. API Gateway é aplicado depois, recebendo o listener por input. Nginx roteia os três prefixos /api/{os,execution,billing}/v1; APIs validam JWT e OS realiza login. HPA usa metrics-server; OTLP segue ao Collector compartilhado.

Bancos, Kafka/tópicos, segredos e configurações são criados pela action central. Consultar o [guia operacional](../docs/operacao/plataforma-aws.md) antes de ativar o fluxo e seu agendamento destrutivo.
