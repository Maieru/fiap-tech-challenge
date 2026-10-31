# Actions, soluções e configuração de NuGet/Sonar

## Soluções

| Solução | Conteúdo |
| --- | --- |
| `src/Chassis/Chassis.slnx` | Biblioteca, testes e aplicação de exemplo: 3 projetos |
| `src/Services/OrdensServico/OrdensServico.slnx` | API, Application, Domain, Infrastructure e 3 projetos de testes: 7 projetos |
| `Backend.slnx` | Visão completa do backend do monorepo |
| `Legacy.slnx` | Backend legado e suas suítes de testes |
| `src/TechChallengeFase1.slnx` | Os mesmos projetos legados, com Docker Compose integrado ao Visual Studio |

As soluções podem ser abertas diretamente no Visual Studio. `Backend.slnx` agrupa os projetos em `Legacy`, `Chassis` e `Services/OrdensServico`, com testes em pastas próprias. São agrupamentos visuais que preservam os caminhos físicos usados pelos projetos, Dockerfiles e workflows. A solução original em `src` também inclui a suíte PostgreSQL do legado.

## Como a automação funciona

Build, testes, cobertura, validação do pacote e publicação são executados pelo GitHub Actions, sem arquivos PowerShell/Python auxiliares. Os comandos `dotnet` estão nos workflows; três Actions locais pequenas evitam repetições:

| Action local | Responsabilidade |
| --- | --- |
| `.github/actions/prepare-chassis` | Empacotar uma versão local única e selecionar a versão dos consumidores |
| `.github/actions/coverage` | Consolidar relatórios com ReportGenerator 5.5.12 e exigir 80% de linhas por componente |
| `.github/actions/verify-chassis-package` | Iniciar o exemplo construído por PackageReference e verificar HTTP, correlação e logs JSON |

`monorepo-ci.yml` responde a pushes em `main`, PRs abertos/atualizados/reabertos e execução manual. Possui jobs independentes para legado, chassi e OS, usando suas respectivas soluções em Linux. Os passos de testes usam PowerShell 7, `--configuration Release`, `--no-build`, `--no-restore`, TRX e propagação explícita de `$LASTEXITCODE`. A mesma coleta XPlat produz OpenCover para o Sonar e Cobertura para o gate local. O job OS também constrói a imagem e executa o teste NUnit `OsImageSmokeTests`, categoria `ContainerImage`, contra containers reais. Os cenários de CRUD, JWT, migrations, soft delete, histórico e persistência após reinício foram preservados.

O pacote candidato é disponibilizado no artefato `chassis-candidate`. Ele é um resultado do build, não uma publicação no NuGet.org. Após os testes, o CI chama o workflow reutilizável `sonarqube.yml`, sem disparar uma segunda análise independente. Ele usa `windows-latest`, .NET 10, Java 21 Zulu, checkout completo e caches de pacotes Sonar/scanner. O scanner é instalado na versão de `.config/dotnet-tools.json`; o hash do manifesto invalida seu cache quando a versão muda.

O Sonar baixa o mesmo pacote candidato e os relatórios OpenCover gerados pelos jobs Linux, adapta os caminhos dos arquivos-fonte ao checkout Windows, compila `Backend.slnx` entre `begin`/`end` e aguarda o quality gate. Não repete os testes nem exige Docker no Windows. O check agregado `ci-required` só aprova se testes, cobertura e Sonar aprovarem.

O orquestrador AWS mantém seus filtros de caminhos. Mudanças exclusivas de chassi/OS não iniciam provisionamento remoto; o novo serviço continua fora do deploy AWS.

## O que configurar no GitHub

Abra **Settings → Secrets and variables → Actions** no repositório `Maieru/fiap-tech-challenge`.

| Tipo | Nome | Valor |
| --- | --- | --- |
| Repository variable | `NUGET_USER` | Nome do perfil no NuGet.org, sem email |
| Repository secret | `SONAR_TOKEN` | Token de análise gerado pelo Sonar |

O token deve ser cadastrado no GitHub Secret, nunca em arquivo ou no chat. Os identificadores públicos já estão definidos no workflow Sonar: organização `maieru` e projeto `Maieru_fiap-tech-challenge`. Não é necessário cadastrar Repository variables para essas duas chaves. `NUGET_USER` continua sendo uma variável necessária para publicar o pacote.

## NuGet.org: criação da conta e Trusted Publishing

