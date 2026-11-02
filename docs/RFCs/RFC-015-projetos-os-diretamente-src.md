# RFC-015 — Projetos OS diretamente em src

- **Status:** Aceita
- **Data:** 2026-11-02
- **Escopo:** organização física, soluções, Docker, Compose, CI e documentação

## Contexto

Após retirar o monólito, este repositório contém somente o backend OS e o frontend. O agrupamento físico `src/Services/OrdensServico` deixou de separar múltiplos serviços e acrescentava níveis aos comandos, links e builds. Execution e Billing já possuem repositórios independentes.

## Decisão

Mover os nove diretórios `FIAP.TechChallenge.OS.*` diretamente para `src`, sem renomear namespaces, assemblies, projetos, pacotes ou contratos. As referências entre projetos continuam relativas entre diretórios irmãos.

Usar `Backend.slnx` na raiz como solução canônica. Manter `src/TechChallengeFase1.slnx` como alias para o Visual Studio com os mesmos nove projetos; retirar a solução redundante `OrdensServico.slnx` da pasta removida.

Mover o Dockerfile OS para a raiz e usar contexto de build na raiz tanto no Compose integrado quanto no isolado. O Compose isolado passa a `compose.os.yaml`, preservando projeto `fiap-os`, portas e nomes de volumes. Usa o `.env` da raiz, sem sobrescrever credenciais existentes. Durante a validação do arquivo isolado foram encontrados dois comandos de criação de tópicos fora do bloco de comandos; foram recolocados em `kafka-init`, que inicializa os quatro tópicos necessários.

O guia específico do OS passa a `docs/operacao/os-service.md`. Workflows de testes/imagem/publicação de contratos, scripts de restore e links locais usam os caminhos novos. A instrução de build OS no README do Execution também é atualizada. URLs de commits históricos preservam os caminhos existentes naquela versão.

## Alternativas consideradas

- Manter a árvore `Services/OrdensServico`: preserva uma separação que não existe mais neste repositório.
- Mover também testes para uma nova árvore `tests`: aumentaria a reorganização e alteraria referências sem necessidade para remover o agrupamento redundante.
- Renomear namespaces/assemblies ou o frontend: amplia a mudança para compatibilidade e publicação; não é necessário para simplificar diretórios.

## Consequências e validação

Comandos de desenvolvimento usam `dotnet build Backend.slnx`, `docker build -f Dockerfile .` e `docker compose -f compose.os.yaml`. Abrir novamente a solução atualiza os caminhos no Visual Studio. O cache da solução antiga foi arquivado localmente após fechar o IDE, permitindo remover a árvore `Services` do disco.

Restore força a atualização dos metadados NuGet nos caminhos novos. Build, testes/cobertura, imagem e seus testes HTTP verificam a reorganização. Checks estáticos conferem a equivalência das duas soluções, ProjectReferences, workflows, Compose, links e ausência de referências ativas à árvore removida.

Resultados locais em 2026-11-02:

- Restore com o feed candidato já preparado e build Release aprovados, sem avisos ou erros.
- 265 testes OS aprovados, incluindo 59 de integração com PostgreSQL/Kafka reais e isolados. Cobertura de linhas: 90,4%, acima do gate de 80%, consolidada com ReportGenerator 5.5.12.
- Imagem `fiap-os:flat-layout` construída a partir do Dockerfile da raiz; dois testes HTTP da imagem aprovados.
- Geração local de `FIAP.TechChallenge.OS.Application.Messaging` 0.2.0 e pacote de símbolos aprovada, sem publicação.
- 228 arquivos C#/projetos preservados com o mesmo conteúdo versionado; duas soluções equivalentes com nove projetos; 396 links locais e referências de projetos/workflows válidos.
- Composes integrado e isolado validados, mantendo projetos/volumes e inicialização dos quatro tópicos. A pasta `src/Services` não permanece no disco.

TRX, cobertura, pacote local e cópias do cache/solução antiga ficam em `artifacts/project-layout`, ignorado pelo Git. Não foram repetidos os cenários distribuídos, porque namespaces, contratos e código dos serviços não mudaram; os testes de integração e da imagem exercitam os pontos afetados pelo layout. CI remoto, publicação e deploy não foram executados.

Esta mudança não implanta serviços, altera dados/volumes ou publica pacotes. As decisões de [retirada do monólito](../ADRs/ADR-021-retirada-codigo-monolito.md), [CI sem monólito](RFC-014-solucoes-ci-sem-monolito.md) e [proxies do frontend](RFC-013-proxy-frontend-microsservicos.md) permanecem; esta RFC atualiza apenas a organização física e seus pontos de entrada.
