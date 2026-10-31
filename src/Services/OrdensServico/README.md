# OS Service — primeiro incremento

API independente para clientes, veículos, abertura e consulta de ordens. O monólito continua operando com suas rotas e banco existentes. Esta versão não executa diagnóstico, cobrança, reparos ou Saga.

## Executar

A solução `OrdensServico.slnx`, nesta pasta, reúne os sete projetos do serviço e seus testes.

Pré-requisitos: .NET SDK 10, PowerShell 7 e Docker com containers Linux. Antes da primeira publicação NuGet, prepare/baixe o pacote do chassi conforme o [guia de Actions e preparação local](../../../docs/operacao/monorepo-ci.md). Depois, na raiz do repositório:

```powershell
Copy-Item src/Services/OrdensServico/.env.example src/Services/OrdensServico/.env
# Edite Jwt issuer/audience/signing key no .env para corresponder ao emissor administrativo.
docker compose -f src/Services/OrdensServico/compose.yaml up --build -d
```

O Actions gera um `.nupkg` candidato e seleciona a versão em `artifacts/chassis-version.txt`. API, exemplo e monólito usam `PackageReference`; não há referência aos projetos-fonte do chassi. Na preparação local, use uma versão nova ao alterar a biblioteca para evitar cache de pacotes imutáveis.

| Recurso | Endereço |
| --- | --- |
| API | `http://localhost:8081/api/os/v1` |
| Scalar | `http://localhost:8081/scalar/v1` |
| OpenAPI | `http://localhost:8081/openapi/v1.json` |
| Liveness/readiness | `/api/os/v1/health/live` e `/ready` |
| PostgreSQL 18.3 | `localhost:5433`, banco/usuário `fiap_os` |

Migrations são aplicadas na inicialização local pelo Compose. Fora dele, configure `ConnectionStrings__DefaultConnection` e habilite explicitamente `Database__ApplyMigrations=true`, ou execute `dotnet tool restore` e `dotnet ef database update --project src/Services/OrdensServico/FIAP.TechChallenge.OS.Infrastructure` na raiz. A factory de design lê a mesma variável de conexão. A readiness exige conexão e migrations aplicadas; liveness não consulta o banco.

O volume `fiap-os_os_postgres_data` pertence apenas ao serviço. `docker compose -f src/Services/OrdensServico/compose.yaml down` encerra o ambiente preservando seus dados. Não execute `down -v` se quiser conservá-los.

## Autenticação

Endpoints administrativos exigem `Authorization: Bearer <JWT>`. O token é emitido pelo login administrativo existente, `POST /api/usuarios/login` no monólito. Copie os mesmos issuer, audience e chave para o `.env` do OS. O OS valida assinatura e validade sem consultar o banco legado, sem emitir tokens e sem alterar a Lambda atual. A API pode iniciar e atender seus health checks sem o monólito em execução.

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

Listagens usam `pageNumber` e `pageSize`, inicialmente 1 e 10. Cadastros retornam 201 com `Location`, edições 200, exclusões 204, validações 400, recursos inexistentes/inativos 404 e duplicidades de documentos/placas ativos 409. Erros de negócio preservam `{ error, errorCode }`; exceções técnicas usam `ProblemDetails`.

Exemplo de abertura:

```json
{ "clienteId": "<UUID>", "veiculoId": "<UUID>", "descricaoProblema": "Motor com falha" }
```

O veículo precisa pertencer ao cliente e ambos devem estar ativos. A ordem nasce em `Recebida`; ordem e evento `OrdemCriada` são gravados em uma única transação. O histórico contém IDs, tipo, data UTC e correlação, sem depender da leitura de clientes/veículos ativos.

## Edição e soft delete

- Cliente: editar nome, telefone e email; CPF/CNPJ permanecem imutáveis.
- Veículo: editar placa, marca, modelo e ano; proprietário permanece imutável.
- O ID da rota prevalece sobre qualquer ID enviado no corpo.
- DELETE marca `Ativo=false`. Consultas normais ocultam o cadastro; PUT/DELETE posteriores retornam 404.
- Exclusão não se propaga a veículos ou ordens e não apaga referências históricas.
- Documentos e placas de inativos podem ser cadastrados novamente, com novos IDs; índices únicos parciais protegem somente ativos.
- Atualizações preservam `Ativo`; concorrência com inativação não reativa o registro.
- Não há reativação ou exclusão física neste incremento.

## Testes e imagem

```powershell
dotnet test src/Services/OrdensServico/OrdensServico.slnx -c Release --filter 'TestCategory!=ContainerImage'
docker build -f src/Services/OrdensServico/Dockerfile -t fiap-os:local .
$env:OS_TEST_IMAGE = 'fiap-os:local'
dotnet test src/Services/OrdensServico/FIAP.TechChallenge.OS.Integration.Tests/FIAP.TechChallenge.OS.Integration.Tests.csproj -c Release --no-build --filter 'TestCategory=ContainerImage'
```

A suíte usa NUnit, Moq nas unidades e PostgreSQL real/Testcontainers nos testes HTTP, com JWTs realmente assinados. Os cenários Reqnroll estão no projeto de integração. O teste NUnit da imagem também verifica dados após reiniciar a API. No CI, `OS_OPENAPI_OUTPUT` seleciona a exportação da especificação para `artifacts/os-openapi.json`. Containers de teste são descartáveis e não acessam os volumes de desenvolvimento.

Cobertura é medida separadamente para os assemblies `FIAP.TechChallenge.OS.*`, com mínimo de 80%. Consulte [evidências](../../../docs/operacao/evidencias-fase4-incremento1.md), [arquitetura](../../../docs/arquitetura/fase4-incremento1.md) e [CI/configuração externa](../../../docs/operacao/monorepo-ci.md).

## Observabilidade e próximos incrementos

Logs JSON, `X-Correlation-ID`, W3C trace context e health checks vêm do chassi. OTLP é opcional: habilite `OS_ENABLE_OTLP=true` e indique o Collector em `OTEL_EXPORTER_OTLP_ENDPOINT`. Não é necessária uma chave New Relic para executar ou testar localmente.

Permanecem pendentes o deploy AWS/Kubernetes, infraestrutura Terraform do OS, migração do frontend/dados, adaptação da autenticação de clientes, mensageria, Saga, Billing/Mercado Pago, Execution/NoSQL e BDD distribuído. O monorepo deverá ser separado para a entrega final.
