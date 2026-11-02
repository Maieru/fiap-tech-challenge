# ADR-021 — Retirada do código do monólito e separação da infraestrutura legada

- **Status:** Aceita
- **Data:** 2026-11-02
- **Escopo:** código, soluções, Compose, CI e referências de infraestrutura

## Contexto

A organização física posterior é definida pela [RFC-015](../RFCs/RFC-015-projetos-os-diretamente-src.md): projetos OS diretamente em `src`, Dockerfile e Compose isolado na raiz. O escopo da retirada registrado abaixo permanece.

OS, Execution e Billing atendem ao ciclo distribuído com identidade própria, catálogo/estoque, pagamento mockado, execução e entrega. O frontend já usa os contratos novos. Os nove projetos do monólito duplicavam regras e testes e ainda tinham Compose e workflows capazes de reconstruir e publicar a aplicação antiga. Os serviços novos não referenciam seu código ou banco.

## Decisão

Retirar os nove projetos versionados do monólito, seus testes de carga, solução exclusiva, Compose/projeto Docker e scripts exclusivos. Manter OS e frontend nos caminhos atuais. `Backend.slnx` e `src/TechChallengeFase1.slnx` passam a representar os mesmos nove projetos OS, incluindo contratos e testes; a solução própria do OS permanece.

CI verifica OS e frontend, com cobertura/Sonar backend e ambos os resultados exigidos por `ci-required`. Remover os workflows exclusivos de build/deploy do monólito e sua orquestração de provisionamento, para não republicar o componente retirado. A publicação reutilizável do frontend e os workflows de contratos permanecem.

Arquivar os seis manifests do backend antigo em `legacy/aws/k8s/backend`, fora da árvore Kubernetes ativa. A retirada do código não é retirada da infraestrutura: não executar alteração de tráfego, exclusão de RDS/bancos/volumes, segredos, Lambda, Terraform ou recursos Kubernetes. O workflow de destruição agendada existente permanece inalterado. A nova implantação dos microsserviços e a política de retenção/migração de dados exigem etapa própria.

Preservar o Collector local como recurso opcional em `compose.observability.yaml`, independente do Compose removido. Documentos históricos continuam disponíveis e links para o código retirado apontam para o commit anterior à remoção.

## Alternativas consideradas

- Manter todo o legado até o deploy AWS: mantém duplicação e um caminho de deploy incompatível com o frontend migrado. O histórico Git permite recuperação sem mantê-lo ativo.
- Copiar o código para uma pasta de arquivo: conserva as dependências e permite reconstrução acidental; manter somente referência de versão é suficiente.
- Excluir imediatamente os recursos remotos: não comprova retenção dos dados nem disponibilidade dos substitutos na nuvem. Não faz parte desta retirada do checkout.

## Consequências e evidências

O desenvolvimento e os checks deste repositório deixam de depender de nove projetos antigos. Perdem-se os comandos locais exclusivos do monólito; recuperação exige consultar o histórico. A infraestrutura legada não será reprovisionada pelo orquestrador removido, mas recursos já existentes e automações externas não são alterados por uma mudança de arquivos.

A [ADR-011](ADR-011-extracao-incremental-os-monorepo.md) passa a ser referência do período de coexistência. A [ADR-001](ADR-001-monolito-em-camadas.md) descreve a arquitetura histórica; a separação em camadas continua nos serviços atuais. Decisões de banco e topologia da infraestrutura existente não são revogadas por excluir código.

Escopo, versão de recuperação e resultados: [retirada do monólito](../arquitetura/retirada-monolito.md). Automação: [RFC-014](../RFCs/RFC-014-solucoes-ci-sem-monolito.md).
