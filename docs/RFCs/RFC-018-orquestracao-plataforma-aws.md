# RFC-018 — Orquestração e segurança da plataforma AWS

- **Status:** Aceita
- **Data:** 2026-11-07

## Contexto e decisão

Workflows reutilizáveis herdam contexto e identidade do chamador. Usar implicitamente github.sha/github.repository verificaria o CI errado no deploy entre repositórios. O principal resolve commits uma vez e passa source_sha, parâmetros públicos e roles a workflows reutilizáveis. Catálogos de entrega identificam o repositório de cada serviço. Fonte, imagem e manifests usam o mesmo commit; imagem é entregue por digest.

Plan e apply ocorrem no mesmo job com plano local removido ao terminar, sem artifact sensível. Estados permanecem separados e o bucket preserva versões e lockfiles. Bootstrap é administrativo, fora do ciclo cotidiano. Infra e DB usam policies diferentes, com escritas limitadas aos seus prefixes. SecretStores usam roles de leitura por namespace; aplicações não alteram os stores.

Inicialização de usuários/grants ocorre em Jobs internos com credenciais administrativas separadas. OS/Billing têm --migrate-only, que encerra sem iniciar API/JWT/consumidores. Execution usa --verify-persistence em banco temporário e adapta somente o transporte TLS/sessões para DocumentDB. Falhas bloqueiam entrega; não há rollback automático nem fallback de banco.

A única entrada operacional é manage-platform.yml em main, com apply/destroy e cron diário. concurrency.group fiap-platform serializa ambos, sem cancelamento. Destroy segue dependências inversas, ignora somente estados vazios e verifica remoção de recursos. O listener ALB é input/output persistido no estado do gateway.

## Alternativas e consequências

Dispatch separado por repositório manteria identidades locais, mas exigiria polling e trava distribuída. Planos em artifacts facilitariam jobs separados, mas propagariam credenciais. Migrações no startup dos pods criariam concorrência e misturariam entrega com inicialização da API. Essas alternativas não foram adotadas.

As referências de workflows entre repositórios continuam em main; alterações de interface exigem publicação coordenada. Recursos/configuração de produção, backups duráveis e promoção hml/prod ficam fora do laboratório. O [guia operacional](../operacao/plataforma-aws.md) registra ativação, testes e limitações.
