# Sistema de Gestão de Oficina Mecânica

Aplicação para administrar o ciclo operacional de uma oficina mecânica. A solução reúne uma API em .NET, um frontend React, persistência em PostgreSQL, ambiente local com Docker Compose e infraestrutura AWS provisionada com Terraform e Kubernetes.

## Funcionalidades

- autenticação de usuários com JWT;
- cadastro e gestão de clientes e veículos;
- catálogo de serviços, peças e insumos, com controle de estoque;
- criação de ordens de serviço com cliente e veículo existentes ou cadastrados no próprio fluxo;
- diagnóstico, orçamento, aprovação, execução, finalização, entrega e cancelamento de ordens de serviço;
- acompanhamento público do andamento da ordem de serviço;
- registro do tempo gasto e cálculo do tempo médio dos serviços;
- exclusão lógica para preservação do histórico;
- interface administrativa responsiva para operação dos principais fluxos.

## Arquitetura

O backend é um monólito modular organizado em camadas, com as regras de negócio isoladas dos detalhes de persistência e entrega HTTP:

```mermaid
graph LR
    Frontend["Frontend React"] --> API["API ASP.NET Core"]
    API --> Application["Application / casos de uso"]
    API --> Infrastructure["Infrastructure"]
    Application --> Domain["Domain"]
    Infrastructure --> Domain
    Infrastructure --> PostgreSQL[(PostgreSQL)]
```

- **API:** controllers, autenticação, OpenAPI e composição da aplicação;
- **Application:** casos de uso e contratos de entrada e saída;
- **Domain:** entidades, value objects, enums, interfaces e regras de negócio;
- **Infrastructure:** Entity Framework Core, repositórios, migrations, JWT e serviços externos;
- **Frontend:** aplicação React com rotas protegidas, consumo da API e telas administrativas.

A API aplica automaticamente as migrations pendentes na inicialização, exceto no ambiente de testes.

## Tecnologias

- .NET 10, ASP.NET Core e Entity Framework Core;
- PostgreSQL e Npgsql;
- JWT e BCrypt;
- React 19, TypeScript, Vite e Tailwind CSS;
- Docker, Docker Compose e Nginx;
- Terraform, AWS e Kubernetes;
- Scalar e OpenAPI;
- NUnit, Moq e FluentAssertions.

## Execução local com Docker Compose

### Pré-requisitos

- Docker com Docker Compose;
- portas `5173`, `8080`, `5050` e `5432` disponíveis.

Na raiz do repositório, execute:

```bash
docker compose -f src/docker-compose.yml up --build -d
```

Serviços disponíveis:

| Serviço | Endereço | Credenciais locais |
| --- | --- | --- |
| Frontend | `http://localhost:5173` | — |
| API | `http://localhost:8080` | — |
| Scalar | `http://localhost:8080/scalar/v1` | — |
| OpenAPI | `http://localhost:8080/openapi/v1.json` | — |
| Health check | `http://localhost:8080/api/health` | — |
| PostgreSQL | `localhost:5432` | `postgres / postgres` |
| PgAdmin | `http://localhost:5050` | `admin@admin.com / admin` |

As credenciais e chaves presentes no Compose são destinadas apenas ao desenvolvimento local.

Para encerrar os serviços:

```bash
docker compose -f src/docker-compose.yml down
```

## Execução sem Docker

### Backend

Requer o SDK .NET 10 e uma instância PostgreSQL acessível. Configure `ConnectionStrings:DefaultConnection` e as opções `Jwt` por `appsettings`, variáveis de ambiente ou User Secrets e execute:

```bash
dotnet run --project src/FIAP.TechChallenge.Fase1.API
```

### Frontend

Requer Node.js 22 ou superior. A partir da raiz do repositório:

```bash
cd src/FIAP.TechChallenge.Fase1.Frontend
npm install
npm run dev
```

Para apontar o frontend para uma API executada separadamente, crie um arquivo `.env` no diretório do frontend:

```env
VITE_API_BASE_URL=http://localhost:8080/api
```

