# FIAP Tech Challenge Fase 1

Sistema de gestão para oficina mecânica, desenvolvido com arquitetura em camadas, back-end em .NET, persistência em PostgreSQL e frontend React opcional em projeto separado dentro da mesma solução.

## Visão Geral

O sistema cobre o ciclo operacional da oficina, contemplando:

- cadastro de clientes e veículos;
- catálogo de serviços e peças/insumos;
- abertura, acompanhamento e atualização de ordens de serviço;
- autenticação de usuários com JWT;
- consulta do progresso da ordem de serviço via API;
- controle operacional para execução dos serviços;
- suporte ao acompanhamento do tempo médio de execução dos serviços.

### Fluxo técnico resumido

1. `Controller` recebe a requisição HTTP.
2. `UseCase` coordena o caso de uso e aplica as regras de negócio.
3. `Repository` abstrai o acesso à persistência por meio de interfaces definidas no domínio.
4. `Infrastructure` implementa persistência, autenticação e serviços técnicos.
5. Os dados são persistidos no PostgreSQL via EF Core.

### Migrações

O projeto de API aplica automaticamente as migrations pendentes na inicialização, preparando a base PostgreSQL sem necessidade de execução manual de comandos do EF Core.

## Arquitetura da Solução

O projeto foi desenvolvido como um **monólito em camadas**, conforme proposto no enunciado, buscando simplicidade de implantação para o MVP sem abrir mão de separação de responsabilidades.

### Dependências de ambiente

- Docker + Docker Compose, para subir a API e o banco localmente;
- SDK .NET 10, para execução fora do Docker;
- Node.js, apenas para execução do frontend fora do Docker.

### Dependências entre projetos da solução

- `FIAP.TechChallenge.Fase1.API`
  - depende de `Application` e `Infrastructure`;
- `FIAP.TechChallenge.Fase1.Application`
  - depende de `Domain`;
- `FIAP.TechChallenge.Fase1.Infrastructure`
  - depende de `Domain` e implementa persistência, segurança e serviços externos;
- `FIAP.TechChallenge.Fase1.Domain`
  - núcleo do domínio, contendo entidades, value objects, enums, interfaces e regras de negócio;
- projetos `*.Tests`
  - cobrem API, Application, Domain e Infrastructure.

### Diagrama de dependências

```mermaid
graph LR
    API["FIAP.TechChallenge.Fase1.API"] --> APP["FIAP.TechChallenge.Fase1.Application"]
    API --> INFRA["FIAP.TechChallenge.Fase1.Infrastructure"]
    APP --> DOMAIN["FIAP.TechChallenge.Fase1.Domain"]
    INFRA --> DOMAIN
````

## Tecnologias Utilizadas

* .NET 10
* ASP.NET Core
* Entity Framework Core
* PostgreSQL
* Npgsql
* JWT Authentication
* Docker / Docker Compose
* React
* Scalar / OpenAPI para documentação da API

## Escolha do Banco de Dados

O PostgreSQL foi escolhido por ser um **banco de dados relacional robusto, gratuito e de código aberto**, com fácil uso via Docker, boa integração com aplicações C#/.NET por meio do provider Npgsql e recursos adequados para garantir integridade e consistência dos dados em um sistema transacional de gestão de ordens de serviço, clientes, veículos, peças e estoque.

A escolha por um banco relacional também se mostra adequada pela necessidade de:

* relacionamentos bem definidos entre clientes, veículos, ordens, serviços e peças;
* consistência transacional;
* integridade referencial;
* modelagem estruturada para operações administrativas e operacionais.

## Segurança e Validações

O projeto contempla requisitos de segurança e qualidade previstos no desafio, incluindo:

* autenticação JWT para endpoints administrativos;
* proteção de endpoints privados com `Authorization: Bearer <token>`;
* armazenamento de senha de usuário de forma protegida;
* validação de CPF/CNPJ;
* validação de placa de veículo;
* separação de responsabilidades entre camadas;
* testes automatizados para fluxos críticos.

## Serviços no Docker Compose

O `docker-compose.yml` sobe os seguintes serviços:

* `fiap-techchallenge-api` (API .NET) em `http://localhost:8080`
* `fiap-techchallenge-db` (PostgreSQL) em `localhost:5432`
* `fiap-techchallenge-pgadmin` (opcional, administração do banco) em `http://localhost:5050`

### Credenciais padrão no compose

* PostgreSQL: `postgres / postgres`
* PgAdmin: `admin@admin.com / admin`

## Como Executar com Docker Compose

Na raiz da solução (`src`), execute:

```bash
docker compose -f docker-compose.yml up --build -d
```

Após a inicialização, a API estará disponível em:

```text
http://localhost:8080
```

## Como Executar no Visual Studio

Abra a solução `TechChallengeFase1` no Visual Studio e selecione o projeto `docker-compose` como **Startup Project**.

