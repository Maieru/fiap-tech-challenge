# Evidências — primeiro incremento da Fase 4

Verificação local em **2026-10-31**, branch **inicio-segregacao-servicos**. Estes resultados não são uma execução remota do GitHub Actions nem uma análise SonarQube Cloud.

## Testes e cobertura

| Componente | Testes aprovados | Linhas cobertas | Cobertas/total |
| --- | ---: | ---: | ---: |
| legacy | 703 | 89.88% | 4121/4585 |
| chassis | 11 | 100.00% | 83/83 |
| os | 208 | 84.53% | 1552/1836 |

Total: **922 testes aprovados**, sem falhas ou ignorados nas execuções registradas. As linhas de cada componente são mescladas pelas localizações de origem; bibliotecas compartilhadas não compensam a cobertura de um serviço. Migrações/código gerado e testes não fazem parte da métrica.

O legado inclui 588 testes originais e 115 cenários HTTP/BDD em PostgreSQL real. Antes da extração, os 588 testes passaram e a coleta inicial conjunta mediu 90,32% (coleta inicial sem o arquivo de exclusões posteriormente padronizado). O timeout anterior do testhost não se repetiu na execução fora da sandbox.

Essas verificações históricas usaram o orquestrador local do primeiro incremento, posteriormente retirado. A organização atual usa as soluções específicas, comandos `dotnet` no Actions e ReportGenerator; consulte o [guia atual](monorepo-ci.md). Os [resumos estruturados](evidencias-fase4-incremento1.json) identificam as pastas locais de TRX/Cobertura/OpenCover. No GitHub, esses arquivos serão publicados pelos artefatos `verification-legacy`, `verification-chassis` e `verification-os`.

## Verificações adicionais

- `dotnet build Backend.slnx -c Release`: 19 projetos, sem avisos ou erros na verificação integrada.
- Pacote 0.1.0 gerado em `artifacts/release`, com biblioteca net10.0, README, licença MIT e símbolos.
- Consumidor sem ProjectReference: build e execução HTTP a partir de PackageReference, com saúde, correlação e logs JSON verificados.
- Imagens `fiap-os:local` e `fiap-legacy:phase4` construídas localmente.
- Validação original da imagem (posteriormente transferida para o teste C# `OsImageSmokeTests`): CRUD, JWT, migrations, soft delete, histórico, especificação OpenAPI e dados preservados após reiniciar a API. Containers e rede temporários removidos pelo próprio teste.
- Migrations do OS aplicadas nos testes; modelo sem alterações pendentes; falha forçada no histórico confirma rollback da ordem.
- Proteção da main consultada/aplicada pela API do GitHub: PR, `ci-required` e administradores obrigatórios, preservando as demais exigências existentes.
- ID NuGet consultado: sem registro público em 2026-10-31. Isso não substitui a confirmação de propriedade/permissão da conta publicadora.
- Sintaxe Python, XML de projetos/soluções, oito documentos YAML (via YamlDotNet) e PowerShell verificados; referências do OS não apontam ao legado.

## Organização posterior da automação — 2026-10-31

- `Chassis.slnx` (3 projetos) e `OrdensServico.slnx` (7 projetos) compiladas sem avisos/erros.
- 11 testes do chassi e 208 testes de OS aprovados usando `dotnet test` nas soluções específicas.
- Teste C# `OsImageSmokeTests` aprovado contra a imagem real, preservando as verificações antes feitas em Python.
- Workflows reorganizados validados com actionlint 1.7.12; metadados YAML das Actions locais também verificados.
- ReportGenerator 5.5.12 aplicado aos relatórios atuais: chassi 100% e OS 89,4% de linhas. O OS inclui seus quatro assemblies de produção. O novo gate rejeitou corretamente um limite de cobertura não atingido em um teste negativo.
- Esses percentuais usam a consolidação do ReportGenerator e não devem ser interpretados como aumento de cobertura em relação ao agregador anterior. Os 703 testes legados da tabela histórica não foram repetidos nesta reorganização; seus relatórios também foram aceitos pelo novo gate de 80%.

Os scripts de orquestração e o CSV de inventário foram retirados. As verificações agora estão nos comandos dos workflows, nas Actions locais e nos projetos NUnit. O inventário por responsabilidade permanece no guia de arquitetura.

Na revisão da organização, `Backend.slnx` passou a agrupar os mesmos 19 projetos por componente, e a solução do Visual Studio recebeu o projeto PostgreSQL legado que faltava. A compilação integrada após os agrupamentos terminou sem avisos/erros; os caminhos e a equivalência entre a solução integrada e a união das três soluções específicas foram verificados. Essa revisão alterou apenas soluções e documentação; as suítes de testes não foram repetidas.

## Pendências externas e de escopo

A adaptação ao modelo Sonar foi validada localmente com actionlint e análise sintática dos passos PowerShell. O comando atualizado executou os 11 testes do chassi e gerou TRX, Cobertura e OpenCover. A conversão de caminhos Linux para o checkout Windows foi exercitada em 14 relatórios OpenCover. Essa validação não executou o envio autenticado ao Sonar nem comprova o cadastro do secret no GitHub.

**NuGet.org:** a conta publicadora ainda não foi criada/configurada. O pacote local não foi publicado. **SonarQube Cloud:** os identificadores informados foram configurados no workflow (`maieru` / `Maieru_fiap-tech-challenge`) e o responsável informou ter criado o token. O cadastro como secret `SONAR_TOKEN`, a análise e o quality gate remoto ainda não foram verificados nesta implementação. Consulte o [passo a passo](monorepo-ci.md).

O código continua na branch local, sem push, merge ou criação de PR nesta implementação. O status de CI remoto só poderá ser comprovado após publicar a branch e executar os workflows.

Ainda fora deste incremento: deploy AWS/Kubernetes do OS, novos recursos Terraform, migração de dados/frontend/autenticação de clientes, Billing/Mercado Pago, Execution/NoSQL, mensageria, Saga e BDD distribuído. O monorepo deverá ser separado para a entrega final.
