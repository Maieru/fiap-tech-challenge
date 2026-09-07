# RFC-003 — Autenticação local com BCrypt e JWT

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** segurança da aplicação
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-09-07

## Contexto

A SPA administrativa precisa autenticar usuários sem sessão no servidor. O acesso do cliente a uma ordem usa um token separado do JWT administrativo.

## Decisão

Manter autenticação local com BCrypt e JWT:

- Armazenar hashes com `BCrypt.Net-Next`.
- Emitir JWT `HMAC-SHA256`, atualmente válido por 60 minutos, com `sub`, `unique_name` e `jti`.
- Validar assinatura, emissor, audiência e expiração, com `ClockSkew = 0`.
- Enviar JWT como Bearer; a SPA limpa a sessão ao expirar ou receber `401`.
- Receber a chave remota pelo mecanismo da ADR-007.

## Alternativas consideradas

- **Provedor OIDC externo:** acrescentaria federação e gestão de identidade, exigindo integração adicional.
- **Sessão no servidor:** exigiria estado compartilhado e proteção CSRF.
- **API key:** não atenderia ao fluxo de identidade individual adotado.

## Consequências e limites

- Réplicas validam JWT sem consultar uma sessão; não há refresh token, revogação, MFA ou autorização por perfis.
- O cadastro de usuário é anônimo e permite criar usuários com o mesmo acesso administrativo. Login e health checks também são anônimos.
- A SPA guarda JWT em `localStorage`, sujeito a acesso por scripts em caso de XSS.
- Chaves JWT de fallback ainda estão no artefato; removê-las e exigir segredo remoto válido permanece pendente. Rotação exige coordenação entre pods.

## Acesso à ordem na implementação atual

Acompanhamento, aprovação e cancelamento têm `AllowAnonymous` na API. Na AWS, as três rotas declaradas no estado serverless exigem Lambda Authorizer, sem cache, que valida `token` na query string consultando o banco. Esse token é SHA-256 do CPF normalizado concatenado ao código de aprovação; não é JWT.

A aprovação também valida o token no caso de uso. Acompanhamento e cancelamento dependem da proteção da borda: acessar a API diretamente não executa a Lambda. Esse controle não constitui uma estratégia completa de autorização, e o token na URL pode aparecer em histórico, logs ou telemetria.

Reconsiderar antes de uso público durável ou quando houver necessidade de perfis, SSO, MFA ou revogação.

## Evidências e relações

**Fase 3:** a Function deve autenticar por CPF, consultar existência/status e emitir JWT. O authorizer atual não emite JWT e não satisfaz esse requisito completo. A adequação depende de implementação e atualização desta decisão; veja os [diagramas de sequência](../arquitetura/sequencias.md).

- [Configuração JWT Bearer](../../src/FIAP.TechChallenge.Fase1.Infrastructure/InfraestructureDependecyInjection.cs)
- [Emissão do JWT](../../src/FIAP.TechChallenge.Fase1.Infrastructure/Security/JwtTokenService.cs)
- [Hash de senha](../../src/FIAP.TechChallenge.Fase1.Infrastructure/Security/BCryptPasswordHasher.cs)
- [Persistência da sessão no frontend](../../src/FIAP.TechChallenge.Fase1.Frontend/src/services/storage.ts)
- [ADR-007 — Gestão de segredos](../ADRs/ADR-007-gestao-centralizada-de-segredos.md)
- [Rotas anônimas na API](../../src/FIAP.TechChallenge.Fase1.API/Controllers/OrdensServicoController.cs)
- [Validação na aprovação](../../src/FIAP.TechChallenge.Fase1.Application/UseCases/OrdensServico/AprovarExecucaoOrdemServico/AprovarExecucaoOrdemServicoUseCase.cs)
- [Rotas com autorizador](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/serverless/api-gateway.tf)
- [Código do autorizador](https://github.com/Maieru/fiap-tech-challenge-serverless/tree/main/src/FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer)
