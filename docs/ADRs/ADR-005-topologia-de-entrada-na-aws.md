# ADR-005 — Entrada pública por API Gateway e ALB interno

- **Status:** Aceita
- **Data:** 2026-08-23
- **Responsáveis:** equipe do Tech Challenge
- **Escopo:** rede e exposição HTTP
- **Natureza:** formalização retrospectiva
- **Última revisão:** 2026-11-02

## Contexto

Frontend e backend no EKS compartilham uma entrada pública, mantendo ALB e services internos.

## Decisão

Usar `API Gateway HTTP API → VPC Link → ALB interno → pods`.

- A integração `HTTP_PROXY` encaminha a rota `$default` ao ALB.
- Ingresses no grupo `fiap-edge` mantêm `/api` para o backend legado (prioridade 10) e `/` para o frontend (20). A extensão da [ADR-020](ADR-020-frontend-microsservicos-mesma-origem.md) prepara os três prefixos versionados para o proxy Nginx com prioridade 5.
- Services são `ClusterIP`; o Load Balancer Controller registra os IPs dos pods nos target groups.
- As rotas legadas de acompanhamento, aprovação e cancelamento usam Lambda Authorizer com `token` na query string, sem cache. A RFC-003 descreve esse controle adicional. Os caminhos novos `/api/os/v1/ordens/acompanhamento/*` validam o token privado no OS; não reutilizam o authorizer por CPF.
- O Nginx encaminha `/otlp/*` ao Collector interno.
- O stage configura limite de 100 requisições/s, burst 200 e access logs por 7 dias, sem headers ou corpos.

## Alternativas consideradas

- **ALB público:** reduziria componentes, mas removeria os controles centralizados do API Gateway.
- **Load balancer por aplicação:** duplicaria recursos e endpoints.
- **Outro controller ou descoberta direta:** exigiria substituir a integração Kubernetes/ALB existente.

## Consequências

- Uma entrada atende às duas aplicações; falhas no ALB afetam ambas.
- API Gateway depende do ALB criado pelos Ingresses; o estado serverless depende do gateway.
- O trecho VPC Link–ALB usa HTTP, sem TLS interno.
- A ingestão `/otlp/*` não tem autenticação específica e aceita telemetria não confiável.
- O ALB permite saída para o CIDR da VPC nas portas 80 e 8080, sem restrição apenas aos targets.

Novas exposições públicas exigem decisão própria. Reconsiderar por custo, latência, isolamento de serviços ou necessidade de TLS interno.

A migração do frontend mantém esta topologia de entrada, mas requer os Services OS, Execution e Billing e seus upstreams antes do rollout. Os manifests novos não comprovam implantação: migração do tráfego AWS e verificação remota continuam pendentes.

A retirada posterior do código na [ADR-021](ADR-021-retirada-codigo-monolito.md) moveu os manifests do backend antigo para `legacy/aws`, fora da árvore ativa. Isso não removeu regras ou recursos existentes na AWS.

## Evidências e relações

- [Ingress do backend](../../legacy/aws/k8s/backend/ingress.yaml)
- [Ingress do frontend](../../k8s/frontend/ingress.yaml)
- [Ingress dos prefixos de microsserviços](../../k8s/frontend/microservices-ingress.yaml)
- [Service do backend](../../legacy/aws/k8s/backend/services.yaml)
- [Proxy Nginx do frontend](../../src/FIAP.TechChallenge.Fase1.Frontend/nginx.conf)
- [Terraform do API Gateway](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/api-gateway/api-gateway.tf)
- [Diagramas da arquitetura e da infraestrutura](../arquitetura/README.md)
- [RFC-001 — Escolha da AWS](../RFCs/RFC-001-escolha-da-nuvem-aws.md)
- [RFC-004 — Containers no EKS](../RFCs/RFC-004-containers-no-amazon-eks.md)
- [ADR-009 — Observabilidade com OpenTelemetry](ADR-009-observabilidade-com-opentelemetry.md)
- [Rotas com Lambda Authorizer](https://github.com/Maieru/fiap-tech-challenge-infra/blob/main/infra/serverless/api-gateway.tf)
- [RFC-003 — Autenticação e acesso às ordens](../RFCs/RFC-003-autenticacao-jwt-e-bcrypt.md)
