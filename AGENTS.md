# Orientações para agentes

## Decisões técnicas e arquiteturais

Toda decisão de grande impacto deve ser documentada. Isso inclui escolhas com efeito transversal, alto custo de reversão ou consequências relevantes para arquitetura, segurança, dados, integrações, infraestrutura, operação, escalabilidade ou observabilidade.

- Registre decisões técnicas relevantes como RFCs em `docs/RFCs`.
- Registre decisões arquiteturais permanentes como ADRs em `docs/ADRs`.
- Inclua contexto, alternativas consideradas, decisão, consequências e evidências da implementação.
- Atualize o índice da pasta correspondente ao adicionar ou substituir um registro.
- O catálogo versionado deve conter somente decisões com status `Aceita`. Rascunhos e discussões ainda não aprovados devem permanecer na issue ou pull request em que estão sendo avaliados.
- A documentação da decisão deve ser criada ou atualizada junto da mudança que a materializa.

## Implementação no OS

- Use um caso de uso existente e estável, como `CriarCliente`, como referência para commands, responses, interfaces, resultados e testes.
- Os projetos atuais ficam em `src/FIAP.TechChallenge.OS.*`. Use `Backend.slnx`, na raiz, para abrir, compilar e testar o backend.
- Antes de implementar, confira entidades, value objects, contratos em `FIAP.TechChallenge.OS.Domain/Interfaces`, `AppDbContext` e mappers existentes. Crie somente os contratos e repositórios necessários.
- Organize casos de uso em `FIAP.TechChallenge.OS.Application/UseCases/<Agregado>/<NomeCasoDeUso>`, com command, response, interface e implementação conforme o padrão da feature.
- Valide entradas com value objects e regras do domínio, encadeie `Result<T>`, preserve erros de negócio e persista somente no caminho de sucesso. Mapeie o resultado para a resposta final.
- Confira o registro em `ApplicationDependencyInjection`: casos de uso com interfaces são descobertos por convenção. Registre os repositórios em `InfrastructureDependencyInjection`.
- Implemente contratos de repositório no Domain e persistência em `Infrastructure/Persistence/Repositories`, reutilizando os mappers. Propague `CancellationToken` em métodos assíncronos.
- Nas APIs, use os códigos HTTP apropriados: `201/200` para sucesso, `404` para recurso ausente e `400` para entrada ou regra de negócio inválida, conforme o contrato existente.

## Testes e validação

- Testes NUnit com múltiplos asserts independentes (`Assert.That`) devem agrupá-los em `Assert.Multiple`.
- Mantenha Arrange/Act/Assert claro, helpers para entidades e comandos válidos e nomes no formato `Metodo_ShouldComportamento_WhenCondicao`.
- Em `FIAP.TechChallenge.OS.Application.Tests`, cubra entradas inválidas, precondições, conflitos quando aplicáveis e sucesso, verificando `Result`, response e chamadas esperadas de repositório (`Times.Once/Never`).
- Novos testes de persistência e API devem usar PostgreSQL real via Testcontainers, aplicar migrations e isolar os dados entre cenários. Preserve testes existentes com `UseInMemoryDatabase`, identificando o uso de um provedor substituto.
- Nos testes de repositório, verifique consultas, persistência, constraints, rollback e retenção física de registros inativados, conforme o comportamento alterado.
- Testes de controller devem chamar endpoints reais por `HttpClient`, com autenticação real. Não mocke use cases, services, repositórios, `DbContext` ou clientes HTTP nesses testes.
- Prepare dados de testes HTTP pelo próprio contrato da API e valide status e payload. Use `FIAP.TechChallenge.OS.Integration.Tests` e seu `OsTestHost` como referência, incluindo cenários de sucesso e erro para endpoints alterados.
- Execute primeiro as suítes afetadas e depois a regressão apropriada à mudança. Confira registros de DI, contratos, mapeamento HTTP e cobertura dos comportamentos alterados antes de concluir.
- Para compilar o backend, use `dotnet build Backend.slnx -c Release`. Para regressão sem testes de imagem, use `dotnet test Backend.slnx -c Release --filter 'TestCategory!=ContainerImage'`.

