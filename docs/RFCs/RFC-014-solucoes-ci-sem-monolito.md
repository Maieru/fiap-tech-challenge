# RFC-014 — Soluções e verificação sem o monólito

- **Status:** Aceita
- **Data:** 2026-11-02
- **Escopo:** soluções .NET, CI, observabilidade local e recuperação histórica

## Contexto

A [RFC-015](RFC-015-projetos-os-diretamente-src.md) atualiza posteriormente os caminhos físicos e elimina a solução OS redundante. As duas soluções ativas continuam equivalentes e o gate de CI permanece o mesmo.

A retirada da fonte do monólito quebra soluções, matriz de testes e workflows que ainda apontem para seus projetos. O Collector era definido no Compose legado, embora continue útil aos microsserviços.

## Decisão

Manter `Backend.slnx` como ponto de entrada do backend atual, com os nove projetos OS. Preservar `src/TechChallengeFase1.slnx` como alias de conveniência para o Visual Studio, sem projeto Docker legado. Remover `Legacy.slnx`. Os projetos atuais não mudam de namespace ou diretório, para evitar migração não relacionada.

Remover a entrada `legacy` da matriz de `monorepo-ci.yml`. O job OS mantém os testes com PostgreSQL/Kafka reais, cobertura mínima de 80%, imagem e OpenAPI; Sonar compila a solução agora exclusiva do OS. `ci-required` passa a exigir `frontend`, `component` e `quality`, falhando também quando o frontend falha ou é ignorado. Os gates não são relaxados para compensar exclusão de testes antigos.

Remover `build-backend-image.yml`, `deploy-applications.yml` e `initialize-and-deploy.yml`. Esses workflows eram exclusivos da implantação antiga; não convertê-los automaticamente em um deploy de serviços ainda não provisionados. Preservar publicação frontend, contratos e destruição agendada existente.

Fornecer `compose.observability.yaml` standalone, projeto `fiap-observability`, somente com Collector e portas locais 4317/4318. Credenciais continuam fora do Git; `.env.example` da raiz documenta as opções sem exigir licença para a stack de negócio. Arquivos `.env` locais e volumes não são removidos.

## Alternativas consideradas

- Renomear todos os projetos remanescentes: aumenta mudanças em imagens, imports e links sem relação com a retirada.
- Redirecionar o workflow antigo para OS: manteria dependências de RDS/Lambda/topologia do monólito e aparentaria suportar um deploy ainda incompleto.
- Remover o Collector: descartaria uma capacidade técnica compartilhada que não depende do monólito.

## Consequências e validação

Build e testes backend devem passar sem os arquivos antigos, incluindo uma imagem nova construída do checkout. Testes distribuídos verificam as três APIs em ambiente isolado com essa imagem OS. Frontend mantém build e testes próprios. A revisão estática confere soluções, referências de projetos, chamadas de workflows, Compose e links documentais.

Resultados são registrados em [retirada do monólito](../arquitetura/retirada-monolito.md). Essa validação não substitui CI remoto ou implantação AWS. A [ADR-021](../ADRs/ADR-021-retirada-codigo-monolito.md) estabelece o limite entre retirada do código e exclusão de recursos/dados.
