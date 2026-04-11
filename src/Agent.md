# Agent Instructions

## NUnit Assertions
- Sempre que um teste tiver multiplos asserts independentes (`Assert.That`), agrupar dentro de `Assert.Multiple(() => { ... })`.
- Evitar asserts independentes sequenciais fora de `Assert.Multiple` nesses cenarios.

## Playbook Generico: Novo Caso de Uso + Testes

### Objetivo
Implementar um novo caso de uso de ponta a ponta, mantendo padrao arquitetural e cobertura de testes em:
1. Application (use case)
2. Infrastructure (repository, quando aplicavel)
3. API (controller/endpoint, quando aplicavel)

---

## 1) Descoberta e baseline

### 1.1 Escolher referencia interna
- Encontrar um caso de uso existente e estavel para usar como molde (ex.: `CriarCliente`).
- Reutilizar o mesmo padrao de:
  - command
  - response
  - interface de use case
  - classe de use case
  - testes unitarios

### 1.2 Mapear dependencias de dominio
- Confirmar entidade e value objects ja existentes.
- Confirmar contratos de repositorio necessarios.
- Se faltar contrato, criar interface nova em `Model/Interfaces`.

### 1.3 Mapear infraestrutura
- Verificar `AppDbContext`, entities e mappers existentes.
- Criar repository concreto apenas se o caso de uso exigir acesso a persistencia nao coberto.

---

## 2) Implementacao do caso de uso (Application)

### 2.1 Estrutura minima
Criar pasta de feature em `Application/UseCases/<Agregado>/<NomeCasoDeUso>` com:
- `<NomeCasoDeUso>Command`
- `<NomeCasoDeUso>Response`
- `I<NomeCasoDeUso>UseCase`
- `<NomeCasoDeUso>UseCase`

### 2.2 Regras de implementacao
- Validar entradas com value objects e/ou regras de dominio.
- Encadear validacoes com retornos de `Result<T>`.
- Propagar erros de negocio sem mascarar mensagens relevantes.
- Persistir no repository apenas no caminho de sucesso.
- Retornar `Response` mapeado a partir da entidade final.

### 2.3 DI
- Registrar use case em `ApplicationDependecyInjection`.
- Registrar novos repositories em `InfraestructureDependecyInjection`.

---

## 3) Implementacao de repository (Infrastructure, quando aplicavel)

### 3.1 Contrato
- Criar/ajustar interface de repositorio no Domain com metodos minimos para o caso.

### 3.2 Implementacao
- Implementar repository em `Infrastructure/Persistence/Repositories`.
- Usar mappers existentes (`ToEntity`, `ToDomain`) para manter consistencia.
- Garantir uso de `CancellationToken` nos metodos async.

### 3.3 API (quando aplicavel)
- Criar ou ajustar controller para expor endpoint.
- Mapear status HTTP:
  - `201/200` sucesso
  - `404` nao encontrado (quando fizer sentido de negocio)
  - `400` erro de validacao/regra de negocio

---

## 4) Testes obrigatorios por camada

### 4.1 Application.Tests (use case)
Criar suite `<NomeCasoDeUso>UseCaseTests` com, no minimo:
- Cenarios de entrada invalida (ex.: VO invalido).
- Cenarios de pre-condicao de negocio (ex.: dependencia inexistente).
- Cenarios de conflito/duplicidade (quando existir regra).
- Cenario de sucesso com verificacao de:
  - `Result` de sucesso
  - dados do `Response`
  - chamadas esperadas de repository (`Times.Once/Never`)

### 4.2 Infrastructure.Tests (repository)
Criar suite `<Entidade>RepositoryTests` com, no minimo:
- Consulta positiva (`true`/item encontrado).
- Consulta negativa (`false`/item nao encontrado).
- Persistencia (`Add`/`Update`/`Delete`, conforme contrato), verificando campos chave.
- Usar `AppDbContext` com `UseInMemoryDatabase`.

### 4.3 Regras de estilo de teste
- Arrange/Act/Assert claro.
- `Assert.Multiple` para multiplas validacoes independentes.
- Metodos helper para montar entidades/comandos validos.
- Nomes de teste no formato `Metodo_ShouldComportamento_WhenCondicao`.

---

## 5) Validacao e checklist final

### 5.1 Comandos recomendados
```powershell
# Testes direcionados da feature
dotnet test <Application.Tests.csproj> --filter "<NomeCasoDeUso>UseCaseTests" -v minimal
dotnet test <Infrastructure.Tests.csproj> --filter "<Entidade>RepositoryTests" -v minimal

# Suites completas para regressao
dotnet test <Application.Tests.csproj> -v minimal
dotnet test <Infrastructure.Tests.csproj> -v minimal
```

### 5.2 Checklist de aceite
- Caso de uso criado e registrado em DI.
- Contratos de repositorio atualizados (se necessario).
- Repository implementado (se necessario).
- Endpoint API criado/atualizado (se no escopo).
- Testes de use case cobrindo falha e sucesso.
- Testes de repository cobrindo consulta e persistencia.
- Suites de teste da camada sem regressao.

---

## Template rapido para novos cards

Substituir placeholders:
- `<Agregado>`
- `<NomeCasoDeUso>`
- `<Entidade>`

Fluxo minimo:
1. Copiar padrao de um caso de uso existente.
2. Implementar comando/response/interface/use case.
3. Ajustar dominio + repository + DI.
4. Criar testes do use case.
5. Criar testes do repository.
6. Rodar testes filtrados e depois suites completas.
