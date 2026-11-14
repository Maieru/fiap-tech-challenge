# Sequências dos fluxos principais

Os diagramas descrevem o frontend e os microsserviços verificados localmente. O [estado da migração](estado-da-migracao.md) distingue essa validação da implantação AWS ainda pendente.

## Frontend migrado: autenticação e roteamento

```mermaid
sequenceDiagram
    actor Operador
    participant SPA as Frontend React
    participant Proxy as Proxy Vite ou Nginx
    participant OS as OS
    participant Execution as Execution
    participant Billing as Billing
    Operador->>SPA: Informa usuário e senha
    SPA->>Proxy: POST /api/os/v1/usuarios/login
    Proxy->>OS: Encaminha login
    OS-->>SPA: token, tipoToken e expiresInSeconds
    SPA->>SPA: Persiste sessão administrativa
    SPA->>Proxy: /api/os/v1 com Bearer JWT
    Proxy->>OS: Cadastros, ordens e entrega
    SPA->>Proxy: /api/execution/v1 com Bearer JWT
    Proxy->>Execution: Catálogo, estoque e execução
    SPA->>Proxy: /api/billing/v1 com Bearer JWT
    Proxy->>Billing: Consulta da cobrança
    Note over SPA,Billing: Cada API valida o mesmo JWT localmente
```

Localmente, o proxy evita acesso do navegador às três portas e não exige Kong. Na configuração de nuvem preparada, API Gateway e ALB ficam antes do Nginx, preservando os prefixos. Essa topologia ainda requer implantação e validação remotas.

## Frontend migrado: abertura e confirmação assíncrona

```mermaid
sequenceDiagram
    actor Operador
    participant SPA as Frontend React
    participant OS as OS via proxy
    participant Kafka
    participant Execution
    opt Novo cliente
        SPA->>OS: POST /api/os/v1/clientes
        OS-->>SPA: ID do cliente
        SPA->>SPA: Preserva ID para retomar falhas
    end
    opt Novo veículo
        SPA->>OS: POST /api/os/v1/veiculos
        OS-->>SPA: ID do veículo
        SPA->>SPA: Preserva ID para retomar falhas
    end
    SPA->>OS: POST /api/os/v1/ordens
    OS->>Kafka: OrdemServicoCriada
    OS-->>SPA: 201 com ID da ordem
    Kafka->>Execution: Entrega evento
    Execution->>Execution: Cria execução no próprio banco
    SPA->>OS: GET /api/os/v1/ordens/{id}
    SPA->>Execution: GET /api/execution/v1/ordens/{id} via proxy
    alt Execução ainda não disponível (404)
        SPA->>SPA: Exibe pendência e valores indisponíveis
        loop A cada 3 segundos enquanto a página estiver visível
            SPA->>OS: Consulta status administrativo
            SPA->>Execution: Consulta execução via proxy
        end
    else Execução disponível
        Execution-->>SPA: execucao e valorTotal
        SPA->>SPA: Compõe detalhes e habilita ações compatíveis
    end
```

As etapas de cadastro não formam uma transação única. IDs confirmados são reutilizados após erro; timeout após commit ainda exige conferir o cadastro por não haver chave de idempotência. Aprovação/cancelamento retornam 202 e aguardam confirmação por eventos. O acompanhamento usa somente o contrato público do OS e token privado, sem JWT administrativo. Ver [acompanhamento e entrega](acompanhamento-e-entrega.md), [Saga](saga-pagamento.md) e [verificação do frontend](../operacao/validacao-frontend.md).