1. Entre em [NuGet.org](https://www.nuget.org/), crie seu perfil e confirme o email solicitado. Anote o nome do perfil para `NUGET_USER`.
2. No menu do perfil, abra **Trusted Publishing** e crie uma política GitHub com os campos abaixo.

| Campo da política | Valor |
| --- | --- |
| Repository owner | `Maieru` |
| Repository | `fiap-tech-challenge` |
| Workflow file | `publish-chassis.yml` — somente o nome, sem `.github/workflows/` |
| Environment | Deixar vazio; o workflow não declara environment |
| Pacote/escopo | `Maieru.FiapTechChallenge.Chassis`, permitindo criar/publicar o pacote |

3. Cadastre `NUGET_USER` no GitHub conforme a tabela anterior e confirme a disponibilidade/propriedade do ID do pacote.
4. Depois de o PR passar no CI e ser integrado à main, publique uma tag semântica, como `chassis-v0.1.0`, no commit revisado. O workflow de publicação será acionado.

O workflow valida tag e ancestralidade em main, empacota, testa os três projetos do chassi, verifica o consumidor real e só então chama `NuGet/login@v1`. O GitHub OIDC é trocado por uma chave temporária: não é necessário criar ou armazenar API key permanente. A versão publicada é imutável; o fluxo não esconde duplicidades com `--skip-duplicate`. [Documentação oficial](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

Após a primeira publicação, confirmar o pacote no NuGet.org e o restore público em ambiente limpo, sem o feed local.

## SonarQube Cloud: criação e importação

1. Entre no [SonarQube Cloud](https://sonarcloud.io/) com GitHub e autorize acesso ao repositório.
2. Crie/importe a organização e importe `Maieru/fiap-tech-challenge` como projeto público. Selecione o plano apropriado para projetos públicos.
3. Escolha análise por **GitHub Actions/CI** para **.NET**. Desabilite a análise automática concorrente, se estiver habilitada.
4. Confira os identificadores já configurados em `sonarqube.yml`: **Organization key** `maieru` e **Project key** `Maieru_fiap-tech-challenge`.
5. O token já foi criado pelo responsável. Salve-o no GitHub como **Repository secret `SONAR_TOKEN`**, com permissão de análise desse projeto, caso esse cadastro ainda não tenha sido feito.
6. Publique a branch/abra o PR e acompanhe **Monorepo CI**. É possível executar manualmente o workflow quando ele estiver disponível na branch padrão. Verifique o resultado no Sonar e no check `ci-required`.

Não é necessário adicionar outro workflow gerado pelo assistente Sonar: o repositório já usa SonarScanner for .NET e envia cobertura OpenCover. [Integração oficial .NET](https://docs.sonarsource.com/sonarqube-cloud/analyzing-source-code/scanners/sonarscanner-for-dotnet/using).

Sem `SONAR_TOKEN`, o job falha com uma mensagem de configuração; identificadores ou permissões inválidos também impedem a análise. PRs de forks não recebem secrets; a análise precisa ocorrer em branch confiável antes do merge. A main já exige PR e `ci-required`, inclusive para administradores; por isso o token precisa estar cadastrado para o CI obrigatório aprovar.

## Comandos locais, quando necessários

Publicação e verificações de CI ficam no Actions. Para desenvolvimento local antes da primeira publicação pública, prepare um pacote sem script auxiliar, na raiz do repositório, usando PowerShell 7:

```powershell
$version = '0.1.0-local.' + (Get-Date -Format yyyyMMddHHmmss)
New-Item -ItemType Directory -Force artifacts/packages | Out-Null
dotnet pack src/Chassis/Maieru.FiapTechChallenge.Chassis/Maieru.FiapTechChallenge.Chassis.csproj -c Release -o artifacts/packages -p:Version=$version
Set-Content artifacts/chassis-version.txt $version -Encoding utf8NoBOM
```

Outra opção é baixar o artefato `chassis-candidate` de uma execução aprovada do job do chassi e colocar os pacotes em `artifacts/packages` e o arquivo de versão em `artifacts/chassis-version.txt`. `NuGet.Config` já conhece o feed local. Consumidores continuam usando PackageReference, sem referência ao projeto da biblioteca.

```powershell
dotnet build src/Chassis/Chassis.slnx -c Release
dotnet test src/Chassis/Chassis.slnx -c Release --no-build
dotnet build src/Services/OrdensServico/OrdensServico.slnx -c Release
dotnet test src/Services/OrdensServico/OrdensServico.slnx -c Release --no-build --filter 'TestCategory!=ContainerImage'
```

Os testes OS exigem Docker. Para selecionar a verificação da imagem explicitamente:

```powershell
docker build -f src/Services/OrdensServico/Dockerfile -t fiap-os:local .
$env:OS_TEST_IMAGE = 'fiap-os:local'
dotnet test src/Services/OrdensServico/FIAP.TechChallenge.OS.Integration.Tests/FIAP.TechChallenge.OS.Integration.Tests.csproj -c Release --no-build --filter 'TestCategory=ContainerImage'
```

O gate de 80% usa ReportGenerator, que consolida os relatórios Cobertura. A métrica pode variar em relação ao agregador Python do primeiro incremento; isso é uma mudança de ferramenta de consolidação, não aumento automático da cobertura. A definição continua sendo cobertura de linhas por componente, excluindo testes e código gerado.

A política de proteção permanece documentada em `.github/branch-protection.json`; o script de configuração foi retirado porque a proteção já foi aplicada. Alterações posteriores podem ser feitas em GitHub → Settings → Branches.
