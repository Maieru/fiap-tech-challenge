# Monorepo: build, verificação e publicação

## Preparação local

Requer .NET SDK 10, PowerShell 7, Python 3.10+ e Docker com containers Linux. Na raiz do repositório:

```powershell
pwsh -File scripts/bootstrap.ps1
dotnet build Backend.slnx -c Release
pwsh -File scripts/test-component.ps1 -Component chassis
pwsh -File scripts/test-component.ps1 -Component os
pwsh -File scripts/test-component.ps1 -Component legacy
```

Se Python não estiver no PATH, passe `-Python <caminho-do-python>` ao script de testes. O bootstrap deve anteceder restores/builds dos consumidores enquanto o pacote público não estiver publicado. `NuGet.Config` inclui o feed local e nuget.org; somente o ID do chassi é elegível no feed local.

`Backend.slnx` reúne os 19 projetos de backend, sem o projeto Docker Compose do Visual Studio. `Legacy.slnx` isola o legado para seus workflows existentes. A solução original em `src` continua disponível para o Visual Studio.

O bootstrap gera versão local única para evitar cache de pacotes imutáveis e escreve a seleção em `artifacts/chassis-version.txt`. Para testar uma versão específica:

```powershell
pwsh -File scripts/bootstrap.ps1 -Version 0.1.0-ci.123.1
pwsh -File scripts/test-component.ps1 -Component os -ChassisVersion 0.1.0-ci.123.1
```

Resultados de cada execução ficam em uma pasta única de `artifacts/verification/<componente>`, contendo TRX, Cobertura, OpenCover e `summary.json`. Relatórios antigos não entram no gate de uma nova execução. O script Python une hits pela localização de origem e não soma linhas repetidas entre suítes.

## CI independente por componente

`monorepo-ci.yml` executa três jobs independentes: legado, chassi e OS. O legado inclui as suítes existentes e os mesmos cenários HTTP/BDD contra PostgreSQL real; o OS inclui unidades e integração/BDD; o chassi inclui biblioteca e consumidor empacotado. Cada job impõe 80% de linhas no seu componente.

O job OS constrói a imagem e executa `scripts/smoke_os_image.py`, validando JWT, CRUD, soft delete, migrations, histórico, OpenAPI e persistência após reinício. O chassi inicia uma aplicação consumidora real e verifica HTTP, correlação e logs JSON. Todos os testes Docker usam recursos próprios e descartáveis.

Após os testes, o job Sonar compila `Backend.slnx`, envia os relatórios OpenCover e aguarda o quality gate. O check agregado `ci-required` só aprova se componentes e Sonar aprovarem. Nenhuma análise ausente é apresentada como sucesso.

O orquestrador AWS existente recebeu filtros de caminhos: alterações exclusivas em `src/Chassis`, `src/Services`, seus testes ou documentação não iniciam o provisionamento remoto. Mudanças nas aplicações legadas/manifests/workflows de deploy continuam usando seu fluxo existente. Ambos os jobs do build de imagem legado preparam o pacote local; o contexto Docker passou a ser a raiz do repositório.

## Configurar SonarQube Cloud — pendência externa

O projeto não possuía conta/projeto Sonar no início desta implementação. Para ativar a análise:

1. Criar a conta e importar o repositório público `Maieru/fiap-tech-challenge` para uma organização SonarQube Cloud.
2. Selecionar análise por CI para .NET; desabilitar análise automática concorrente.
3. No GitHub, cadastrar as Variables `SONAR_ORGANIZATION` e `SONAR_PROJECT_KEY` com os valores fornecidos pelo Sonar.
4. Gerar o token de análise no Sonar e cadastrá-lo somente no GitHub Secret `SONAR_TOKEN`.
5. Executar o workflow e verificar o quality gate. A cobertura mínima por componente também é imposta localmente, independentemente do percentual global do Sonar.

Sem essas configurações, o job de qualidade falha com uma instrução explícita. PRs de forks não recebem secrets; a análise precisa ser executada em uma branch confiável do repositório antes do merge. Não usar `pull_request_target` para executar código de contribuições com secrets.

## Publicar o pacote — pendência externa

A conta NuGet ainda precisa ser criada. O pacote gerado e consumido localmente não é uma publicação pública.

1. Criar e confirmar uma conta no NuGet.org; confirmar disponibilidade e propriedade do ID `Maieru.FiapTechChallenge.Chassis`.
2. Em Trusted Publishing, criar política para owner `Maieru`, repository `fiap-tech-challenge`, workflow **`publish-chassis.yml`**, sem environment. Autorizar esse ID na política.
3. No GitHub, cadastrar a Variable `NUGET_USER` com o nome do perfil NuGet, não o email. Não cadastrar uma API key permanente.
4. Após merge aprovado na main, criar uma tag semântica, por exemplo `chassis-v0.1.0`, apontando ao commit revisado, e publicar a tag.
5. O workflow valida a tag e sua ancestralidade em main, empacota a versão exata, testa biblioteca/consumidor e usa `NuGet/login@v1` para obter uma chave OIDC temporária.
6. Confirmar a versão no NuGet.org e testar restore público em um diretório limpo, sem feed local. Alterar o padrão `ChassisVersion` dos consumidores somente por PR revisado.

O workflow não sobrescreve versões e não usa `--skip-duplicate` para esconder conflitos. Símbolos ficam no artefato da release, além do pacote. [Trusted Publishing oficial](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

## Proteção da main

Política revisável em `.github/branch-protection.json`: exigir PR, branch atualizada e `ci-required`, incluir administradores e bloquear force push/exclusão. Não exigir outro aprovador obrigatório nesta etapa; o autor continua podendo trabalhar sozinho, mantendo PR e checks.

```powershell
python scripts/configure_branch_protection.py
python scripts/configure_branch_protection.py --apply
```

O script usa `GH_TOKEN` ou Git Credential Manager não interativo. Não imprime credenciais e preserva proteções mais fortes existentes. Se a autenticação não tiver permissão administrativa, aplicar a mesma política em GitHub → Settings → Branches. A política de referência, por si só, não comprova a configuração remota; consultar o estado pelo script ou pela interface do GitHub.

Antes de uma release pública, concluir a configuração Sonar para que o check obrigatório possa aprovar. Deploy do novo OS, Terraform e migração do tráfego continuam fora deste incremento.
