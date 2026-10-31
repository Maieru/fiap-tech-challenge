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
