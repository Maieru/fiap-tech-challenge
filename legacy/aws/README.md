# Referência da infraestrutura legada

Os manifests em `k8s/backend` deste diretório descrevem o backend monolítico anteriormente implantável. Foram retirados da árvore ativa `k8s` para evitar aplicação acidental junto dos microsserviços. Não são um roteiro de deploy ou de remoção de recursos.

O código, testes, Compose e workflows de build/deploy do monólito foram removidos do checkout. A versão anterior está no commit [`067aef498eb1fcf7104bf678915ad3d7058ba089`](https://github.com/Maieru/fiap-tech-challenge/tree/067aef498eb1fcf7104bf678915ad3d7058ba089). Não há cópia executável do monólito neste diretório.

Esta retirada não executa `kubectl`, `terraform`, exclusão de bancos/volumes, alteração de segredos ou migração de dados. API Gateway/ALB, RDS, Lambda Authorizer e demais recursos AWS legados ainda exigem inventário e retirada próprios, após implantação dos novos serviços e decisão sobre retenção dos dados. O workflow já existente de destruição agendada da infraestrutura permanece inalterado; esta mudança não o executa.

Os workflows que recriavam/publicavam o monólito (`initialize-and-deploy.yml`, `build-backend-image.yml`, `deploy-applications.yml`) foram removidos. A publicação reutilizável da imagem frontend permanece, mas o novo fluxo de deploy dos serviços ainda precisa ser implementado.

Ver [retirada do monólito](../../docs/arquitetura/retirada-monolito.md) e [ADR-021](../../docs/ADRs/ADR-021-retirada-codigo-monolito.md).
