# CI do monorepo e dependências

Este repositório contém o legado e o OS Service. O [chassi técnico](https://github.com/Maieru/fiap-tech-challenge-chassis) possui biblioteca, testes, exemplo, CI/Sonar e publicação NuGet em repositório próprio.

## Soluções

| Solução | Conteúdo |
| --- | --- |
| `Backend.slnx` | Legado e OS: 18 projetos, agrupados por componente |
| `Legacy.slnx` | Backend legado e testes: 9 projetos |
| `src/Services/OrdensServico/OrdensServico.slnx` | OS, contratos de mensageria e testes: 9 projetos |
| `src/TechChallengeFase1.slnx` | Projetos legados e integração Docker Compose do Visual Studio |

## Dependências entre repositórios

O legado fixa `Maieru.FiapTechChallenge.Chassis` **0.1.0**. O OS usa chassi **1.1.1**, adaptador `Maieru.FiapTechChallenge.Messaging.Kafka` **0.1.1**, contratos Execution **0.1.0** e contratos Billing **0.1.0** nos respectivos `PackageReference`. Billing consome contratos OS **0.2.0**; Execution permanece em OS **0.1.0**, cujos tipos continuam compatíveis.

Os contratos são distribuídos por produtor, sem referência ao código-fonte de outro repositório. Consulta ao NuGet em 2026-11-02 confirmou OS 0.2.0 e Execution 0.1.0 disponíveis; o índice Billing ainda retornou 404. O [estado da migração](../arquitetura/estado-da-migracao.md#pacotes-e-builds-reproduzíveis) registra a consulta e as evidências. Build local aprovado com pacote candidato não comprova restore em runner limpo usando somente NuGet.org.

O erro `NU1102` do Billing por ausência de OS 0.2.0 deve ser revalidado após a disponibilidade dessa versão no feed. Para o build OS, a dependência ainda pendente de publicação é Billing Messaging 0.1.0. Reexecute o pipeline consumidor após a publicação e indexação; não reduza o `PackageReference` para uma versão que não contém o contrato necessário.

### Publicação por produtor

| Pacote | Repositório e workflow | Tag da versão necessária |
| --- | --- | --- |
| OS Messaging | `fiap-tech-challenge`, `publish-os-messaging.yml` | `os-messaging-v0.2.0`, versão disponível no feed |
| Billing Messaging | `fiap-tech-challenge-billing`, `publish-messaging.yml` | `messaging-v0.1.0`, ainda pendente na consulta registrada |
| Execution Messaging | `fiap-tech-challenge-execution`, `publish-messaging.yml` | `messaging-v0.1.0`, versão disponível no feed |

Os workflows de publicação validam a tag e exigem commit já integrado à `origin/main`, `NUGET_USER` no repositório produtor e política NuGet Trusted Publishing correspondente. A tag do pacote é independente da versão 1 do envelope Kafka. Não reaproveite uma versão publicada para conteúdo diferente. A configuração de publicação do chassi fica em seu próprio repositório; publicar contratos OS não publica Billing automaticamente.

### Candidatos locais antes da publicação

Na raiz OS, com o checkout Billing ao lado e .NET SDK 10:

```powershell
dotnet pack ../FiapTechChallengeBilling/src/FIAP.TechChallenge.Billing.Application.Messaging -c Release -o artifacts/packages
./scripts/Restore-BillingLocal.ps1 -BillingMessagingPackage artifacts/packages/FIAP.TechChallenge.Billing.Application.Messaging.0.1.0.nupkg
```

Esse roteiro valida o pacote e restaura o OS com `artifacts/NuGet.Local.Config`, contendo o feed local explícito e NuGet.org. O Dockerfile OS também aceita candidatos em `artifacts/packages`. Depois da publicação, validar `dotnet restore` sem esse feed e em diretório de pacotes limpo antes de considerar o CI desbloqueado.

Para um contrato Execution candidato, permanece disponível `scripts/Restore-OsLocal.ps1 -ExecutionMessagingPackage <arquivo>`. No Billing, `scripts/Restore-Local.ps1 -OsMessagingPackage <arquivo>` permite validar um candidato OS; a versão OS 0.2.0 agora pode ser restaurada do feed público. Esses roteiros não são publicação e não configuram automaticamente os runners remotos.

## Como a automação funciona

`monorepo-ci.yml` responde a pushes em `main`, PRs abertos/atualizados/reabertos e execução manual. Os jobs Linux verificam legado e OS separadamente. Os passos PowerShell usam build prévio, `--no-build`, `--no-restore`, resultados TRX e propagação de `$LASTEXITCODE`. Uma coleta XPlat gera Cobertura e OpenCover.

A Action local `.github/actions/coverage` usa ReportGenerator 5.5.12 e exige pelo menos 80% de cobertura de linhas em cada componente, sem compensação entre eles. Os artefatos `verification-legacy` e `verification-os` contêm os resultados. O job OS também constrói a imagem e executa `OsImageSmokeTests` contra containers reais.

Após os testes, `sonarqube.yml` usa Windows, .NET 10, Java 21 Zulu, checkout completo e caches. A versão do scanner está fixada em `.config/dotnet-tools.json`. Ele importa os relatórios OpenCover dos jobs Linux, ajusta os caminhos ao checkout Windows, compila `Backend.slnx` entre `begin`/`end` e aguarda o quality gate. O pacote do chassi é restaurado do NuGet.org, como qualquer outra dependência.

O check agregado `ci-required` exige sucesso dos testes, cobertura e Sonar. Os filtros do orquestrador AWS continuam limitados ao legado e seu deploy; alterações exclusivas do OS não provisionam AWS.

## SonarQube Cloud do principal

Os identificadores públicos já estão em `sonarqube.yml`: organização `maieru` e projeto `Maieru_fiap-tech-challenge`. Em **Settings → Secrets and variables → Actions**, cadastre o **Repository secret `SONAR_TOKEN`** com permissão de análise desse projeto. Não envie o token ao chat nem o versione.

No Sonar, use análise por CI para .NET e desabilite a análise automática concorrente, se habilitada. Não adicione outro workflow gerado pelo assistente: a integração já existe. [Documentação oficial](https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/scanners/sonarscanner-for-dotnet/using).

O novo repositório do chassi exige suas próprias configurações Sonar. Secrets e proteções de branch não são transferidos automaticamente entre repositórios.

Sem token ou com permissões inválidas, a análise falha. PRs de forks não recebem secrets e precisam ser validados em branch confiável antes do merge. A main do principal exige PR e `ci-required`, inclusive para administradores; a política está registrada em `.github/branch-protection.json`.

## Comandos locais

Requer .NET SDK 10, dependências NuGet e Docker para os testes PostgreSQL/Kafka/HTTP:

```powershell
dotnet build Backend.slnx -c Release
dotnet test Backend.slnx -c Release --no-build --no-restore --filter 'TestCategory!=ContainerImage'

# Ou somente o OS
dotnet test src/Services/OrdensServico/OrdensServico.slnx -c Release --filter 'TestCategory!=ContainerImage'
```

Para verificar a imagem separadamente:

```powershell
docker build -f src/Services/OrdensServico/Dockerfile -t fiap-os:local .
$env:OS_TEST_IMAGE = 'fiap-os:local'
dotnet test src/Services/OrdensServico/FIAP.TechChallenge.OS.Integration.Tests/FIAP.TechChallenge.OS.Integration.Tests.csproj -c Release --no-build --no-restore --filter 'TestCategory=ContainerImage'
```

Os relatórios do primeiro incremento são históricos; a mudança do agregador de cobertura para ReportGenerator pode alterar percentuais sem representar aumento de cobertura. Consulte as [evidências e limitações](evidencias-fase4-incremento1.md) e a [ADR da separação do chassi](../ADRs/ADR-013-chassi-em-repositorio-proprio.md).