No Visual Studio, abra `src/TechChallengeFase1.slnx`. Para iniciar todo o ambiente em contêineres, selecione o projeto `docker-compose` como projeto de inicialização.

## Autenticação e documentação da API

Em ambiente de desenvolvimento, a especificação OpenAPI e a interface Scalar ficam disponíveis nos endereços indicados acima.

O fluxo de autenticação é:

1. criar um usuário em `POST /api/usuarios`;
2. autenticar em `POST /api/usuarios/login`;
3. enviar o token retornado nas requisições protegidas:

```http
Authorization: Bearer <token>
```

Além do cadastro e login, são públicos o health check e `GET /api/ordensservico/acompanhamento/{id}`. Os demais endpoints exigem autenticação.

## Fluxo da ordem de serviço

O fluxo principal de status é:

```text
Recebida → EmDiagnostico → AguardandoAprovacao → EmExecucao → Finalizada → Entregue
```

Uma ordem também pode assumir o status `Cancelada`, conforme as regras do domínio.

Regras importantes:

- serviços e peças ou insumos só podem ser adicionados durante o diagnóstico;
- a execução depende da aprovação do orçamento por código de aprovação;
- serviços só podem ser concluídos enquanto a ordem está em execução;
- a ordem só pode ser finalizada após a conclusão de todos os serviços;
- a entrega só pode ocorrer depois da finalização;
- os itens vinculados à ordem preservam um snapshot dos dados e valores do momento da inclusão.

## Infraestrutura e Kubernetes

A infraestrutura de produção é declarada em Terraform na pasta `infra` e provisiona, na região `us-east-1`, uma VPC, um cluster Amazon EKS, PostgreSQL no Amazon RDS, repositórios Amazon ECR, Secrets Manager, backend remoto no S3 e autenticação OIDC para as pipelines do GitHub Actions.

Os manifests em `k8s` implantam backend e frontend em namespaces separados. O backend possui duas réplicas, probes de saúde, limites de recursos e HPA de 2 a 10 pods; seus segredos são sincronizados do AWS Secrets Manager pelo External Secrets. O frontend também possui duas réplicas e é publicado por um serviço `LoadBalancer`, encaminhando `/api` para o serviço interno do backend.

Os módulos Terraform devem ser aplicados nesta ordem:

```text
bootstrap → aws-resources → kubernetes-addons → kubernetes-configs
```

O passo a passo de provisionamento, deploy, validação e destruição está em [`infra/README.md`](infra/README.md). A organização dos manifests, o deploy manual e os comandos de diagnóstico estão em [`k8s/README.md`](k8s/README.md). Os workflows em `.github/workflows` automatizam a infraestrutura, o build e publicação das imagens no ECR e o deploy das aplicações no EKS.

## Testes

A solução contém testes de domínio, casos de uso, infraestrutura e API. Para executar toda a suíte:

```bash
dotnet test src/TechChallengeFase1.slnx
```

## Estrutura do repositório

```text
.
├── .github/workflows/   # CI/CD, infraestrutura e deploy
├── infra/               # módulos Terraform e documentação operacional
├── k8s/                 # manifests do backend e frontend
└── src/                 # solução .NET, frontend e Docker Compose
```

## Observações Finais

Este projeto foi construído com foco em:

- aplicar princípios de DDD na modelagem do domínio;
- manter uma separação clara de responsabilidades entre as camadas;
- experimentar, na prática, o conceito de domínio rico;
- proteger as regras de negócio dos detalhes de infraestrutura e apresentação;
- oferecer uma experiência completa, da interface administrativa à persistência dos dados;
- facilitar a execução local, os testes, a avaliação e a evolução futura do sistema;
- aplicar infraestrutura como código e automatizar o provisionamento e o deploy;
- explorar a execução em nuvem com AWS e a orquestração de aplicações com Kubernetes.

Este é meu primeiro projeto em que aplico esses conceitos de forma tão abrangente, cobrindo não apenas a modelagem e a implementação da aplicação, mas também frontend, conteinerização, infraestrutura e entrega contínua.
