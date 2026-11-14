# ADR-008 — Separação de repositórios e estados Terraform

- **Status:** Aceita
- **Data:** 2026-08-23
- **Última revisão:** 2026-11-14
- **Escopo:** organização da plataforma e infraestrutura como código

## Contexto

Aplicações, infraestrutura e bancos têm ciclos e permissões distintos. Provisionamento e destruição precisam respeitar as dependências entre recursos.

## Decisão

Manter o repositório principal como entrada do ciclo, Infra como responsável pela plataforma compartilhada e DB como responsável por RDS e inicialização dos bancos. OS, Execution e Billing mantêm suas imagens, migrations e workloads; Chassis mantém os pacotes técnicos.

Bootstrap é administrativo e separado do ciclo cotidiano. O apply encadeia core → database → add-ons → configurações → messaging → inicialização dos bancos → APIs → frontend → gateway → verificações. Os estágios usam estados separados em S3 e outputs explícitos por `terraform_remote_state`.

## Alternativas consideradas

Um único state ampliaria planos e impacto de falhas. State por recurso criaria dependências demais. Workspaces por componente confundiriam ambientes e responsabilidades.

## Consequências

Outputs e workflows exigem coordenação entre repositórios. Separar chaves de estado não isola automaticamente IAM ou dados sensíveis. A remoção deve respeitar os dependentes e conferir a exclusão efetiva dos recursos e volumes. Estados incompatíveis exigem conciliação e revisão do plano antes de aplicar.

## Evidências

- [Workflow principal](../../.github/workflows/manage-platform.yml).
- [Guia operacional](../operacao/plataforma-aws.md).
- [Plataforma AWS](ADR-024-plataforma-microsservicos-aws.md).
