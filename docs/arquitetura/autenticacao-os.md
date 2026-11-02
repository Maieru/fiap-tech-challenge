# Autenticação administrativa no OS

OS agora cadastra usuários e emite JWT para OS, Execution e Billing. Execution e Billing validam a assinatura localmente, sem consultar o OS a cada requisição. As três APIs usam issuer/audience/chave iguais, definidos no `.env` da stack integrada.

## Endpoints

| Método e caminho no OS | Acesso | Resultado |
| --- | --- | --- |
| POST `/api/os/v1/usuarios` | Anônimo | 201 com ID e login normalizado |
| POST `/api/os/v1/usuarios/login` | Anônimo | 200 com token, tipoToken e expiresInSeconds |
| DELETE `/api/os/v1/usuarios/{id}` | JWT válido | 204 e exclusão lógica |

Cadastro e login recebem `{ "usuario": "admin", "senha": "uma-senha-escolhida-por-voce" }`. Isso é um exemplo de corpo, não uma conta pré-criada. Login é normalizado para minúsculas; senha preserva os espaços e deve ter pelo menos oito caracteres não vazios e até 72 bytes UTF-8. Senhas são armazenadas apenas como hash BCrypt e não são retornadas nas respostas.

Cadastre o usuário na tela de login do frontend em http://localhost:5173 ou pelo Scalar do OS em http://localhost:8081/scalar/v1, depois faça login. O frontend usa o token retornado como `Authorization: Bearer <token>` nos endpoints de negócio das três APIs. O cadastro foi mantido **anônimo por escolha do projeto**; qualquer usuário assim cadastrado recebe acesso administrativo, sem diferenciação de perfis.

## Configuração e migração

Na raiz do OS, `docker compose up -d --build` aplica a migration UsuariosAutenticacao e disponibiliza o novo login. `JWT_ISSUER`, `JWT_AUDIENCE` e `JWT_SIGNING_KEY` são comuns à stack; chave não tem fallback e deve ter pelo menos 32 bytes UTF-8. `JWT_ACCESS_TOKEN_MINUTES` controla a emissão pelo OS, com default 60 e intervalo 1–1440. Não há usuário ou senha padrão.

Os usuários são novos no banco exclusivo do OS: contas do legado não são importadas automaticamente. O frontend migrado autentica em `/api/os/v1/usuarios/login` e compartilha a sessão entre seus três clientes HTTP administrativos. As rotas legadas continuam disponíveis separadamente; a stack integrada não depende delas. Tokens de outro emissor só funcionam se a configuração criptográfica e os metadados coincidirem.

Expiração ou 401 administrativo limpa a sessão e exige novo login. O cliente HTTP público de acompanhamento não envia o JWT e não dispara logout administrativo em 401; seu token privado tem escopo de uma ordem. Consulte o [guia da migração do frontend](migracao-frontend.md) e as [evidências locais](../operacao/validacao-frontend.md).

A emissão é HS256 com sub, unique_name e jti. Assinatura, expiração, issuer e audience são validados, com clock skew zero. Não há refresh token, revogação ou autorização por perfis. Excluir um usuário impede novos logins, mas tokens já emitidos continuam válidos até expirar. Chaves compartilhadas também exigem rotação coordenada.

## Verificação

Os testes HTTP exercitam cadastro anônimo, login real, hash, espaços na senha, limite BCrypt, duplicidade concorrente, credenciais inválidas e exclusão. O BDD distribuído cria usuário e faz login na imagem real do OS; usa esse JWT em OS, Billing e Execution, verifica 401 sem token/com token inválido e percorre o fluxo de negócio completo.

Decisões: [ADR-018](../ADRs/ADR-018-autenticacao-administrativa-no-os.md), [RFC-011](../RFCs/RFC-011-autenticacao-os-bcrypt-jwt.md). O [estado da migração](estado-da-migracao.md) distingue a identidade administrativa do acesso do cliente à própria ordem, implementado por [link privado no OS](acompanhamento-e-entrega.md), separado do JWT administrativo.

Resultados locais em 2026-11-02: 259 testes OS sem imagem aprovados (134 domínio, 66 aplicação, 4 contratos e 55 integração/validação) e 7 cenários distribuídos aprovados com login real. Logs locais: OS `artifacts/os-auth-regression.log` e `artifacts/os-auth-tests.log`; Execution `artifacts/os-auth-e2e.log`, com TRX em `artifacts/os-auth-e2e/`. Não houve publicação de pacotes, execução de CI remoto ou importação de usuários do legado.
