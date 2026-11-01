# OS Service — integração com Execution

API independente para clientes, veículos, abertura, decisão administrativa e consulta de ordens.
Publica as entradas do Execution e projeta suas confirmações por Kafka. O monólito continua com suas rotas e banco existentes.

## Executar

A solução `OrdensServico.slnx`, nesta pasta, reúne os nove projetos do serviço, contratos e testes.

## Contratos de mensageria

O projeto [Application.Messaging](FIAP.TechChallenge.OS.Application.Messaging/README.md) publica os contratos
OrdemServicoCriada, ExecucaoAutorizada e CancelamentoSolicitado. Ele não tem dependências de negócio ou Kafka.
O OS injeta IMessagingService nos casos de uso e recebe os cinco eventos Execution em BackgroundService com switch explícito.
Consulte a [RFC-007](../../../docs/RFCs/RFC-007-contratos-mensageria-kafka.md) e o
[padrão de mensageria](../../../docs/arquitetura/mensageria.md).

Para validar e empacotar, na raiz do repositório:

```powershell
dotnet test src/Services/OrdensServico/FIAP.TechChallenge.OS.Application.Messaging.Tests -c Release
dotnet pack src/Services/OrdensServico/FIAP.TechChallenge.OS.Application.Messaging -c Release -o artifacts/packages
```

Publicação: tag `os-messaging-v0.1.0` em commit revisado de main, variável `NUGET_USER` e
NuGet Trusted Publishing para owner Maieru, repositório fiap-tech-challenge,
workflow publish-os-messaging.yml, environment vazio e pacote FIAP.TechChallenge.OS.Application.Messaging.
O pacote OS.Application.Messaging 0.1.0 já está publicado no NuGet.

## Pré-requisitos locais

Pré-requisitos: .NET SDK 10, PowerShell 7 e Docker com containers Linux.
Versões fixadas: chassi 1.1.1, adaptador Kafka 0.1.1 e contrato Execution.Application.Messaging 0.1.0.
Essas dependências estão publicadas no NuGet. Na raiz deste repositório:

```powershell
dotnet restore src/Services/OrdensServico/OrdensServico.slnx
dotnet build src/Services/OrdensServico/OrdensServico.slnx -c Release --no-restore
```

