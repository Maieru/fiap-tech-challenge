# RFC-003 — Autenticação local com BCrypt e JWT

- **Status:** Aceita
- **Data:** 2026-08-23
- **Última revisão:** 2026-11-14
- **Escopo:** autenticação administrativa e acesso do cliente

## Contexto

A SPA administrativa precisa autenticar usuários sem sessão no servidor. O acesso do cliente a uma ordem utiliza um token privado separado do JWT administrativo.

## Decisão

OS armazena hashes BCrypt e emite JWT. As três APIs validam assinatura, emissor, audiência e expiração localmente. O frontend envia Bearer JWT e encerra a sessão ao expirar ou receber `401`.

Para acompanhamento, aprovação e cancelamento pelo cliente, OS emite um token aleatório por ordem e persiste apenas seu hash. Os endpoints públicos validam o token no próprio OS; uma nova emissão substitui o acesso anterior. O gateway deve encaminhar essas requisições ao serviço, preservando a privacidade das URLs.

## Alternativas consideradas

Um provedor OIDC externo acrescentaria federação e gestão de identidade. Sessões no servidor exigiriam estado compartilhado. API keys não atendem ao fluxo de identidade individual adotado.

## Consequências e limites

Cadastro administrativo anônimo é uma decisão do projeto. Não há refresh, revogação ou perfis distintos. Excluir usuário impede novos logins, mas não revoga JWT emitido. O frontend guarda a sessão em `localStorage`, sujeito a XSS. O token privado não tem expiração automática; logs, histórico e telemetria devem proteger sua URL.

## Evidências

- [Configuração JWT](../../src/FIAP.TechChallenge.OS.API/Security/AuthenticationDependencyInjection.cs).
- [Emissão JWT](../../src/FIAP.TechChallenge.OS.Infrastructure/Security/JwtTokenService.cs).
- [Hash de senha](../../src/FIAP.TechChallenge.OS.Infrastructure/Security/BCryptPasswordHasher.cs).
- [Acompanhamento](../../src/FIAP.TechChallenge.OS.API/Controllers/AcompanhamentoController.cs).
- [Guia de autenticação](../arquitetura/autenticacao-os.md).
- [Acesso do cliente](../arquitetura/acompanhamento-e-entrega.md).
