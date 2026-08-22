# Arquitetura da solução

Este documento apresenta uma visão simplificada da infraestrutura AWS, da organização das cargas no Amazon EKS e das camadas da aplicação.

## Infraestrutura AWS

```mermaid
flowchart LR
    User([Usuário]) --> LB["AWS Load Balancer"]

    subgraph AWS["AWS - us-east-1"]
        ECR[(Amazon ECR<br/>Frontend e Backend)]
        Secrets[AWS Secrets Manager]

        subgraph VPC["VPC"]
            subgraph Public["Sub-redes públicas"]
                LB
                subgraph EKS["Amazon EKS"]
                    FrontService["Kubernetes Service<br/>type: LoadBalancer"]
                    Workloads[Pods da aplicação]
                    FrontService --> Workloads
                end
            end

            subgraph Database["Sub-redes de banco"]
                RDS[(Amazon RDS<br/>PostgreSQL)]
            end
        end
    end

    LB --> FrontService
    ECR -->|Imagens| Workloads
    Secrets -->|Configurações sensíveis| Workloads
    Workloads -->|Dados| RDS
```

O AWS Load Balancer fica fora do EKS e encaminha o tráfego para o `Service` Kubernetes do frontend. O backend e os componentes de observabilidade permanecem acessíveis apenas dentro do cluster.

## Organização dos pods no EKS

```mermaid
flowchart TB
    Internet([Internet]) --> LoadBalancer["AWS Load Balancer<br/>fora do EKS"]

    subgraph EKS["Amazon EKS"]
        direction TB

        subgraph FrontNS["Namespace: fiap-frontend"]
            direction LR
            FrontService["Service<br/>LoadBalancer"] --> FrontDeploy[Deployment]
            FrontDeploy --> FrontPods["1 pod<br/>React + Nginx"]
        end

        subgraph Internal["Cargas internas"]
            direction LR

            subgraph BackNS["Namespace: fiap-backend"]
                direction TB
                BackService[Service interno] --> BackDeploy[Deployment]
                HPA[HPA] -->|1 a 10 réplicas| BackDeploy
                BackDeploy --> BackPods["Pods Backend<br/>API .NET"]
            end

            subgraph ObsNS["Namespace: fiap-observability"]
                direction TB
                Telemetry[OpenTelemetry] --> Monitoring["Prometheus<br/>Loki + Jaeger"]
                Monitoring --> Grafana[Grafana]
            end
        end

        FrontPods -->|/api| BackService
        BackPods -->|Telemetria| Telemetry
    end

    LoadBalancer --> FrontService
    BackPods --> RDS[(Amazon RDS<br/>PostgreSQL)]
```

Os pods podem ser distribuídos entre os nós do node group gerenciado pelo EKS. O frontend mantém uma réplica; o backend começa com uma e pode escalar até dez conforme o uso de CPU.

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
