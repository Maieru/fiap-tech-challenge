# RFC-014 — Soluções e verificação de OS e frontend

- **Status:** Aceita
- **Data:** 2026-11-02
- **Última revisão:** 2026-11-14
- **Escopo:** soluções, CI e observabilidade local

## Contexto

O repositório contém o backend OS e o frontend. Uma solução de entrada única reduz duplicação de caminhos e configuração de desenvolvimento.

## Decisão

Usar `Backend.slnx` na raiz, com os nove projetos OS, tanto no CLI quanto no Visual Studio. Cada serviço externo mantém sua própria solução.

`monorepo-ci.yml` verifica OS e frontend. O job OS usa PostgreSQL/Kafka reais, cobertura mínima de 80%, imagem e OpenAPI; Sonar compila a mesma solução. `ci-required` agrega `frontend`, `component` e `quality`, falhando quando um requisito falha ou é ignorado.

Manter `compose.observability.yaml` independente, com Collector e portas locais 4317/4318. Segredos ficam fora do Git; a stack de negócio não exige licença New Relic para iniciar.

## Alternativas consideradas

Soluções duplicadas exigiriam manter listas de projetos equivalentes. Renomear namespaces ou assemblies ampliaria o escopo sem benefício para a organização. Remover o Collector descartaria uma capacidade compartilhada.

## Consequências e evidências

Build e testes usam a solução canônica. Validação local não substitui CI remoto ou implantação AWS.

- [Solução backend](../../Backend.slnx).
- [CI e dependências](../operacao/monorepo-ci.md).
- [Organização dos projetos](RFC-015-projetos-os-diretamente-src.md).