Para testar um pacote de contratos gerado localmente, opcionalmente use `scripts/Restore-OsLocal.ps1 -ExecutionMessagingPackage <caminho-do-nupkg>`.
O roteiro valida o pacote e gera artifacts/NuGet.Local.Config ignorado pelo Git; o Dockerfile também aceita artifacts/packages/*.nupkg.
O restore padrão e o CI usam o feed público, sem compilar outro checkout.
Na raiz do repositório:

```powershell
Copy-Item src/Services/OrdensServico/.env.example src/Services/OrdensServico/.env
# Edite Jwt issuer/audience/signing key no .env para corresponder ao emissor administrativo.
docker compose -f src/Services/OrdensServico/compose.yaml up --build -d
```

O chassi é mantido em [repositório próprio](https://github.com/Maieru/fiap-tech-challenge-chassis) e restaurado como pacote público.
Para integrar as duas APIs, use compose.yaml + compose.integrated.yaml no Execution com uma imagem OS pronta,
conforme o README daquele repositório. O Compose desta pasta usa broker próprio para trabalho isolado.

| Recurso | Endereço |
| --- | --- |
| API | `http://localhost:8081/api/os/v1` |
| Scalar | `http://localhost:8081/scalar/v1` |
| OpenAPI | `http://localhost:8081/openapi/v1.json` |
| Liveness/readiness | `/api/os/v1/health/live` e `/ready` |
| PostgreSQL 18.3 | `localhost:5433`, banco/usuário `fiap_os` |
| Kafka isolado | `127.0.0.1:19094`; dentro do Compose, `kafka:9092` |

Migrations são aplicadas na inicialização local pelo Compose. Fora dele, configure ConnectionStrings__DefaultConnection,
Kafka__BootstrapServers e JWT, habilitando Database__ApplyMigrations=true, ou aplique migrations com dotnet ef.
A readiness exige PostgreSQL, migrations, consumidor ativo e Kafka; liveness não consulta dependências.

O volume `fiap-os_os_postgres_data` pertence apenas ao serviço. `docker compose -f src/Services/OrdensServico/compose.yaml down` encerra o ambiente preservando seus dados. Não execute `down -v` se quiser conservá-los.

## Autenticação

Endpoints administrativos exigem `Authorization: Bearer <JWT>`. O token é emitido pelo login administrativo existente, `POST /api/usuarios/login` no monólito. Copie os mesmos issuer, audience e chave para o `.env` do OS. O OS valida assinatura e validade sem consultar o banco legado, sem emitir tokens e sem alterar a Lambda atual. A API pode iniciar e atender seus health checks sem o monólito em execução.

Issuer e audience usam a seção Jwt de IConfiguration. A chave simétrica vem explicitamente de Jwt__SigningKey,
com pelo menos 32 bytes UTF-8, e é registrada como singleton e injetada nas opções de validação JWT.
No Compose, JWT_SIGNING_KEY é mapeada para Jwt__SigningKey; não há fallback da chave para arquivo de configuração.
O OS e o Execution usam a mesma chave do emissor. Provisione segredos externamente, sem versionar seu valor.

Para uma demonstração completamente isolada, `OsImageSmokeTests` cria credenciais temporárias de teste, assina um JWT real e exercita o container. Não fornece autenticação alternativa à aplicação.

## Contratos

| Rota | Operações |
| --- | --- |
| `/api/os/v1/clientes` | POST e GET; filtros exclusivos `cpf` ou `cnpj` |
| `/api/os/v1/clientes/{id}` | GET, PUT e DELETE |
| `/api/os/v1/veiculos` | POST e GET; filtros exclusivos `placa` ou `clienteId` |
| `/api/os/v1/veiculos/{id}` | GET, PUT e DELETE |
| `/api/os/v1/ordens` | POST e GET; filtros `clienteId`, `veiculoId`, `status`, ordenação e paginação |
| `/api/os/v1/ordens/{id}` | GET |
| `/api/os/v1/ordens/{id}/historico` | GET |
| `/api/os/v1/ordens/{id}/aprovar-execucao` | PUT |
| `/api/os/v1/ordens/{id}/cancelar` | PUT |

Listagens usam `pageNumber` e `pageSize`, inicialmente 1 e 10. Cadastros retornam 201 com `Location`, edições 200, exclusões 204, validações 400, recursos inexistentes/inativos 404 e duplicidades de documentos/placas ativos 409. Erros de negócio preservam `{ error, errorCode }`; exceções técnicas usam `ProblemDetails`.

Exemplo de abertura:

```json
{ "clienteId": "<UUID>", "veiculoId": "<UUID>", "descricaoProblema": "Motor com falha" }
```

O veículo precisa pertencer ao cliente e ambos devem estar ativos. A ordem nasce Recebida;
o caso de uso salva ordem/histórico, publica OrdemServicoCriada e faz commit local.
Falha na publicação aborta os dados; falha posterior de commit pode deixar mensagem órfã, conforme o padrão aprovado.
O histórico preserva IDs, tipo, data UTC e correlação, sem depender de cadastros ativos.

Aprovar/cancelar só inicia uma decisão enquanto AguardandoAprovacao e retorna 202 com decisao e pendente.
O status só avança após confirmação do Execution. Repetir a mesma decisão não republica; a oposta retorna 409.
Consultas retornam status/datas projetados e decisão. Itens, valores e estoque são consultados no Execution.

## Edição e soft delete

- Cliente: editar nome, telefone e email; CPF/CNPJ permanecem imutáveis.
- Veículo: editar placa, marca, modelo e ano; proprietário permanece imutável.
- O ID da rota prevalece sobre qualquer ID enviado no corpo.
- DELETE marca `Ativo=false`. Consultas normais ocultam o cadastro; PUT/DELETE posteriores retornam 404.
- Exclusão não se propaga a veículos ou ordens e não apaga referências históricas.
- Documentos e placas de inativos podem ser cadastrados novamente, com novos IDs; índices únicos parciais protegem somente ativos.
- Atualizações preservam `Ativo`; concorrência com inativação não reativa o registro.
- Não há reativação ou exclusão física.

## Testes e imagem

```powershell
dotnet test src/Services/OrdensServico/OrdensServico.slnx -c Release --filter 'TestCategory!=ContainerImage'
docker build -f src/Services/OrdensServico/Dockerfile -t fiap-os:local .
$env:OS_TEST_IMAGE = 'fiap-os:local'
dotnet test src/Services/OrdensServico/FIAP.TechChallenge.OS.Integration.Tests/FIAP.TechChallenge.OS.Integration.Tests.csproj -c Release --no-build --filter 'TestCategory=ContainerImage'
```

A suíte usa NUnit, Moq nas unidades, PostgreSQL e Kafka reais/Testcontainers nos testes HTTP, com JWTs assinados.
Os cenários Reqnroll locais estão na integração; o BDD das duas imagens fica em Distributed.slnx no Execution.
Há testes de concorrência, deduplicação, rollback e mensagem órfã após falha no commit PostgreSQL.
O teste da imagem verifica reinício, OpenAPI e encerramento com offset pendente diante de mensagem inválida.
No CI, OS_OPENAPI_OUTPUT seleciona a exportação. Containers são descartáveis e não usam volumes de desenvolvimento.

Cobertura é medida separadamente para os assemblies `FIAP.TechChallenge.OS.*`, com mínimo de 80%. Consulte a [integração OS/Execution](../../../docs/arquitetura/execution.md) e o [guia de CI/configuração externa](../../../docs/operacao/monorepo-ci.md).

## Observabilidade e estado da migração

Logs JSON, `X-Correlation-ID`, W3C trace context e health checks vêm do chassi. OTLP é opcional: habilite `OS_ENABLE_OTLP=true` e indique o Collector em `OTEL_EXPORTER_OTLP_ENDPOINT`. Não é necessária uma chave New Relic para executar ou testar localmente.

A integração Kafka, o Execution/MongoDB e o BDD distribuído estão implementados:
[ADR-016](../../../docs/ADRs/ADR-016-integracao-os-execution.md),
[RFC-009](../../../docs/RFCs/RFC-009-integracao-os-execution.md) e
[evidências](../../../docs/operacao/revisao-execution-etapa3.md).
O [estado da migração](../../../docs/arquitetura/estado-da-migracao.md) reúne as pendências de Billing/pagamentos,
autenticação, frontend, infraestrutura e retirada do legado. Não há requisito de importar o histórico legado para os novos bancos de demonstração.
