# ADR-018 — Identidade administrativa temporariamente no OS

- **Status:** Aceita
- **Data:** 2026-11-02
- **Última revisão:** 2026-11-02

## Contexto

Os três serviços validavam JWT, mas a emissão e os usuários pertenciam exclusivamente ao legado. A stack integrada não permitia login sem executar outro backend. A decisão do projeto é manter essa responsabilidade temporariamente no OS.

## Decisão

OS mantém usuários administrativos em seu PostgreSQL exclusivo e oferece cadastro, login e exclusão lógica sob `/api/os/v1/usuarios`. Não acessa banco nem assemblies do legado. Cadastro permanece anônimo, conforme escolha explícita do usuário; login também é anônimo e exclusão exige JWT. Não há perfil distinto para cada usuário cadastrado: um usuário autenticado recebe acesso administrativo.

JWT conserva HS256, sub/unique_name/jti, issuer e audience configuráveis e chave fornecida apenas por variável Jwt__SigningKey. Execution e Billing permanecem validadores independentes, com a mesma configuração. Usuários antigos não são importados automaticamente; os novos serviços podem usar dados novos de demonstração.

## Alternativas

Provedor de identidade ou quarto serviço de autenticação: adiados para manter os três serviços atuais. Login apenas no legado: mantém dependência que a migração deve eliminar. Bootstrap protegido para cadastro: não adotado porque foi solicitado preservar o cadastro anônimo.

## Consequências

A stack dos três serviços autentica por conta própria. O legado conserva suas rotas separadamente, mas o frontend migrado já usa o login OS, conforme a [ADR-020](ADR-020-frontend-microsservicos-mesma-origem.md). Não há refresh, revogação ou perfis; exclusão impede novos logins, sem invalidar JWT já emitido. Cadastro público administrativo deve ser reavaliado antes de exposição pública. Reconsiderar a localização da identidade quando o escopo ou a infraestrutura exigirem.

## Evidências

Migration UsuariosAutenticacao, casos de uso e testes HTTP próprios do OS; BDD obtém tokens pelo login real do OS e os utiliza nos três serviços. Consulte RFC-011 e o guia de autenticação.
