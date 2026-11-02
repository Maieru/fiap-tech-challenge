# ADR-013 — Chassi em repositório próprio

- **Status:** Aceita
- **Data:** 2026-10-31
- **Escopo:** organização, distribuição NuGet e CI

## Contexto

A preparação do chassi dentro do monorepo exigia feed temporário, seleção dinâmica de versões, artefatos entre jobs e cópia de pacotes nos Dockerfiles das aplicações. O responsável criou `Maieru/fiap-tech-challenge-chassis` e autorizou a extração para reduzir essa complexidade.

## Decisão

Mover biblioteca, testes, exemplo, solução, licença e automação específica para o novo repositório. Preservar `Maieru.FiapTechChallenge.Chassis` como ID do pacote, sua API pública e a versão inicial 0.1.0. O chassi terá build, cobertura, Sonar e publicação OIDC próprios.

Legado e OS permanecem no monorepo e fixam a versão do pacote diretamente nos respectivos `PackageReference`. Seus pipelines e Dockerfiles restauram dependências públicas. O principal deixa de produzir o chassi, selecionar versões candidatas ou transportar seu `.nupkg` entre jobs. Regras de negócio permanecem em cada aplicação.

## Alternativas consideradas

- Manter o chassi no monorepo: preserva o layout anterior, mas mantém o acoplamento da preparação do pacote ao CI e aos Dockerfiles das aplicações.
- Criar um kernel com negócio compartilhado: amplia o escopo e cria dependências entre modelos que devem evoluir separadamente.

## Consequências

Cada repositório passa a ter suas configurações Sonar e proteções de branch. A publicação é configurada somente no repositório do chassi. O restore limpo e as imagens das aplicações dependem da primeira publicação pública da versão 0.1.0; validar um pacote local não comprova sua disponibilidade no NuGet.org.

Essa decisão atualiza a organização descrita na RFC-006. A ADR-011 continua orientando a coexistência do legado e do OS, cuja separação final permanece pendente.

## Evidências e relações

- [Repositório do chassi](https://github.com/Maieru/fiap-tech-challenge-chassis).
- [CI e dependências do monorepo](../operacao/monorepo-ci.md).
- [RFC-006](../RFCs/RFC-006-chassi-nuget-e-verificacao-monorepo.md).
