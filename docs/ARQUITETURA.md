# Arquitetura da solução

Este documento apresenta uma visão simplificada da infraestrutura AWS, da organização das cargas no Amazon EKS e das camadas da aplicação.

## Infraestrutura AWS

```mermaid
flowchart LR
    User([Usuário]) --> Gateway["Amazon API Gateway<br/>HTTP API"]

    subgraph AWS["AWS - us-east-1"]
        ECR[(Amazon ECR<br/>Frontend e Backend)]
        Secrets[AWS Secrets Manager]
        Logs[CloudWatch Logs<br/>retenção de 7 dias]

        subgraph VPC["VPC"]
            Link["VPC Link"] --> ALB["ALB interno"]

            subgraph EKS["Amazon EKS"]
                FrontService["Frontend Service<br/>ClusterIP"] --> FrontPods["Pods React + Nginx"]
                BackService["Backend Service<br/>ClusterIP"] --> BackPods["Pods ASP.NET Core"]
            end

            subgraph Database["Sub-redes de banco"]
                RDS[(Amazon RDS<br/>PostgreSQL)]
            end
        end
    end

    Gateway --> Logs
    Gateway --> Link
    ALB -->|"/*"| FrontService
    ALB -->|"/api/*"| BackService
    ECR -->|Imagens| FrontPods
    ECR -->|Imagens| BackPods
    Secrets -->|Configurações sensíveis| BackPods
    BackPods -->|Dados| RDS
```

O API Gateway é o único endpoint público. Seu VPC Link acessa o listener HTTP do ALB interno, que usa regras por caminho e target groups independentes para frontend e backend. Os services Kubernetes e os componentes de observabilidade permanecem privados.

## Organização dos pods no EKS

```mermaid
flowchart TB
    Internet([Internet]) --> Gateway["API Gateway"]
    Gateway --> Link["VPC Link"]
    Link --> ALB["ALB interno"]

    subgraph EKS["Amazon EKS"]
        direction TB

        subgraph FrontNS["Namespace: fiap-frontend"]
            direction LR
            FrontService["Service<br/>ClusterIP"] --> FrontDeploy[Deployment]
            FrontDeploy --> FrontPods["1 pod<br/>React + Nginx"]
        end

        subgraph Internal["Cargas internas"]
            direction LR

            subgraph BackNS["Namespace: fiap-backend"]
                direction TB
                BackService["Service<br/>ClusterIP"] --> BackDeploy[Deployment]
                HPA[HPA] -->|1 a 10 réplicas| BackDeploy
                BackDeploy --> BackPods["Pods Backend<br/>API .NET"]
            end

            subgraph ObsNS["Namespace: fiap-observability"]
                direction TB
                Telemetry[OpenTelemetry] --> Monitoring["Prometheus<br/>Loki + Jaeger"]
                Monitoring --> Grafana[Grafana]
            end
        end

        BackPods -->|Telemetria| Telemetry
    end

    ALB -->|"/*"| FrontService
    ALB -->|"/api/*"| BackService
    BackPods --> RDS[(Amazon RDS<br/>PostgreSQL)]
```

O AWS Load Balancer Controller registra diretamente os IPs dos pods nos target groups do ALB. O frontend mantém uma réplica; o backend começa com uma e pode escalar até dez conforme o uso de CPU.

## Camadas da aplicação

```mermaid
flowchart TB
    User([Usuário]) --> Frontend["Frontend<br/>React + TypeScript"]
    Frontend --> API["API<br/>Controllers e HTTP"]
    API --> Application["Application<br/>Casos de uso"]
    Application --> Domain["Domain<br/>Entidades e regras de negócio"]

    API --> Infrastructure["Infrastructure<br/>Persistência e integrações"]
    Infrastructure -. implementa contratos .-> Domain
    Infrastructure --> Database[(PostgreSQL)]
```

O `Domain` concentra as regras de negócio e não depende das demais camadas. A `Application` coordena os casos de uso, a `API` expõe as operações por HTTP e a `Infrastructure` implementa persistência e integrações externas.
