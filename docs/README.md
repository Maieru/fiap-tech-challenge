# Documentação técnica

Este diretório é o catálogo central das decisões que abrangem a aplicação e os repositórios de infraestrutura, banco de dados e componentes serverless do Tech Challenge.

## Conteúdo

- [Arquitetura da solução](ARQUITETURA.md): visão dos componentes, infraestrutura AWS, pods e camadas da aplicação;
- [RFCs](RFCs/README.md): decisões técnicas relevantes aceitas;
- [ADRs](ADRs/README.md): decisões arquiteturais permanentes já adotadas.

## RFC ou ADR?

Uma RFC registra uma escolha técnica relevante depois de sua avaliação. Ela apresenta o problema, critérios, alternativas e a decisão aceita.

Um ADR registra uma decisão arquitetural consolidada, suas consequências e os limites em que ela é válida. Uma mudança arquitetural posterior deve ser registrada em um novo ADR aceito e vinculada ao registro anterior.

Como estes registros foram introduzidos depois de parte da implementação, os documentos aceitos identificam sua natureza como **formalização retrospectiva**. Isso significa que as evidências comprovam a decisão existente, mas as alternativas reconstruídas não pretendem representar uma discussão histórica que não foi registrada.

## Estado dos registros

| Estado | Significado |
| --- | --- |
| `Aceita` | Decisão aprovada e vigente. |

O catálogo versionado contém somente decisões aceitas. Rascunhos e discussões permanecem em issues ou pull requests até a aprovação.

## Critério de inclusão

Uma decisão deve ganhar RFC ou ADR quando tiver impacto transversal, custo relevante de reversão ou efeito duradouro sobre segurança, dados, integração, operação, escalabilidade ou evolução do sistema. Detalhes locais e facilmente reversíveis permanecem junto do código.

## Processo de manutenção

1. Discutir a mudança em uma issue ou pull request.
2. Depois da aprovação, copiar o template da pasta correspondente e usar o próximo número disponível.
3. Manter uma única decisão principal por documento.
4. Incluir evidências da implementação e vínculos com registros relacionados.
5. Atualizar o índice da pasta na mesma mudança.
6. Para alterar uma decisão aceita, criar um novo registro aceito e vincular o anterior.
