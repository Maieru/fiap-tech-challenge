# Sequências dos fluxos principais

Os três primeiros diagramas descrevem a implementação atual. O último representa o requisito ainda não implementado da Fase 3.

## Autenticação administrativa atual

```mermaid
sequenceDiagram
    actor Usuario as Usuário
    participant SPA as SPA
    participant API as API / login
    participant Banco as PostgreSQL
    Usuario->>SPA: Informa login e senha
    SPA->>API: POST /api/usuarios/login
    Note over SPA,API: Na AWS: API Gateway, VPC Link e ALB
    API->>Banco: Consulta usuário ativo pelo login
    Banco-->>API: Usuário e hash de senha
    API->>API: Verifica senha com BCrypt
    alt Credenciais válidas
        API->>API: Assina JWT HMAC-SHA256 (60 minutos)
        API-->>SPA: JWT e dados da resposta
        SPA->>SPA: Guarda sessão em localStorage
        SPA->>API: Chamada protegida com Bearer JWT
        API->>API: Valida assinatura, emissor, audiência e expiração
        API-->>SPA: Resultado da operação
    else Credenciais inválidas
        API-->>SPA: Erro de autenticação
    end
```

Este fluxo autentica `Usuarios`, não clientes por CPF. Ele não usa Lambda para emitir JWT.

## Acesso atual do cliente à ordem na AWS

```mermaid
sequenceDiagram
    actor Cliente
    participant Gateway as API Gateway
    participant Lambda as Lambda Authorizer
    participant Banco as PostgreSQL
    participant API as API via VPC Link e ALB
    Cliente->>Gateway: Rota protegida da ordem com id e token na query
    Gateway->>Lambda: Evento v2 com id e token (sem cache)
    Lambda->>Lambda: Valida formato do token e id
    Lambda->>Banco: Consulta ordem e cliente ativos
    Banco-->>Lambda: CPF e código de aprovação, se encontrados
    Lambda->>Lambda: Compara SHA-256 do CPF normalizado + código
    alt Token válido
        Lambda-->>Gateway: isAuthorized = true
        Gateway->>API: Encaminha a requisição
        opt Aprovar execução
            API->>Banco: Carrega ordem e cliente
            API->>API: Valida novamente o token
        end
        API-->>Gateway: Resultado do caso de uso
        Gateway-->>Cliente: Resposta HTTP
    else Token inválido ou registro ausente/inativo
        Lambda-->>Gateway: isAuthorized = false
        Gateway-->>Cliente: Acesso negado
    end
```

O token é gerado na criação da ordem quando o cliente tem CPF e não é JWT. A proteção da Lambda se aplica às três rotas declaradas no gateway. Acesso direto ao backend não executa esse controle; acompanhamento e cancelamento não repetem a validação do token na aplicação.

## Abertura completa de ordem

```mermaid
sequenceDiagram
    actor Operador
    participant API as API
    participant UC as CriarOrdemServicoCompleta
    participant Casos as Casos de uso de cliente, veículo e ordem
    participant Banco as PostgreSQL via repositórios EF
    participant OTel as OpenTelemetry
    Operador->>API: POST /api/ordensservico/completa + Bearer JWT
    API->>API: Valida JWT e entrada HTTP
    API->>UC: Executa command
    UC->>UC: Abre TransactionScope assíncrono
    UC->>Casos: Cria cliente, veículo e ordem
    Casos->>Banco: Persiste cliente e veículo
    Casos->>Casos: Confere vínculo cliente-veículo e cria ordem Recebida
    Casos->>Banco: Persiste ordem e código de aprovação
    Casos->>OTel: Agenda contador para após commit
    Casos-->>UC: Dados da ordem e token de acesso
    UC->>Casos: Inicia diagnóstico
    Casos->>Banco: Atualiza status e timestamp
    loop Serviços e peças informados
        UC->>Casos: Adiciona item
        Casos->>Banco: Persiste snapshot e alterações de estoque aplicáveis
    end
    alt Todas as operações concluídas
        UC->>UC: Complete e encerramento do escopo
        Banco-->>UC: Commit
        OTel->>OTel: Registra ordem criada após commit
        UC-->>API: Resultado de sucesso
        API-->>Operador: 201 com ordem EmDiagnostico e itens
    else Falha de validação ou persistência
        UC->>UC: Encerra escopo sem Complete
        Banco-->>UC: Rollback
        UC-->>API: Resultado de falha ou exceção
        API-->>Operador: Resposta de erro
    end
```

A criação simples (`POST /api/ordensservico`) usa cliente e veículo existentes e retorna a ordem em `Recebida`. O fluxo `/com-cliente-veiculo` faz três gravações sem transação própria quando chamado diretamente; somente o fluxo completo acima abre o escopo externo.

## Autenticação por CPF exigida pela Fase 3 — pendente

```mermaid
sequenceDiagram
    actor Cliente
    participant Gateway as API Gateway
    participant Funcao as Function de autenticação
    participant Banco as Base de clientes
    participant API as API protegida
    Cliente->>Gateway: Solicita autenticação informando CPF
    Gateway->>Funcao: Encaminha solicitação
    Funcao->>Funcao: Valida CPF
    Funcao->>Banco: Consulta existência e status do cliente
    Banco-->>Funcao: Cliente ativo ou recusa
    alt CPF válido e cliente ativo
        Funcao->>Funcao: Gera JWT assinado e com expiração
        Funcao-->>Gateway: JWT
        Gateway-->>Cliente: JWT
        Cliente->>Gateway: Consome API com Bearer JWT
        Gateway->>API: Encaminha conforme política de validação JWT
        API->>API: Aplica autorização para o cliente autenticado
        API-->>Cliente: Resultado via gateway
    else CPF inválido, ausente ou cliente inativo
        Funcao-->>Gateway: Autenticação recusada
        Gateway-->>Cliente: Erro sem emissão de token
    end
```

Este diagrama transcreve o comportamento exigido; não define um endpoint já disponível nem uma decisão aceita. A implementação ainda deve definir contrato, assinatura, claims e escopo de acesso, e atualizar a RFC-003 junto da mudança.

## Evidências

- [Controllers](../../src/FIAP.TechChallenge.Fase1.API/Controllers).
- [Fluxo completo](../../src/FIAP.TechChallenge.Fase1.Application/UseCases/OrdensServico/CriarOrdemServicoCompleta/CriarOrdemServicoCompletaUseCase.cs).
- [Criação simples](../../src/FIAP.TechChallenge.Fase1.Application/UseCases/OrdensServico/CriarOrdemServico/CriarOrdemServicoUseCase.cs).
- [Fluxo com cliente e veículo](../../src/FIAP.TechChallenge.Fase1.Application/UseCases/OrdensServico/CriarOrdemServicoComClienteEVeiculo/CriarOrdemServicoComClienteEVeiculoUseCase.cs).
- [Lambda](https://github.com/Maieru/fiap-tech-challenge-serverless/tree/main/src/FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer).
- [RFC-003](../RFCs/RFC-003-autenticacao-jwt-e-bcrypt.md).
