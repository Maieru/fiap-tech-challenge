# Evidências — primeiro incremento da Fase 4

Verificação local em **2026-10-31**, branch **inicio-segregacao-servicos**. Estes resultados não são uma execução remota do GitHub Actions nem uma análise SonarQube Cloud.

## Testes e cobertura

| Componente | Testes aprovados | Linhas cobertas | Cobertas/total |
| --- | ---: | ---: | ---: |
| legacy | 703 | 89.88% | 4121/4585 |
| chassis | 11 | 100.00% | 83/83 |
| os | 208 | 84.53% | 1552/1836 |

Total: **922 testes aprovados**, sem falhas ou ignorados nas execuções registradas. As linhas de cada componente são mescladas pelas localizações de origem; bibliotecas compartilhadas não compensam a cobertura de um serviço. Migrações/código gerado e testes não fazem parte da métrica.

O legado inclui 588 testes originais e 115 cenários HTTP/BDD em PostgreSQL real. Antes da extração, os 588 testes passaram e a coleta inicial conjunta mediu 90,32% (coleta inicial sem o arquivo de exclusões posteriormente padronizado). O timeout anterior do testhost não se repetiu na execução fora da sandbox.

As verificações atuais usam exatamente `scripts/test-component.ps1`, também chamado pelo CI. Os [resumos estruturados](evidencias-fase4-incremento1.json) identificam as pastas locais de TRX/Cobertura/OpenCover. No GitHub, esses arquivos serão publicados pelos artefatos `verification-legacy`, `verification-chassis` e `verification-os`.

## Verificações adicionais

- `dotnet build Backend.slnx -c Release`: 19 projetos, sem avisos ou erros na verificação integrada.
- Pacote 0.1.0 gerado em `artifacts/release`, com biblioteca net10.0, README, licença MIT e símbolos.
- Consumidor sem ProjectReference: build e execução HTTP a partir de PackageReference, com saúde, correlação e logs JSON verificados.
- Imagens `fiap-os:local` e `fiap-legacy:phase4` construídas localmente.
- `scripts/smoke_os_image.py`: CRUD, JWT, migrations, soft delete, histórico, especificação OpenAPI e dados preservados após reiniciar a API. Containers e rede temporários removidos pelo próprio teste.
- Migrations do OS aplicadas nos testes; modelo sem alterações pendentes; falha forçada no histórico confirma rollback da ordem.
- Proteção da main consultada/aplicada pela API do GitHub: PR, `ci-required` e administradores obrigatórios, preservando as demais exigências existentes.
- ID NuGet consultado: sem registro público em 2026-10-31. Isso não substitui a confirmação de propriedade/permissão da conta publicadora.
- Sintaxe Python, XML de projetos/soluções, oito documentos YAML (via YamlDotNet) e PowerShell verificados; referências do OS não apontam ao legado.

## Pendências externas e de escopo

**NuGet.org:** a conta publicadora ainda não foi criada/configurada. O pacote local não foi publicado. **SonarQube Cloud:** organização, projeto e token ainda não estão configurados; análise e quality gate remoto não foram executados. O workflow falha explicitamente quando faltam essas configurações. Consulte o [passo a passo](monorepo-ci.md).

O código continua na branch local, sem push, merge ou criação de PR nesta implementação. O status de CI remoto só poderá ser comprovado após publicar a branch e executar os workflows.

Ainda fora deste incremento: deploy AWS/Kubernetes do OS, novos recursos Terraform, migração de dados/frontend/autenticação de clientes, Billing/Mercado Pago, Execution/NoSQL, mensageria, Saga e BDD distribuído. O monorepo deverá ser separado para a entrega final.
