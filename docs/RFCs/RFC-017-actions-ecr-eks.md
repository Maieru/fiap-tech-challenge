# RFC-017 — Actions de publicação ECR e aplicação EKS

- **Status:** Aceita
- **Data:** 2026-11-07

## Contexto e decisão

Implementar a [ADR-023](../ADRs/ADR-023-entrega-servicos-ecr-eks.md) com publish-image.yml e deploy-eks.yml em cada repositório, scripts Node 22 com YAML 2.8.3 fixado em lockfile e testes no gate ci-required. Cada repositório possui sua cópia da ferramenta para não depender de checkout de outro projeto.

Autenticar por configure-aws-credentials/OIDC. Variables de repositório fornecem ARNs das roles publish/deploy, região, broker e identidade JWT pública. Não transmitir credenciais dos bancos, chave JWT ou licença New Relic às Actions.

A publicação valida CI em main via API GitHub e gera sha-<SHA completo>. Resolve/reutiliza tags imutáveis e registra digest. O wrapper build-frontend-image.yaml delega ao novo publisher sem publicar latest.

O deploy valida CI do código atual e da imagem selecionada, consulta digest ECR, prepara kubectl da versão minor EKS com checksum e usa kubeconfig/cópias YAML temporários. Substitui parâmetros estruturadamente, bloqueia placeholders e registra checksum/identificador da execução no Pod template. Faz dry-run no servidor, força refresh e aguarda ExternalSecrets atualizados, aplica workloads e aguarda rollout. Não modifica replicas dos Deployments com HPA.

## Alternativas consideradas

Sed/envsubst para YAML: substituição textual não preserva tipos/quoting com segurança. Segredos como inputs: entrega desnecessária de valores à Action. Um único workflow build/deploy: impede selecionar e reimplantar artefato já publicado.

## Consequências e validação

GitHub Actions tem início manual, concorrência serializada por componente e sem cancelamento de deploy ativo. O CI atual deve passar para o SHA exato antes da publicação/deploy. Alterações de namespace/CRDs/RBAC ficam no Infra e não na role de aplicação.

Testar renderização, digest obrigatório, entradas inválidas, gatilho de renovação dos pods e seleção do último CI. Validar workflows com actionlint, recursos renderizados com kubeconform e módulos Infra com fmt/validate. Execução remota depende de applies/configurações do operador; não é simulada como sucesso por validação estática.
