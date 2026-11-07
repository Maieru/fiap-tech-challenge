# Operação da plataforma AWS

A entrada operacional é **Manage AWS microservices platform**, no repositório principal, em main. Selecionar apply cria/atualiza e entrega o ambiente; destroy remove o laboratório e seus dados. O cron 0 3 * * * executa destroy diariamente à meia-noite de São Paulo, sujeito a atrasos do GitHub Actions.

## Arquitetura e responsabilidades

- Infra: VPC, EKS (três t3.medium), ECR, IAM, add-ons, Kafka, observabilidade e API Gateway.
- DB: RDS PostgreSQL 18.3/db.t4g.micro/20 GiB e DocumentDB 5.0/db.t4g.medium, ambos privados e criptografados.
- OS/Billing: bancos fiap_os/fiap_billing e usuários diferentes no mesmo RDS. Não compartilham schemas nem credenciais; as conexões usam pools máximos de 10 por pod. Cada usuário administra apenas seu próprio banco.
- Execution: DocumentDB fiap_execution; usuário criado em admin com readWrite somente nesse banco. Localmente permanece MongoDB 8.0.
- Kafka: apache/kafka 4.1.0 por digest, KRaft de um nó no namespace fiap-messaging, PVC gp3 criptografado de 20 GiB. Quatro tópicos, uma partição e replicação 1. Sem exposição pública, autenticação de aplicação ou alta disponibilidade; NetworkPolicy restringe acesso às APIs/Job de tópicos. EBS fica vinculado a uma AZ: é necessário nó disponível nessa AZ para remontagem.
- Entrada: HTTPS do API Gateway → VPC Link → ALB interno → Nginx → APIs. JWT é emitido pelo OS e validado nas três APIs. Não há Lambda authorizer.
- EKS permanece com nós públicos como no laboratório anterior, sem NAT Gateway adicional; bancos ficam em sub-redes privadas. Collector/New Relic existentes são reutilizados.

## Ativação inicial e transição

Esta entrega altera código/configuração; a execução real das Actions e AWS exige publicação coordenada dos repositórios e o bootstrap abaixo. Não foi realizada migração de dados nem retirada remota do legado nesta implementação.

1. Manter PLATFORM_ENABLED ausente/false e desabilitar o workflow antigo de destruição agendada durante a transição. Inventariar estados e recursos AWS antes de remover qualquer recurso rastreado.
2. Usar checkouts separados das versões anteriores para retirar o legado: Infra 39b580d8e2afc99178267cd6d4975f543b314871, DB ccdb4d9b2e9998fe86d92d47dea3209098ecbe28, principal dde85a8a2462ac3c14e7fdc2079a2b6eaa9ea1b1. Não aplicar as declarações novas sobre os estados legados ainda ocupados. Destruir serverless → api-gateway → Ingresses/ALB → kubernetes-configs → kubernetes-addons → database → aws-resources com os respectivos códigos antigos, mantendo os pais disponíveis aos dependentes. Conferir recursos remotos, locks e estados vazios; não usar state rm para esconder falhas.
3. Publicar mudanças de Infra, DB, Execution e Billing antes de ativar a nova action principal. Os workflows reutilizáveis precisam estar acessíveis ao repositório principal nas configurações do GitHub Actions.
4. Com credenciais administrativas, executar o bootstrap atualizado uma única vez: terraform init, terraform plan -out=<plano> e terraform apply <plano> em infra/bootstrap. Em conta nova, criar o backend inicialmente com state local e migrá-lo ao S3 conforme o procedimento de bootstrap do Infra. O ciclo cotidiano nunca aplica ou destrói bootstrap.
5. Configurar variables/secrets no repositório principal conforme a tabela. ARNs vêm dos outputs do bootstrap. Não criar access keys AWS para o GitHub.
6. Aguardar CI aprovado em main de OS, Execution e Billing. Definir PLATFORM_ENABLED=true e executar apply. A partir desse momento, o mesmo workflow também executará destroy diariamente.
7. Fazer o ensaio criar → reaplicar → destruir → destruir novamente → recriar e registrar a execução GitHub e inventário final como evidência operacional.