## Documentação da API

Em ambiente de desenvolvimento, a API publica a documentação interativa via Scalar.

* UI do Scalar: `http://localhost:8080/scalar/v1`
* OpenAPI JSON: `http://localhost:8080/openapi/v1.json`

A documentação permite visualizar e testar os endpoints disponíveis de forma interativa.

## Autenticação

### Regras

* os endpoints de usuário (`/api/usuarios` e `/api/usuarios/login`) são públicos;
* os demais endpoints exigem autenticação por token JWT;
* por se tratar de um projeto acadêmico, não é necessário nenhum nível de privilégio para 
criar usuários. Em um projeto de produção, isso deve ser revisado.

### Como autenticar

1. Crie um usuário em `/api/usuarios`;
2. Faça login em `/api/usuarios/login`;
3. Copie o token JWT retornado;
4. Envie o token no header das requisições protegidas:

```http
Authorization: Bearer <seu_token>
```

## Regras de Negócio

### Fluxo da Ordem de Serviço

A entidade `OrdemServico` implementa o seguinte fluxo de status:

1. `Recebida`
2. `EmDiagnostico`
3. `AguardandoAprovacao`
4. `EmExecucao`
5. `Finalizada`
6. `Entregue`

### Regras importantes

* serviços e peças/insumos só podem ser adicionados enquanto a OS está em diagnóstico;
* a execução só pode começar após o avanço correto no fluxo;
* um serviço só pode ser concluído quando a OS estiver em execução;
* a OS só pode ser finalizada quando todos os serviços vinculados tiverem sido concluídos;
* a OS só pode ser entregue quando já estiver finalizada;
* o progresso da ordem de serviço pode ser consultado via API;
* os dados de serviços e peças vinculados à OS são preservados por snapshot, evitando impacto de alterações futuras no catálogo administrativo.

## Principais Entidades

### Núcleo

* `Cliente`: dados do cliente, incluindo identificação e contato;
* `Veiculo`: veículo vinculado ao cliente, contendo placa, marca, modelo e ano;
* `Usuario`: responsável pela autenticação no sistema.

### Catálogo administrativo

* `Servico`: serviço cadastrável, com descrição e valor unitário;
* `PecaInsumo`: peça ou insumo do estoque, com código, preço, quantidade e status de ativo.

### Operação da oficina

* `OrdemServico`: representa o processo completo de atendimento da oficina;
* `ServicoDaOrdemDeServico`: snapshot do serviço aplicado na OS, preservando dados do momento da vinculação e informações operacionais como conclusão e tempo gasto;
* `PecaOuInsumoDaOrdemDeServico`: snapshot da peça/insumo aplicada na OS, desacoplado de alterações posteriores no catálogo ou no estoque.

### Relações principais

* 1 cliente -> N veículos
* 1 cliente -> N ordens de serviço
* 1 veículo -> N ordens de serviço
* 1 ordem de serviço -> N serviços da ordem
* 1 ordem de serviço -> N peças/insumos da ordem

## Endpoints Principais

Os grupos de endpoints contemplam os requisitos funcionais do desafio, incluindo:

* autenticação e usuários;
* CRUD de clientes;
* CRUD de veículos;
* CRUD de serviços;
* CRUD de peças e insumos;
* criação, listagem, detalhamento e acompanhamento de ordens de serviço;
* atualização do fluxo operacional da ordem;
* consulta de progresso da OS.

A lista completa e atualizada pode ser consultada na documentação OpenAPI/Scalar.

## Testes

O projeto possui testes unitários e de integração cobrindo os fluxos críticos do domínio, com foco especial em:

* autenticação;
* validações de dados sensíveis;
* catálogo administrativo;
* ciclo de vida da ordem de serviço;
* integração entre API, aplicação e persistência.

Para executar os testes localmente:

```bash
dotnet test
```

## Frontend (Opcional)

O repositório contém também um frontend React em `FIAP.TechChallenge.Fase1.Frontend`, utilizado como apoio visual e para validação manual dos fluxos. Ele não compõe o escopo obrigatório principal da Fase 1.

Para executar o frontend localmente:

```bash
cd FIAP.TechChallenge.Fase1.Frontend
npm install
npm run dev
```

Se o backend estiver rodando via Docker em `http://localhost:8080`, ajuste o `.env` do frontend para:

```env
VITE_API_BASE_URL=http://localhost:8080/api
```

## Observações Finais

Este projeto foi construído com foco em:

* aplicar princípios de DDD na modelagem do domínio;
* manter separação clara de responsabilidades;
* experimentar com o conceito de domínio rico;
* facilitar execução local, avaliação e evolução futura do sistema.

Este é meu primeiro projeto onde tento aplicar os conceitos acima de forma tão forte. Com certeza a organização dos projetos e a separação de responsabilidades não está perfeita e pode ser melhorada.