# RFC-009 — Integração Kafka do OS e BDD distribuído

- **Status:** Aceita
- **Data:** 2026-11-01
- **Escopo:** implementação da etapa 3 aprovada

## Contexto e decisão

Implementar a [ADR-016](../ADRs/ADR-016-integracao-os-execution.md) com PostgreSQL/EF Core no OS,
chassi 1.1.1, adaptador Kafka 0.1.1 e contratos de cada produtor na versão 0.1.0.
Cada serviço referencia o pacote de contratos remoto por NuGet; não há ProjectReference entre repositórios.
Commands, interfaces e casos de uso ficam em arquivos separados.

OS usa IUnitOfWork e ITransaction mínimos, IMessagingService e MessageContext por escopo.
O consumidor recebe DiagnosticoIniciado, DiagnosticoConcluido, ExecucaoIniciada, ExecucaoConcluida e ExecucaoCancelada.
Confere envelope, versão, chave e payload antes de delegar. Preserva correlação, causação e contexto W3C.
A deduplicação usa chave oficina-os-v1/MessageId, independente do GroupId configurável dos testes.
O OS guarda status e datas; snapshots de itens, valores e estoque permanecem no Execution.

PUT /api/os/v1/ordens/{id}/aprovar-execucao e PUT /api/os/v1/ordens/{id}/cancelar exigem JWT administrativo real.
Retornam 202 com decisão, status e pendente. Estado inadequado retorna 400, inexistente 404 e decisão oposta 409.
PostgreSQL e consumidor/broker Kafka integram readiness; liveness não consulta dependências.

## Ambiente e verificação

O Compose independente do OS inclui seu próprio Kafka para desenvolvimento isolado.
Para o fluxo completo, compose.integrated.yaml no repositório Execution acrescenta OS e PostgreSQL
ao seu Compose, compartilhando somente o broker. Recebe imagem OS pronta, sem compilar o outro checkout.
Ambos os serviços validam o mesmo issuer/audience/chave JWT.

Distributed.slnx no Execution contém testes Reqnroll independentes de assemblies das APIs.
Testcontainers inicia as duas imagens, PostgreSQL, MongoDB replica set e Kafka em rede/volumes descartáveis.
Os cenários exercitam HTTP autenticado e observam Kafka, sem mocks ou acesso aos bancos dos serviços.
Cobrem conclusão, cancelamento, repetição, concorrência, indisponibilidade e propagação de contexto.
Um workflow manual recebe imagens publicadas para repetir esses testes sem checkout do OS.

Testes locais do OS também provocam indisponibilidade do broker e falha de constraint diferida no commit real,
verificando rollback e a possibilidade aceita de mensagem órfã. O teste da imagem verifica encerramento por mensagem inválida
e offset pendente. Não existem flags de falha no código de produção.

## Alternativas consideradas

- Referenciar projetos das duas APIs nos testes distribuídos: impediria builds independentes dos repositórios.
- Inserir estado diretamente em bancos no BDD: não verificaria os contratos HTTP/Kafka de ponta a ponta.
- Executar BDD distribuído na solução padrão: exigiria uma imagem OS em toda validação isolada do Execution.

## Consequências

Build/CI limpos exigem os dois contratos publicados. Enquanto isso, scripts explícitos validam e copiam nupkg
para artifacts/packages e geram configuração NuGet local ignorada pelo Git. Não há seleção automática de versões.
CI remoto, publicação NuGet e deploy permanecem ações separadas da validação local.

Consulte as [evidências](../operacao/revisao-execution-etapa3.md) e os READMEs dos serviços para os comandos.