| Configuração no principal | Tipo | Origem |
| --- | --- | --- |
| PLATFORM_ENABLED | Variable | true somente após transição/bootstrap |
| INFRA_ACTION_ROLE | Variable | github_actions_infra_role_arn.k8s_infra |
| DATABASE_ACTION_ROLE | Variable | github_actions_infra_role_arn.database_infra |
| AWS_OS_PUBLISH_ROLE_ARN / AWS_OS_DEPLOY_ROLE_ARN | Variables | application_delivery_role_arns.os_publish / os_deploy |
| AWS_EXECUTION_PUBLISH_ROLE_ARN / AWS_EXECUTION_DEPLOY_ROLE_ARN | Variables | application_delivery_role_arns.execution_publish / execution_deploy |
| AWS_BILLING_PUBLISH_ROLE_ARN / AWS_BILLING_DEPLOY_ROLE_ARN | Variables | application_delivery_role_arns.billing_publish / billing_deploy |
| JWT_ISSUER / JWT_AUDIENCE | Variables | Identificadores públicos iguais para as três APIs |
| NEW_RELIC_LICENSE_KEY | Secret | Licença de ingestão existente |
| REPOSITORIES_TOKEN | Secret, quando necessário | Token restrito aos repositórios envolvidos: Contents read e Actions read |

A região é us-east-1 e o cluster fiap-eks-cluster. KAFKA_BOOTSTRAP_SERVERS é output do estágio messaging, sem preenchimento manual. Os secrets antigos db_username, db_password e jwt_signing_key não são consumidos no novo fluxo.

## Criação e entrega

A action resolve SHAs completos de main uma vez, registra-os no resumo e consulta o CI de cada aplicação no próprio repositório. Checkout, imagem e manifestos usam o mesmo SHA. Os workflows chamados recebem a identidade/contexto do chamador; por isso SOURCE_SHA e repository no catálogo de entrega são explícitos. As definições reutilizáveis são referenciadas em main; alterações de suas interfaces precisam ser coordenadas entre os repositórios.

Fluxo: core → database → add-ons → configurações → messaging → inicialização dos bancos/compatibilidade → APIs → frontend → gateway → smoke → verificação de Collector/métricas/HPA. Imagens podem ser publicadas em paralelo após core; tags sha-<commit> são imutáveis e o deploy usa o digest resolvido.

Jobs no namespace fiap-data-bootstrap criam/atualizam usuários e grants. O PostgreSQL testa acesso ao próprio banco e rejeição cruzada. A mesma imagem Execution que será entregue executa --verify-persistence em fiap_compat_<run>, validando TLS, índices, transações, rollback, concorrência e deduplicação; o banco temporário é removido ao final. Esse Job usa credencial administrativa, nunca entregue aos pods da API. Falha bloqueia os rollouts sem troca automática de tecnologia.

Após sincronizar ExternalSecrets, OS/Billing executam --migrate-only em Jobs com a mesma imagem por digest e somente a conexão de seu banco. Não iniciam HTTP, JWT ou consumidores. Falha bloqueia o rollout; não há rollback automático de schema. Migrations precisam ser compatíveis com a versão em execução.

O smoke usa o endpoint público, testa frontend/readiness/JWT e conclui uma ordem real com pagamento Billing e execução, conferindo valor e estoque. Cria dados sintéticos exclusivos e desativa o usuário de teste ao terminar. Não registra senha, JWT ou connection strings. Os demais dados sintéticos permanecem no laboratório até destroy.

## Segredos e permissões

Senhas e JWT são aleatórios, mantidos no Secrets Manager. Os segredos de banco incluem metadados administrativos para inicialização; cada ExternalSecret de aplicação importa somente ConnectionStrings__DefaultConnection ou Mongo__ConnectionString. O operador usa Pod Identity e assume uma role diferente por SecretStore, com acesso aos ARNs exatos do namespace. Deploy não altera SecretStores, RBAC ou namespaces e não lê Secrets diretamente. Criar Jobs/Deployments permite executar código com as credenciais autorizadas ao próprio serviço, portanto entrega é uma permissão privilegiada dentro desse escopo.

