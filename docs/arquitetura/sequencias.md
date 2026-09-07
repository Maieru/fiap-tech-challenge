# Sequências dos fluxos principais

Os diagramas descrevem a implementação atual: autenticação administrativa na API, validação das requisições dos clientes pela Lambda e abertura de ordens.

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

Este fluxo autentica os usuários administrativos por login e senha, com JWT emitido pela API.

## Acesso atual do cliente à ordem na AWS

Conforme o alinhamento do projeto, a Lambda atua como authorizer do API Gateway: valida o acesso do cliente à ordem e devolve a decisão de autorização. A emissão de JWT permanece no fluxo administrativo da API.

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
    alt Formato inválido
        Lambda-->>Gateway: isAuthorized = false
        Gateway-->>Cliente: Acesso negado
    else Formato válido
        Lambda->>Banco: Consulta ordem e cliente ativos
        Banco-->>Lambda: CPF e código de aprovação, se encontrados
        Lambda->>Lambda: Valida CPF armazenado e compara SHA-256 do CPF + código
        alt Acesso válido
            Lambda-->>Gateway: isAuthorized = true
            Gateway->>API: Encaminha a requisição
            opt Aprovar execução
                API->>Banco: Carrega ordem e cliente
                API->>API: Valida novamente o token
            end
            API-->>Gateway: Resultado do caso de uso
            Gateway-->>Cliente: Resposta HTTP
        else Registro ausente/inativo, CPF inválido ou token divergente
            Lambda-->>Gateway: isAuthorized = false
            Gateway-->>Cliente: Acesso negado
        end
    end
```

O token é gerado pela aplicação na criação da ordem quando o cliente tem CPF. Ele contém o SHA-256 do CPF normalizado concatenado ao código de aprovação da ordem. O cliente envia o identificador da ordem no caminho e o token na query string.

| Método | Rota validada pela Lambda |
| --- | --- |
| GET | `/api/ordensservico/acompanhamento/{id}` |
| PUT | `/api/ordensservico/{id}/aprovar-execucao` |
| PUT | `/api/ordensservico/{id}/cancelar` |

A Lambda verifica o formato do token e do identificador, consulta ordem e cliente ativos e compara o token com o valor esperado. Entradas inválidas são recusadas antes da consulta; registros ausentes/inativos, CPF inválido ou token divergente resultam em `isAuthorized = false`. Com acesso válido, retorna `isAuthorized = true`, e o gateway encaminha a requisição à API. A decisão não usa cache.

Acesso direto ao backend não executa esse controle; acompanhamento e cancelamento não repetem a validação do token na aplicação. A aprovação valida o token também no caso de uso.

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

## Evidências

- [Controllers](../../src/FIAP.TechChallenge.Fase1.API/Controllers).
- [Fluxo completo](../../src/FIAP.TechChallenge.Fase1.Application/UseCases/OrdensServico/CriarOrdemServicoCompleta/CriarOrdemServicoCompletaUseCase.cs).
- [Criação simples](../../src/FIAP.TechChallenge.Fase1.Application/UseCases/OrdensServico/CriarOrdemServico/CriarOrdemServicoUseCase.cs).
- [Fluxo com cliente e veículo](../../src/FIAP.TechChallenge.Fase1.Application/UseCases/OrdensServico/CriarOrdemServicoComClienteEVeiculo/CriarOrdemServicoComClienteEVeiculoUseCase.cs).
- [Lambda](https://github.com/Maieru/fiap-tech-challenge-serverless/tree/main/src/FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer).
- [RFC-003](../RFCs/RFC-003-autenticacao-jwt-e-bcrypt.md).
