# Kubernetes — OS e frontend

A pasta os mantém a API OS em fiap-os; frontend mantém Nginx e Ingresses em fiap-frontend.

manage-platform.yml publica e entrega usando o mesmo SHA, CI aprovado e imagem por digest. Namespace, SecretStore/role, RBAC e CA AWS pertencem ao Infra. OS recebe somente sua credencial PostgreSQL e JWT; monta /certs/global-bundle.pem para validar TLS do RDS.

A entrega sincroniza ExternalSecrets, executa Job --migrate-only antes do OS e acompanha rollout. O Job usa somente a conexão do banco e encerra sem HTTP/JWT/consumidores. Database__ApplyMigrations fica false nos pods normais. Placeholders dos YAMLs são substituídos em cópias temporárias pelas ferramentas de entrega.

Frontend é entregue após as três APIs e cria o ALB interno compartilhado. API Gateway é aplicado depois, recebendo o listener por input. Nginx roteia os três prefixos /api/{os,execution,billing}/v1; APIs validam JWT e OS realiza login. HPA usa metrics-server; OTLP segue ao Collector compartilhado.

Bancos, Kafka/tópicos, segredos e configurações são criados pela action central. Consultar o [guia operacional](../docs/operacao/plataforma-aws.md) antes de ativar o fluxo e seu agendamento destrutivo.

## Inventário e pré-requisitos

| Pasta | Namespace | Recursos da aplicação |
| --- | --- | --- |
| os | fiap-os | ConfigMap, ExternalSecrets, Deployment, Service e HPA |
| frontend | fiap-frontend | ConfigMap, Deployment, Service e Ingresses |

O Infra deve ter criado namespaces, SecretStores, RBAC, CAs, controllers e Collector. DB prepara bancos/grants; messaging prepara Kafka/MongoDB/tópicos. O frontend depende dos Services das três APIs, e seus upstreams precisam coincidir com os nomes provisionados. As imagens e manifestos vêm do mesmo SHA aprovado.

## Conferência

Os arquivos fonte contêm placeholders e devem ser renderizados por scripts/delivery. Não aplique esses arquivos diretamente. Para acompanhar um ambiente já entregue, confirme o contexto Kubernetes e consulte:

```powershell
kubectl -n fiap-os get pods,services,hpa,externalsecrets
kubectl -n fiap-frontend get pods,services,ingresses
kubectl -n fiap-os rollout status deployment/fiap-os-deployment --timeout=360s
kubectl -n fiap-frontend rollout status deployment/fiap-frontend-deployment --timeout=360s
```

Detalhes e inspeção manual controlada: [OS](os/README.md), [entrega](../scripts/delivery/README.md) e [frontend](../src/FIAP.TechChallenge.Fase1.Frontend/README.md). Um dry-run ou rollout concluído não comprova autenticação, Saga ou compra externa: o smoke central e a validação operacional precisam conferir essas integrações.