Conexões PostgreSQL usam VerifyFull e DocumentDB usa TLS com CA e hostname validados. O bundle público AWS é versionado no Infra; revisar sua atualização em rotações de CA, conferindo a origem https://truststore.pki.rds.amazonaws.com/global/global-bundle.pem . Hash inicial SHA-256: 90a6f1a7978a1b1b096dca6c57511ca48d3b8cf052f8e704baf358f34ea2f21b.

Terraform gera credenciais, portanto elas também existem no state. O bucket S3 é criptografado, privado, versionado e protegido contra destruição; credenciais e versões antigas do state permanecem sensíveis mesmo após apagar bancos. Infra/DB têm escritas separadas por prefixo e leituras explícitas de dependências. Remote state não constitui isolamento de segredos entre essas roles. As permissões de plataforma EC2/EKS/KMS continuam amplas dentro dos serviços necessários ao provisionamento; isto não é um perfil de produção.

## Destruição, retomada e limitações

Ordem: gateway → aplicações/Ingresses/ALB → Kafka/PVC/EBS → configurações → add-ons → bancos → EKS/ECR/rede. Aguardar recursos dependentes antes de remover controllers ou seus pais. O Terraform do gateway recebe o listener por input e o registra em output, evitando redescobrir ALB durante destroy. A exclusão assíncrona dos segredos é aguardada antes da reconstrução. Estados vazios são ignorados; AccessDenied, falhas de conectividade e drift não são tratados como recurso ausente.

Apply e destroy compartilham concurrency.group fiap-platform, sem cancelar operação em andamento. As entradas manuais antigas de provisionamento/publicação/deploy foram retiradas; não operar os estágios avulsos durante uma execução central. Em falha parcial, corrigir a causa e repetir a mesma operação. O fluxo não desfaz automaticamente o que já foi criado. Se um cluster for removido externamente enquanto ainda houver objetos Kubernetes rastreados, recuperar o acesso/estado antes de continuar; a action não descarta esses objetos silenciosamente.

Destroy apaga bancos sem snapshot final, segredos de runtime, mensagens/offsets Kafka, volumes e imagens ECR. RDS tem retenção automática zero; DocumentDB usa retenção mínima de um dia enquanto ativo. Bootstrap, IAM das pipelines e versões do state permanecem e podem ter custo residual S3. Dados/telemetria já enviados ao New Relic seguem a retenção desse serviço externo.

Validações locais não comprovam IAM efetivo, oferta regional, compatibilidade do DocumentDB real, ingestão New Relic ou o ciclo AWS. As Actions validam oferta regional e executam os gates de persistência/smoke; confirmar a ingestão de telemetria no New Relic no ambiente implantado e guardar evidências. Não utilizar este laboratório para dados únicos ou produção.

## Evidências locais desta implementação (2026-11-07)

- Sete raízes Terraform: fmt/validate aprovados, com providers inicializados e backend desabilitado.
- Workflows dos cinco repositórios ativos: actionlint aprovado; scripts Node/Bash com sintaxe verificada.
- Manifests renderizados: 21 recursos nativos aprovados no kubeconform; seis ExternalSecrets excluídos explicitamente da validação nativa (CRDs verificados por dry-run no cluster durante deploy).
- Três APIs compiladas em Release, sem avisos/erros.
- 25 testes Node de entrega aprovados; 11 testes Execution de persistência/transações/TLS aprovados. Os dois testes do gate foram repetidos após o ajuste da limpeza e passaram.
- PostgreSQL 18.3 temporário com TLS: bootstrap e isolamento repetidos, seguido de migrations OS/Billing repetidas. O ensaio final usou administrador com CREATEDB/CREATEROLE e sem SUPERUSER, exercitando o limite de permissões do RDS.
- Kafka 4.1.0 com a imagem/configuração dos manifests, usuário 1000 e volume temporário: quatro tópicos criados e mensagem preservada após reinício.
- Containers/volume/rede temporários removidos após os ensaios; nenhuma operação de provisionamento/destruição foi executada na AWS.

O gate DocumentDB, o smoke público, o teste de HPA/Collector e o ensaio criar/destruir/recriar AWS estão implementados para execução pela action. Seus resultados reais ainda não foram obtidos. Validar IAM efetivo e ingestão New Relic após publicação coordenada/bootstrap.
