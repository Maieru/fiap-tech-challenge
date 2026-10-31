# CI do monorepo e dependências

Este repositório contém o legado e o OS Service. O [chassi técnico](https://github.com/Maieru/fiap-tech-challenge-chassis) possui biblioteca, testes, exemplo, CI/Sonar e publicação NuGet em repositório próprio.

## Soluções

| Solução | Conteúdo |
| --- | --- |
| `Backend.slnx` | Legado e OS: 16 projetos, agrupados por componente |
| `Legacy.slnx` | Backend legado e testes: 9 projetos |
| `src/Services/OrdensServico/OrdensServico.slnx` | OS e testes: 7 projetos |
| `src/TechChallengeFase1.slnx` | Projetos legados e integração Docker Compose do Visual Studio |

## Dependência do chassi

Legado e OS fixam `Maieru.FiapTechChallenge.Chassis` **0.1.0** diretamente nos respectivos `PackageReference`. Build, CI e Docker restauram a versão pública no NuGet.org. Não há projeto-fonte, feed temporário, arquivo de seleção de versão ou publicação do chassi neste repositório.

A primeira publicação de 0.1.0 no novo repositório é pré-requisito para restore e imagens em ambiente limpo. O cache local pode conter versões antigas e não comprova disponibilidade pública. Para atualizar o chassi, publique uma versão imutável no repositório responsável e altere explicitamente o `PackageReference` dos consumidores desejados.

A configuração `NUGET_USER`, a política Trusted Publishing e a tag `vX.Y.Z` pertencem somente a `Maieru/fiap-tech-challenge-chassis`. Consulte seu README para publicação e desenvolvimento da biblioteca.

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

Requer .NET SDK 10, pacote público do chassi disponível e Docker para os testes PostgreSQL/HTTP:

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
