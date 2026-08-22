# Kubernetes

Esta pasta contém os manifests Kubernetes do backend, frontend e observabilidade. Os recursos foram preparados para execução no Amazon EKS e dependem da infraestrutura provisionada no repositório [`fiap-tech-challenge-infra`](https://github.com/Maieru/fiap-tech-challenge-infra).

## Arquitetura do deploy

```mermaid
graph LR
    Internet --> LB["Service LoadBalancer<br/>porta 30150"]
    LB --> Frontend["Frontend / Nginx<br/>2 réplicas"]
    Frontend -->|"/api"| BackendService["Service ClusterIP<br/>porta 8080"]
    BackendService --> Backend["API .NET<br/>2 a 10 réplicas"]
    Backend --> RDS[(Amazon RDS PostgreSQL)]
    Backend -->|"OTLP"| Collector[OpenTelemetry Collector]
    Collector --> Jaeger
    Collector --> Loki
    Prometheus -->|"/metrics"| BackendService
    Grafana --> Prometheus
    Grafana --> Loki
    Secrets["AWS Secrets Manager"] --> ExternalSecrets["External Secrets Operator"]
    ExternalSecrets --> BackendSecret["Kubernetes Secret"]
    BackendSecret --> Backend
```

O frontend é o único componente exposto publicamente. As chamadas para `/api` são encaminhadas pelo Nginx para o serviço interno do backend, usando o endereço `fiap-backend-service.fiap-backend.svc:8080`.

## Estrutura

```text
k8s/
├── backend/
│   ├── infra/
│   │   ├── namespace.yaml
│   │   └── secret-store.yml
│   └── application/
│       ├── config-maps.yaml
│       ├── deployment.yaml
│       ├── hpa.yaml
│       ├── secrets.yaml
│       └── services.yaml
└── frontend/
    ├── infra/
    │   └── namespace.yaml
    └── application/
        ├── config-maps.yaml
        ├── deployment.yaml
        └── services.yaml
```

Os diretórios possuem responsabilidades diferentes:

- `infra`: namespaces e `SecretStore` gerenciados pelo módulo Terraform `kubernetes-configs`;
- `application`: configurações, segredos externos, deployments, serviços e escalabilidade aplicados pelo workflow ou pelo `kubectl`.

Em `observability`, os manifests de `application` são aplicados pelo módulo
Terraform `infra/kubernetes-configs` do repositório de infraestrutura. Seus ConfigMaps são gerados diretamente
de `src/ObservabilityConfig`, compartilhando a configuração com o Docker
Compose. Os Services são internos (`ClusterIP`) e os dados são efêmeros para
manter a instalação simples.

## Recursos implantados

### Backend

O backend é executado no namespace `fiap-backend` e possui:

- `Deployment` com duas réplicas iniciais;
- imagem hospedada no Amazon ECR com `imagePullPolicy: Always`;
- `ConfigMap` com configurações do ASP.NET Core e do JWT;
- `ExternalSecret` que cria o Secret `fiap-backend-secret`;
- `Service` do tipo `ClusterIP`, disponível internamente na porta `8080`;
- startup e liveness probes no endpoint `/api/health/live`;
- readiness probe no endpoint `/api/health/ready`;
- requests de `100m` de CPU e `128Mi` de memória;
- limits de `500m` de CPU e `512Mi` de memória;
- `HorizontalPodAutoscaler` entre 2 e 10 réplicas, com alvo de 70% de utilização de CPU.

O `ExternalSecret` lê o segredo `fiap-secret-manager-backend` do AWS Secrets Manager. Esse segredo é criado pelo Terraform e contém a connection string do PostgreSQL e a chave de assinatura JWT. Os valores sensíveis não ficam armazenados nos manifests.

### Frontend

O frontend é executado no namespace `fiap-frontend` e possui:

- `Deployment` com duas réplicas;
- imagem hospedada no Amazon ECR com `imagePullPolicy: Always`;
- `ConfigMap` com o endereço interno do backend;
- requests de `100m` de CPU e `128Mi` de memória;
- limits de `500m` de CPU e `512Mi` de memória;
- `Service` do tipo `LoadBalancer`, publicado na porta `30150` e encaminhado à porta `80` do Nginx.

## Pré-requisitos

Antes de aplicar os manifests da aplicação, é necessário ter:

- AWS CLI autenticada;
- `kubectl` instalado;
- cluster EKS `fiap-eks-cluster` criado;
- imagens do backend e do frontend publicadas nos respectivos repositórios ECR;
- External Secrets Operator e Metrics Server instalados;
- namespaces `fiap-backend` e `fiap-frontend` criados;
- `SecretStore` `aws-secrets-store` disponível no namespace do backend;
- segredo `fiap-secret-manager-backend` disponível no AWS Secrets Manager.

Essas dependências são provisionadas pelos módulos Terraform nesta ordem:

```text
bootstrap → aws-resources → kubernetes-addons → kubernetes-configs
```

Consulte o [`README` de infraestrutura](https://github.com/Maieru/fiap-tech-challenge-infra/tree/main/infra) para o procedimento completo de provisionamento.

## Deploy manual

Execute os comandos a partir da raiz do repositório.

### 1. Configurar o acesso ao cluster

```bash
aws eks update-kubeconfig --name fiap-eks-cluster --region us-east-1
kubectl get nodes
```

Prossiga quando os nós estiverem com status `Ready`.

### 2. Validar as dependências

```bash
kubectl get deployment external-secrets -n external-secrets
kubectl get deployment metrics-server -n kube-system
kubectl get namespaces fiap-backend fiap-frontend
kubectl get secretstore aws-secrets-store -n fiap-backend
```

### 3. Aplicar os manifests

```bash
kubectl apply -f k8s/backend/application
kubectl apply -f k8s/frontend/application
```

### 4. Aguardar o rollout

```bash
kubectl rollout status deployment/fiap-backend-deployment -n fiap-backend --timeout=5m
kubectl rollout status deployment/fiap-frontend-deployment -n fiap-frontend --timeout=5m
```

## Validação

Confira os principais recursos:

```bash
kubectl get pods,services -n fiap-backend
kubectl get pods,services -n fiap-frontend
kubectl get hpa -n fiap-backend
kubectl get externalsecret -n fiap-backend
```

O `ExternalSecret` deve estar sincronizado e o Secret de destino deve existir:

```bash
kubectl describe externalsecret fiap-backend-secret -n fiap-backend
kubectl get secret fiap-backend-secret -n fiap-backend
```

Para consultar o endereço público do frontend:

```bash
kubectl get service fiap-frontend-service -n fiap-frontend
```

Depois que o campo `EXTERNAL-IP` receber um endereço ou hostname, acesse:

```text
http://<EXTERNAL-IP-OU-HOSTNAME>:30150
```

Também é possível validar os serviços por port-forward:

```bash
kubectl port-forward service/fiap-frontend-service 8081:30150 -n fiap-frontend
kubectl port-forward service/fiap-backend-service 8080:8080 -n fiap-backend
```

Com os comandos executados separadamente, o frontend fica disponível em `http://localhost:8081`, a liveness do backend em `http://localhost:8080/api/health/live` e a readiness em `http://localhost:8080/api/health/ready`.

## Deploy pelo GitHub Actions

O workflow `.github/workflows/deploy-applications.yml` realiza o deploy no EKS. Ele:

1. autentica na AWS via OIDC;
2. atualiza o `kubeconfig` do cluster;
3. verifica as permissões no Kubernetes;
4. reinicia e aguarda o External Secrets Operator;
5. aplica os manifests de `backend/application` e `frontend/application`;
6. reinicia os deployments para buscar as imagens marcadas como `latest`;
7. aguarda a conclusão dos rollouts.

O workflow `.github/workflows/initialize-and-deploy.yml` coordena o processo completo: aplica a infraestrutura, publica as duas imagens no ECR e executa o deploy das aplicações.

## Configurações que exigem atenção

- as imagens dos deployments contêm o ID da conta AWS e a região do ECR; ajuste os endereços se o ambiente mudar;
- os manifests usam a tag `latest`, enquanto as pipelines também publicam uma tag imutável com o SHA do commit;
- o backend está configurado com `ASPNETCORE_ENVIRONMENT=Development`; revise esse valor antes de utilizar os manifests em um ambiente de produção;
- o HPA depende do Metrics Server para obter as métricas de CPU;
- o External Secrets depende da associação de identidade do pod e das permissões IAM criadas pelo Terraform;
- a criação do Secret pode levar alguns instantes após a aplicação do `ExternalSecret`.

## Diagnóstico de problemas

### Pods e eventos

```bash
kubectl get pods -A
kubectl get events -n fiap-backend --sort-by=.metadata.creationTimestamp
kubectl get events -n fiap-frontend --sort-by=.metadata.creationTimestamp
```

### Logs

```bash
kubectl logs deployment/fiap-backend-deployment -n fiap-backend --tail=200
kubectl logs deployment/fiap-frontend-deployment -n fiap-frontend --tail=200
kubectl logs deployment/external-secrets -n external-secrets --tail=200
```

### Métricas e escalabilidade

```bash
kubectl top pods -n fiap-backend
kubectl describe hpa fiap-backend-hpa -n fiap-backend
```

Se o backend não iniciar, verifique primeiro o `ExternalSecret`, o Secret gerado e a conectividade com o RDS. Se o frontend responder, mas as chamadas para `/api` falharem, valide o serviço do backend e o valor de `API_UPSTREAM` no ConfigMap do frontend.

## Remoção das aplicações

Para remover apenas as cargas da aplicação, preservando o cluster e os recursos gerenciados pelo Terraform:

```bash
kubectl delete -f k8s/frontend/application
kubectl delete -f k8s/backend/application
```

Namespaces, add-ons, identidades AWS e demais recursos de infraestrutura devem ser removidos por meio dos respectivos módulos Terraform, conforme o procedimento descrito no [`README` de infraestrutura](https://github.com/Maieru/fiap-tech-challenge-infra/tree/main/infra).
