# ADR-007 — Gestão centralizada de segredos

- **Status:** Aceita
- **Data:** 2026-08-23
- **Última revisão:** 2026-11-14
- **Escopo:** segurança e configuração dos microsserviços

## Contexto

APIs e observabilidade precisam receber credenciais sem incluí-las nas imagens ou nos manifests versionados.

## Decisão

Armazenar segredos de runtime no AWS Secrets Manager e sincronizá-los com Kubernetes por External Secrets. Cada namespace usa seu SecretStore e permissões de leitura dos ARNs necessários via EKS Pod Identity. APIs consomem somente as credenciais de seu banco; os parâmetros JWT precisam ser compatíveis entre os três serviços.

ConfigMaps contêm apenas dados não sensíveis. Licença New Relic e credenciais Mercado Pago seguem o fluxo de segredos. Pipelines acessam AWS por OIDC; valores de pagamento são gravados sem incorporá-los às declarações Terraform.

## Alternativas consideradas

Distribuição manual de Kubernetes Secrets acoplaria a operação aos deploys. Sealed Secrets ou CSI Driver exigiriam outro mecanismo de gestão e consumo.

## Consequências

IAM e RBAC precisam limitar leituras por namespace. Atualizar um Secret não altera variáveis de ambiente de pods existentes; rotação exige rollout e coordenação com JWT emitido. State e planos Terraform podem conter dados sensíveis. Configuração sem janela de recuperação e a destruição diária exigem cuidado com retenção.

## Evidências

- [ExternalSecret OS](../../k8s/os/external-secrets.yaml).
- [Deployment OS](../../k8s/os/deployment.yaml).
- [Guia da plataforma](../operacao/plataforma-aws.md).
- [Observabilidade](../operacao/observabilidade.md).
