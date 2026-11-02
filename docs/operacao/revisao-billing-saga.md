# Revisão da implementação Billing e Saga

Revisão em 2026-11-01 do código local após a organização por camadas e casos de uso. Escopo: contratos, domínio, persistência, publicação/consumo, coordenação no OS, cancelamento no Execution e testes. A revisão original não alterou o código de produção. Os dois achados foram corrigidos posteriormente neste mesmo ciclo, conforme registrado abaixo.

## Achados

### P2 — Campo monetário ausente é convertido em zero — corrigido

Antes da correção, `BillingKafkaConsumer` desserializava `PagamentoSolicitado` sem conferir a presença de `valor` no JSON. O DTO usa decimal não anulável; o serializador assume zero para um parâmetro ausente. `Cobranca.Criar` aceita zero por regra de negócio e o mock aprova a cobrança. Não é possível distinguir um pedido de valor zero explícito de um pedido inválido sem valor.

O mesmo mecanismo existia na entrada `DiagnosticoConcluido` do OS para `valorTotal`. Se um produtor omitir esse campo, OS registra orçamento zero e pode produzir um pedido de pagamento zero, mesmo que o Execution possua itens com valor positivo. Se apenas o pedido Billing estiver sem valor e OS guardar orçamento positivo, a confirmação zero é rejeitada no OS, interrompendo o consumidor e bloqueando o fluxo.

Reprodução na revisão original: um programa temporário em `Billing/artifacts/review-probe` desserializou um pedido com IDs e moeda válidos, sem `valor`, chamou a fábrica real e o gateway mockado. Resultado: `valor=0; gateway approved=True`. Nenhum serviço ou dado de desenvolvimento foi alterado.

Correção recomendada: validar a presença e o tipo dos campos obrigatórios antes de transformar o payload em command, ou tornar sua presença obrigatória na desserialização dos contratos. Rejeitar mensagem sem total/valor mantendo offset pendente. Continuar aceitando zero explícito. Adicionar testes de entrada incompleta nas duas fronteiras.

Referências: BillingKafkaConsumer.cs, linhas 72 e 101–102; OsKafkaConsumer.cs, linha 99. Correção aplicada: validação no consumidor antes da desserialização; ausência, null, string e overflow são rejeitados e zero explícito permanece válido.

### P2 — Teste de reentrega pode passar antes de o consumidor aplicar as repetições — corrigido

Antes da correção, `ProcessRealMessageAndQueryWithJwt` publicava a mensagem repetida e uma mensagem equivalente com outro ID e logo consulta o banco. A confirmação de `ProduceAsync` só comprova entrega ao broker. A cobrança original já existe, portanto `totalItems=1` pode ser observado antes das duas reentregas chegarem ao caso de uso. Mesmo uma falha do consumidor ou uma tentativa indevida do gateway durante a repetição pode passar despercebida.

Correção recomendada: capturar os offsets publicados e esperar o grupo confirmar o último; depois verificar cobrança única, quantidade de resultados emitidos e readiness. Para verificar chamadas duplicadas ao gateway, usar um gateway contador de teste. O teste separado de concorrência usa contador e cobre exclusão mútua, mas não substitui essa verificação da reentrega via consumidor.

Referência: BillingIntegrationTests.cs, linhas 74–75. Correção aplicada: o teste espera o commit do último offset de entrada, conta chamadas ao gateway e resultados no Kafka e verifica cobrança única e readiness.

## Limites já aceitos, separados dos achados

Publicação antes do commit pode gerar mensagens órfãs; não há atomicidade entre gateway, banco e Kafka. A implementação não contempla Outbox, DLQ, retry de aplicação, estorno, cancelamento administrativo após aprovação ou timeout comercial da Saga. Essas escolhas estão documentadas na ADR-017/RFC-010 e não foram tratadas como regressões nesta revisão.

A compensação cobre recusa de pagamento e devolução de estoque; não é um mecanismo geral de desfazer qualquer falha posterior. O BDD de recusa usa um responder Kafka de teste; a resposta recusada do gateway é testada separadamente no Billing.

## Validação desta revisão

- Programa temporário de reprodução: confirmou o default monetário zero na ausência de campo.
- Billing: 12 testes aprovados (2 de contratos e 10 no projeto principal, incluindo smoke da imagem), com HTTP/JWT, PostgreSQL e Kafka reais. Log local: `FiapTechChallengeBilling/artifacts/review-tests.log`.
- BDD: 6 cenários aprovados com as três imagens e bancos isolados. Log local: `FiapTechChallengeExecution/artifacts/review-saga-bdd.log`.
- Links relativos dos novos documentos e whitespace verificados. Não foram executados deploy, publicação NuGet ou CI/Sonar remotos.
- Na revisão original, os testes existentes não cobriam campo monetário ausente nem esperavam a conclusão das reentregas. Essas lacunas passaram a ter regressões específicas.

Documento explicativo: [Saga de pagamento](../arquitetura/saga-pagamento.md).

## Correções aplicadas

- `MonetaryPayloadValidation` em cada Infrastructure valida a presença e o tipo JSON numérico com representação decimal válida. Billing aplica a guarda ao pedido; OS aplica ao diagnóstico e aos resultados de pagamento. A guarda antecede a desserialização e o caso de uso, sem mudar os DTOs NuGet ou exigir migrations.
- Cada serviço possui testes para campo ausente, null, string, overflow e payload não objeto, além de zero explícito, valor fracionário e limite decimal válido.
- Os testes de imagem processam valor zero real antes de enviar mensagem sem valor; verificam encerramento não zero e offset da mensagem inválida ainda pendente.
- O teste de reentrega espera o grupo confirmar o último offset e então verifica uma cobrança, uma chamada ao gateway, um resultado publicado e readiness saudável, tanto na aprovação quanto na recusa.
- Os detalhes históricos dos achados acima descrevem o comportamento anterior à correção.

### Validação das correções

- Billing: 19 testes de contratos/unidade/integração e 2 testes de imagem aprovados; cobertura de linhas de 92,22% na suíte sem imagem. O teste de reentrega espera offsets e verifica chamada única ao gateway nos dois resultados de negócio.
- OS: 242 testes na solução sem imagem (4 contratos, 56 aplicação, 134 domínio e 48 integração/validação), além de 2 testes de imagem aprovados. Zero explícito é projetado normalmente; diagnóstico sem valorTotal encerra o host e mantém o offset pendente.
- BDD: 6 cenários aprovados com as imagens corrigidas.
- Total: 271 testes aprovados. Compilação final Billing sem erros nem avisos; whitespace verificado.
- Logs locais ignorados pelo Git: Billing `artifacts/review-fixes.log` e `artifacts/review-fixes-images.log`; OS `artifacts/billing-review-fixes.log` e `artifacts/billing-review-fixes-images.log`; Execution `artifacts/review-fixes-saga-bdd.log`.

As duas correções estão concluídas. Permanecem os limites de consistência distribuída descritos na Saga; não houve alteração dos contratos NuGet, migrations ou publicação remota.
