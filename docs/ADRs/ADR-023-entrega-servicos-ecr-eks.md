# ADR-023 — Entrega independente por repositório com OIDC e digest

- **Status:** Aceita
- **Data:** 2026-11-07
- **Escopo:** OS/frontend, Execution, Billing e infraestrutura de entrega

## Contexto

A ADR-022 preparou os manifestos para aplicação manual. A revisão encontrou ECRs ausentes para as APIs, identidade de aplicação limitada aos namespaces legados e ausência de roles para Execution/Billing. Também era necessário renovar pods ao atualizar ConfigMaps/Secrets consumidos por ambiente.

## Decisão

Cada repositório oferece um workflow de publicação ECR e outro de deploy EKS. No OS, o componente pode ser OS ou frontend. Ambos são manuais, em main, e exigem o último CI do commit concluído com sucesso; PRs não recebem credenciais de entrega.

Usar OIDC com roles separadas de publicação e deploy por repositório. Infra declara ECRs imutáveis das APIs, roles/trust policies, access entries EKS e RBAC nos namespaces. Não adicionar serviços ao mapa legado que também cria roles amplas de infraestrutura. O grupo de deploy OS atende fiap-os/fiap-frontend; os demais atendem somente seus namespaces. O grupo novo não lê Secrets nem administra RBAC/cluster.

Publicar sha-<commit>, registrar digest e implantar somente por digest. Uma tag já existente é reutilizada; AccessDenied ou falha de rede interrompem a entrega. O deploy usa manifestos do commit atual e permite selecionar imagem de SHA anterior com CI aprovado, registrando os dois commits para recuperação consciente.

Renderizar sem editar fontes e bloquear placeholders. O deploy faz dry-run no servidor, aguarda atualização dos ExternalSecrets, aplica os recursos e acompanha rollout. As anotações de configuração/execução renovam os pods, inclusive em rotação de segredos sem troca de imagem. O HPA continua controlando replicas.

## Alternativas consideradas

- Deploy em todo push: adiado para preservar controle operacional sobre dependências e migrations ainda externas.
- Uma role ampla para todos os serviços: amplia acessos desnecessários e mistura entrega com provisionamento.
- Deploy de latest: impede vincular rollout ao artefato publicado e prejudica recuperação.
- Construir imagem durante deploy: acopla duas operações que precisam ser independentes.
- Alterar Secret diretamente pela Action: substituído pela sincronização do operador existente.

## Consequências e evidências

Bootstrap, aws-resources e kubernetes-configs precisam ser aplicados antes de configurar as Actions variables. Bancos, Kafka, segredos/grants, IAM do operador e migrations continuam pré-requisitos. Não há apply de infraestrutura, publicação ECR ou deploy remoto nesta implementação.

Aplicação de múltiplos recursos não é atômica. Rollback não desfaz migrations; imagens antigas precisam ser compatíveis com schemas e manifestos atuais. Roles antigas são mantidas para evitar retirada implícita de acessos.

Ver [operação](../../scripts/delivery/README.md), [RFC-017](../RFCs/RFC-017-actions-ecr-eks.md) e [mapa CI/CD](../operacao/ci-cd.md).
