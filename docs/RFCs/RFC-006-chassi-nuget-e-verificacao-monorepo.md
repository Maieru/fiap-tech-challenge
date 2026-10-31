# RFC-006 — Chassi NuGet e verificação independente no monorepo

- **Status:** Aceita
- **Data:** 2026-10-31
- **Escopo:** infraestrutura de aplicação e qualidade do primeiro incremento

## Contexto

Os serviços precisam de observabilidade, correlação, tratamento de falhas e probes consistentes. A equipe escolheu distribuir esses recursos como pacote NuGet público, preservando a propriedade local das regras de negócio. A base anterior já tinha NUnit, Coverlet e testes HTTP, mas usava EF InMemory e não impunha cobertura mínima no CI.

## Alternativas consideradas

- Copiar configurações por serviço: fácil de iniciar, com divergência de comportamento.
- Compartilhar projetos por referência: vincula builds ao layout do monorepo.
- Chassi completo com broker, persistência e Saga: antecipa decisões de incrementos ainda não implementados.
- Pacote técnico mínimo com consumidores reais: explicita compatibilidade e permite atualizações independentes.

## Decisão

Publicar `Maieru.FiapTechChallenge.Chassis`, .NET 10/MIT, por versões imutáveis no NuGet.org e GitHub OIDC Trusted Publishing. Inicialmente inclui telemetria, correlação HTTP, exceções técnicas, saúde extensível e envelope genérico. Cada consumidor fixa uma versão; builds locais/CI produzem candidatos com identificador único em feed temporário.

Preservar NUnit/Moq. Acrescentar PostgreSQL real/Testcontainers e BDD Reqnroll, mantendo os testes existentes. Medir pelo menos 80% de cobertura de linhas separadamente para legado, chassi e OS; excluir apenas testes e código gerado. Usar SonarQube Cloud e check agregado obrigatório nos PRs. Credenciais ausentes ou análise reprovada não são tratadas como sucesso.

## Consequências

Builds de consumidores exigem publicação disponível ou bootstrap do feed local. A aplicação de exemplo valida o `.nupkg`, não uma referência de projeto. Testes de integração exigem Docker. A publicação pública depende da criação/configuração da conta NuGet; a análise depende do projeto Sonar. Esses pré-requisitos externos ainda não estavam disponíveis no início da implementação.

Foram fixadas versões corrigidas de Microsoft.OpenApi (2.7.5) e SSH.NET (2026.0.0, somente nas dependências de teste) após avisos reais da restauração. As atualizações permanecem cobertas pela regressão.

## Evidências

- [Operação do CI e publicação](../operacao/monorepo-ci.md).
- [Resultados e limitações](../operacao/evidencias-fase4-incremento1.md).
- Biblioteca, testes e consumidor em `src/Chassis`.

## Organização da automação

Build, teste, cobertura e publicação são declarados nos workflows e Actions locais em `.github`. O chassi possui solução com três projetos e o OS possui solução com sete projetos. Os scripts PowerShell/Python de orquestração foram retirados; ReportGenerator 5.5.12 substitui a agregação própria de cobertura, mantendo o gate de 80% por componente. A validação comportamental da imagem foi transferida para NUnit/Testcontainers, categoria `ContainerImage`.

O inventário CSV foi substituído pela tabela de responsabilidades no guia de arquitetura. A mudança de agregador pode alterar percentuais por tratamento de linhas compartilhadas/geradas; os relatórios anteriores permanecem como evidências históricas, sem atribuir a diferença a novos testes.

As soluções usam agrupamentos visuais por componente e pastas próprias para testes/exemplo, preservando diretórios físicos e contratos de build. `Backend.slnx` reúne os mesmos 19 projetos das soluções específicas; a solução original com Docker Compose também inclui a suíte PostgreSQL legada. O README raiz concentra o mapa de soluções e cada componente novo possui seu guia de desenvolvimento.

A análise Sonar fica no workflow reutilizável `sonarqube.yml`, chamado após os testes do monorepo e incluído no check `ci-required`. Segue o modelo .NET do Sonar com runner Windows, Java 21, checkout completo e caches. A versão do scanner permanece fixada no manifesto de ferramentas. Os testes PostgreSQL continuam em Linux; o Sonar consome seu pacote candidato e cobertura OpenCover com caminhos ajustados ao checkout Windows. Organização/projeto públicos ficam no workflow, e somente `SONAR_TOKEN` exige secret.

Os passos de teste usam PowerShell 7, build prévio, `--no-build`, `--no-restore`, resultados TRX e verificação explícita do código de saída. Uma coleta gera os formatos Cobertura e OpenCover para atender, respectivamente, o gate por componente e a análise Sonar.
