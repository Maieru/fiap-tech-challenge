# ADR-005 — Entrada pública por API Gateway e ALB interno

- **Status:** Aceita
- **Data:** 2026-08-23
- **Última revisão:** 2026-11-14
- **Escopo:** rede e exposição HTTP

## Contexto

Frontend e APIs no EKS compartilham uma entrada pública. Os Services das aplicações permanecem internos ao cluster.

## Decisão

Usar `API Gateway HTTP API → VPC Link → ALB interno → Nginx → APIs`. O grupo de Ingress `fiap-edge` encaminha os prefixos versionados ao proxy Nginx com prioridade 5 e as páginas do frontend com prioridade 20. O proxy preserva os prefixos e usa os Services OS, Execution e Billing configurados nos upstreams.

Chamadas administrativas são autenticadas por JWT nas APIs. Acompanhamento e decisões do cliente são autorizados pelo OS com token privado da ordem. Nginx também encaminha `/otlp/*` ao Collector interno.

## Alternativas consideradas

ALB público reduziria componentes, mas retiraria controles do API Gateway. Um load balancer por aplicação duplicaria entradas e recursos.

## Consequências

A disponibilidade da entrada depende de API Gateway, ALB e Nginx. Os trechos internos usam HTTP; TLS interno exige configuração própria. A ingestão de telemetria não tem autenticação específica. Manifestos e validação local não comprovam implantação nem funcionamento remoto.

## Evidências e relações

- [Ingress do frontend](../../k8s/frontend/ingress.yaml).
- [Ingress dos prefixos](../../k8s/frontend/microservices-ingress.yaml).
- [Proxy Nginx](../../src/FIAP.TechChallenge.Fase1.Frontend/nginx.conf).
- [Frontend e mesma origem](ADR-020-frontend-microsservicos-mesma-origem.md).
- [Guia operacional AWS](../operacao/plataforma-aws.md).
