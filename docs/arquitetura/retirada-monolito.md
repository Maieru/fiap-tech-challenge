# Retirada do monólito — código e automação

Atualizado em 2026-11-02. A fonte do monólito foi retirada deste checkout. OS, Execution, Billing e frontend continuam sendo a aplicação ativa; a infraestrutura e os dados antigos ainda exigem uma etapa própria de retirada.

## Escopo executado

- Removidos nove projetos .NET do monólito: API, Application, Domain, Infrastructure e cinco projetos de testes.
- Removidos cenários k6 das rotas antigas, `Legacy.slnx`, Compose/projeto Docker do legado e scripts/configurações exclusivos.
- `Backend.slnx` e `src/TechChallengeFase1.slnx` contêm os mesmos nove projetos OS. A reorganização posterior da [RFC-015](../RFCs/RFC-015-projetos-os-diretamente-src.md) moveu esses projetos diretamente para `src` e retirou a solução OS redundante da pasta antiga.
- Matriz de CI sem legado; `ci-required` inclui frontend, testes/cobertura OS e Sonar.
- Removidos os três workflows que construíam e implantavam o monólito e orquestravam sua infraestrutura.
- Seis manifests do backend antigo preservados em [legacy/aws](../../legacy/aws/README.md), fora de `k8s` ativo.
- Collector opcional mantido em `compose.observability.yaml`, sem banco ou aplicação legada.

A remoção inicial contemplou 378 arquivos versionados exclusivos e preservou os arquivos locais ignorados pelo Git. Uma limpeza posterior do disco removeu 14 pastas obsoletas, com 4.217 arquivos gerados e aproximadamente 415,3 MiB: saídas `bin`/`obj`, caches de projetos antigos e código Reqnroll gerado. As preferências `.user` antigas foram copiadas para `artifacts/monolith-retirement/local-cleanup` antes da limpeza. Frontend, OS, configuração de observabilidade, `.env`, dados, volumes e recursos remotos foram preservados. Não foram feitas alterações no código de produção dos três microsserviços.

## Histórico e recuperação

A versão anterior está no commit [`067aef498eb1fcf7104bf678915ad3d7058ba089`](https://github.com/Maieru/fiap-tech-challenge/tree/067aef498eb1fcf7104bf678915ad3d7058ba089). Consulte essa árvore ou crie um checkout separado dessa versão se precisar investigar o comportamento antigo. Não restaure automaticamente o monólito no checkout principal nem seus workflows de deploy.

Relatórios e decisões anteriores são históricos, não instruções atuais. Links para fontes removidas apontam para esse commit; a documentação atual usa somente os serviços novos.

## Validação desta retirada

Os resultados abaixo registram a etapa de retirada. A reorganização posterior para projetos diretamente em `src`, com duas soluções ativas, está documentada e validada na [RFC-015](../RFCs/RFC-015-projetos-os-diretamente-src.md).

As evidências desta sessão ficam em `artifacts/monolith-retirement/`, ignoradas pelo Git. A [verificação anterior do frontend](../operacao/validacao-frontend.md) continua sendo um registro separado.

| Verificação após remover o código | Resultado |
| --- | --- |
| Build `Backend.slnx` Release | Nove projetos OS compilados; zero erros e avisos. |
| Regressão OS sem imagem | 265 aprovados: 136 domínio, 66 aplicação, 4 contratos e 59 integração com PostgreSQL/Kafka reais. |
| Cobertura OS | ReportGenerator 5.5.12: 90,5%, gate mínimo de 80% aprovado. |
| Imagem nova `fiap-os:retirement` | Construída deste checkout, sem a fonte do monólito; dois testes de imagem aprovados. |
| Distribuídos OS/Execution/Billing | Nove cenários aprovados usando a imagem nova OS e imagens locais Execution/Billing, com contêineres/dados isolados. |
| Frontend | Nove testes Node/Vite e build TypeScript/Vite aprovados. Mantidos os avisos anteriores de tamanho do bundle e base Browserslist. |
| Referências e documentação | Três soluções equivalentes com nove projetos; ProjectReferences e workflows locais sem destinos ausentes; 387 links locais em 72 documentos verificados. Links históricos preservados no commit de recuperação. |
| Configuração e arquivo AWS | YAML/workflows válidos; Composes integrado, observabilidade e smoke validados sem iniciar serviços. Seis manifests arquivados com o mesmo conteúdo versionado original. Workflow de destruição agendada sem alterações; serviços de desenvolvimento permanecem ativos. |

TRX/cobertura OS estão em `artifacts/monolith-retirement/tests`, `image-tests` e `coverage`; distribuídos em `FiapTechChallengeExecution/artifacts/monolith-retirement-distributed`. Build Docker usa o candidato Billing já preparado em `artifacts/packages`; não comprova publicação desse pacote nem restore de um runner remoto exclusivamente pelo feed público. Nenhum teste usa bancos ou volumes de desenvolvimento. CI remoto, Sonar remoto e deploy não foram executados nesta sessão.

## Próxima etapa: infraestrutura e dados

A remoção dos arquivos não executa desligamento remoto. Para concluir a retirada operacional, ainda é necessário inventariar os recursos e a necessidade de retenção dos dados, implantar os três serviços e suas dependências, validar Gateway/ALB/JWT/links privados, migrar o tráfego e só então retirar os recursos exclusivos antigos. RDS, Lambda Authorizer, segredos, repositórios de imagem e recursos compartilhados precisam ser avaliados nos repositórios responsáveis.

O workflow de destruição agendada existente foi preservado e não foi executado nesta sessão. O deploy novo precisa de automação própria; a publicação reutilizável da imagem frontend continua disponível, mas não provisiona microsserviços.

Decisões: [ADR-021](../ADRs/ADR-021-retirada-codigo-monolito.md) e [RFC-014](../RFCs/RFC-014-solucoes-ci-sem-monolito.md).
