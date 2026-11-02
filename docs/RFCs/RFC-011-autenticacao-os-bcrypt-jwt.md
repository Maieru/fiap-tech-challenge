# RFC-011 — Cadastro/login OS com BCrypt e JWT compatível

- **Status:** Aceita
- **Data:** 2026-11-02

## Implementação

Reutilizar as regras do legado com namespaces OS, arquivos por caso de uso e abstrações independentes de EF/BCrypt/JWT. Infrastructure implementa repositório EF, BCrypt.Net-Next 4.0.3 e emissão JWT com System.IdentityModel.Tokens.Jwt 8.17.0. Não há ProjectReference ao legado.

Usuarios é tabela própria com ID, login normalizado, hash e Ativo. Índice único de login ativo protege concorrência; exclusão lógica retira o usuário do login. Senhas têm mínimo de oito caracteres não vazios e máximo de 72 bytes UTF-8 pelo limite do BCrypt. A senha é usada integralmente no hash e na verificação, preservando espaços fornecidos pelo usuário.

POST /api/os/v1/usuarios retorna 201, POST /login retorna token/tipoToken/expiresInSeconds e DELETE /{id} retorna 204. Entradas inválidas retornam 400, credenciais incorretas/inexistentes/inativas retornam 401 com a mesma mensagem, login duplicado retorna 409 e usuário inexistente na exclusão retorna 404. Respostas não expõem hash/senha.

A chave simétrica singleton é compartilhada por validação e emissão no OS. Issuer/audience continuam obrigatórios, clock skew zero; Jwt:AccessTokenMinutes aceita 1–1440, default 60. Chave vem apenas de Jwt__SigningKey, com mínimo de 32 bytes UTF-8. Não há evento Kafka de login nem chamada HTTP ao OS para validar JWT em Execution/Billing.

## Alternativas e compatibilidade

Sem refresh, revogação, perfis, migração automática de usuários ou bootstrap. Cadastro anônimo foi mantido por decisão explícita. Endpoints legados permanecem para o frontend atual; a API nova usa prefixo versionado OS. Tokens existentes continuam compatíveis enquanto assinatura, issuer, audience e expiração forem válidos.

## Verificação

Casos de uso reaproveitados, HTTP com PostgreSQL/Kafka e autenticação reais, hash persistido, concorrência de login normalizado, senha incorreta, exclusão e senhas com espaços/limite BCrypt. A suíte distribuída cadastra e faz login no OS real antes de acessar as três imagens; não fabrica o JWT do fluxo normal.
