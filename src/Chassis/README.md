# Chassi técnico

Abra [Chassis.slnx](Chassis.slnx) para trabalhar nos três projetos desta pasta.

| Projeto | Responsabilidade |
| --- | --- |
| `Maieru.FiapTechChallenge.Chassis` | Biblioteca distribuída por NuGet: logs, métricas, traces, correlação HTTP, exceções técnicas, saúde e envelope de mensagens |
| `Maieru.FiapTechChallenge.Chassis.Tests` | Testes unitários e HTTP dos contratos da biblioteca |
| `Maieru.FiapTechChallenge.Chassis.Sample` | Aplicação mínima que consome o pacote gerado por `PackageReference` |

As pastas `Tests` e `Samples` da solução são agrupamentos visuais. Os projetos permanecem nos diretórios atuais.

## Desenvolver e validar

Requer .NET SDK 10. Antes da primeira publicação pública, prepare ou baixe o pacote candidato conforme o [guia de preparação local](../../docs/operacao/monorepo-ci.md#comandos-locais-quando-necessários). A aplicação de exemplo precisa desse pacote para restaurar suas dependências.

Na raiz do repositório:

```powershell
dotnet build src/Chassis/Chassis.slnx -c Release
dotnet test src/Chassis/Chassis.slnx -c Release --no-build
```

Ao alterar a biblioteca, gere uma nova versão candidata antes de validar o exemplo. O teste da biblioteca usa `ProjectReference`; o exemplo usa exclusivamente `PackageReference` para comprovar o consumo do `.nupkg`.

## Contrato e publicação

O [README do pacote](Maieru.FiapTechChallenge.Chassis/README.md) descreve a API pública e acompanha o `.nupkg`. Este guia descreve a organização do código no monorepo.

Os workflows e Actions locais em `.github` executam build, testes, cobertura mínima de 80% e validação HTTP do exemplo. A tag `chassis-vX.Y.Z` aciona a publicação, após configurar a conta NuGet conforme o [guia de CI](../../docs/operacao/monorepo-ci.md). Regras de negócio, persistência e transporte de mensagens pertencem aos serviços consumidores.
