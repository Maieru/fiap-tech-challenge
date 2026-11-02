# RFC-012 — APIs de entrega e acompanhamento privado no OS

- Status: Aceita
- Data: 2026-11-02
- Última revisão: 2026-11-02

## Contexto e decisão

Materializa a [ADR-019](../ADRs/ADR-019-entrega-acesso-cliente.md) preservando a separação em API, Application, Domain e Infrastructure. Cada operação tem pasta própria de caso de uso.

- `PUT /api/os/v1/ordens/{id}/entregar`: JWT, 200 com status e data; 400 antes da finalização; 404 inexistente. Repetição preserva a primeira data e o histórico único.
- `POST /api/os/v1/ordens/{id}/acesso-cliente`: JWT, 200 com token e URL relativa; nova emissão revoga o link anterior.
- `GET /api/os/v1/ordens/acompanhamento/{id}?token=...`: consulta privada sem JWT, 200; acesso inválido 401.
- `PUT /api/os/v1/ordens/acompanhamento/{id}/aprovar-execucao?token=...`: 202, mesma Saga financeira.
- `PUT /api/os/v1/ordens/acompanhamento/{id}/cancelar?token=...`: 202 antes da aprovação; decisão contrária já registrada retorna 409.

`IAcessoClienteService` separa emissão/validação da implementação SHA-256. Geração usa `RandomNumberGenerator` (32 bytes); comparação é em tempo constante. A tabela `AcessosCliente` possui uma linha por ordem, FK e hash hexadecimal de 64 caracteres. Plaintext não é persistido. Não há expiração automática; o administrador revoga o link emitindo outro.

Consultas e ações do cliente abrem transação e bloqueiam a ordem com `FOR UPDATE`, também usado pela emissão. Assim, rotação, decisões administrativas, decisões do cliente e eventos concorrentes são serializados. Uma requisição já concluída antes da rotação conserva seu efeito; a troca não desfaz decisões aceitas. Token ausente nunca pode usar o caminho administrativo sem JWT.

A migration adiciona `DataEntrega`, `OrcamentoJson`, tabela de acessos e status 6 à restrição do banco. É aditiva para dados existentes; registros antigos mantêm os campos nulos. A projeção dos itens valida quantidade, IDs, descrição, valores e total decimal exato; nenhum arredondamento é introduzido. Reentregas não alteram snapshots já registrados.

## Segurança e operação

Respostas do acompanhamento e emissão usam `Cache-Control: no-store` e `Referrer-Policy: no-referrer`. O log padrão de início/fim de requisição do ASP.NET Hosting foi reduzido a Warning, pois inclui query string. O middleware de correlação do chassi registra apenas o caminho. A infraestrutura que expuser essas APIs deve omitir query strings de logs e usar HTTPS. Links privados não devem ser copiados para logs, analytics ou documentação de evidências.

Cadastro/login administrativos permanecem como definidos na ADR-018. O escopo original deste incremento era a API, sem frontend, envio automático do link, integração AWS/Lambda ou alteração dos pacotes de mensageria. A adaptação posterior das telas está na [RFC-013](RFC-013-proxy-frontend-microsservicos.md); envio automático e implantação AWS continuam fora dessa entrega.

## Alternativas e validação

Foram descartados acesso apenas por CPF e consultas síncronas ao Execution pelas razões da ADR. Não é necessário framework de Saga ou novo contrato de entrega. Testes de domínio cobrem estado terminal e orçamento; testes reais de HTTP/PostgreSQL/Kafka verificam permissões, rotação, concorrência de entrega e publicação única; cenários distribuídos verificam estoque e pagamento. O limite transacional banco/Kafka permanece o documentado na Saga.
