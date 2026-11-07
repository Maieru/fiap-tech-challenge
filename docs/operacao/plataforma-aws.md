# Operação da plataforma AWS

A entrada operacional é **Manage AWS microservices platform**, no repositório principal, em main. Selecionar apply cria/atualiza e entrega o ambiente; destroy remove o laboratório e seus dados. O cron 0 3 * * * executa destroy diariamente à meia-noite de São Paulo, sujeito a atrasos do GitHub Actions.

## Arquitetura e responsabilidades

- Infra: VPC, EKS (três c7i-flex.large x86_64, 2 vCPUs/4 GiB por nó), ECR, IAM, add-ons, Kafka, observabilidade e API Gateway. O tipo EC2 é elegível ao Free Tier atual; isso não implica gratuidade da plataforma inteira. Créditos, prazo do plano e cobrança dos serviços devem ser acompanhados na conta.
- DB: RDS PostgreSQL 18.3/db.t4g.micro/20 GiB privado/criptografado, credenciais/CA MongoDB e Jobs de inicialização. Infra executa MongoDB no EKS junto do estágio messaging.
- OS/Billing: bancos fiap_os/fiap_billing e usuários diferentes no mesmo RDS. Não compartilham schemas nem credenciais; as conexões usam pools máximos de 10 por pod. Cada usuário administra apenas seu próprio banco.
- Execution: MongoDB 8.0 em replica set rs0 de um membro, namespace fiap-mongodb, TLS obrigatório e PVC gp3 criptografado de 20 GiB. Usuário criado em admin com readWrite somente em fiap_execution, retryWrites=true e pool máximo de 10. A mesma tecnologia atende desenvolvimento e AWS; não há alta disponibilidade ou backup permanente no laboratório.
- Kafka: apache/kafka 4.1.0 por digest, KRaft de um nó no namespace fiap-messaging, PVC gp3 criptografado de 20 GiB. Quatro tópicos, uma partição e replicação 1. Sem exposição pública, autenticação de aplicação ou alta disponibilidade; NetworkPolicy restringe acesso às APIs/Job de tópicos. EBS fica vinculado a uma AZ: é necessário nó disponível nessa AZ para remontagem.
- Entrada: HTTPS do API Gateway → VPC Link → ALB interno → Nginx → APIs. JWT é emitido pelo OS e validado nas três APIs. Não há Lambda authorizer.
- EKS permanece com nós públicos como no laboratório anterior, sem NAT Gateway adicional; bancos ficam em sub-redes privadas. Collector/New Relic existentes são reutilizados.

## Ativação inicial e transição

O DocumentDB foi substituído por MongoDB no EKS após `FreeTierRestrictionError` na conta. Esse componente não exige upgrade do plano. A elegibilidade, os créditos e a cobrança dos demais recursos continuam sujeitos às condições da conta; o workflow não altera seu plano.

Para atualizar uma tentativa parcial: publicar Infra, DB e Execution, reaplicar o bootstrap administrativo (permissão do segredo fiap-mongodb-admin), publicar o principal e iniciar um novo apply. Preservar o state: o plano remove os recursos/segredo DocumentDB eventualmente existentes e cria a topologia MongoDB; não há migração de dados. A role mantém acesso ao segredo antigo somente para sua limpeza. O inventário ainda verifica DocumentDB legado para evitar recursos abandonados.

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

Fluxo: core → database (RDS, credenciais e certificados MongoDB) → add-ons → configurações → messaging (Kafka e MongoDB) → inicialização dos bancos/compatibilidade → APIs → frontend → gateway → smoke → verificação de Collector/métricas/HPA. Imagens podem ser publicadas em paralelo após core; tags sha-<commit> são imutáveis e o deploy usa o digest resolvido.

Jobs no namespace fiap-data-bootstrap criam/atualizam usuários e grants. O PostgreSQL testa acesso ao próprio banco e rejeição cruzada. A mesma imagem Execution que será entregue executa --verify-persistence em fiap_compat_<run>, validando TLS, índices, transações, rollback, concorrência e deduplicação; o banco temporário é removido ao final. Esse Job usa credencial administrativa, nunca entregue aos pods da API. Falha bloqueia os rollouts sem troca automática de tecnologia.

Após sincronizar ExternalSecrets, OS/Billing executam --migrate-only em Jobs com a mesma imagem por digest e somente a conexão de seu banco. Não iniciam HTTP, JWT ou consumidores. Falha bloqueia o rollout; não há rollback automático de schema. Migrations precisam ser compatíveis com a versão em execução.

O smoke usa o endpoint público, testa frontend/readiness/JWT e conclui uma ordem real com pagamento Billing e execução, conferindo valor e estoque. Cria dados sintéticos exclusivos e desativa o usuário de teste ao terminar. Não registra senha, JWT ou connection strings. Os demais dados sintéticos permanecem no laboratório até destroy.

## Segredos e permissões

Senhas e JWT são aleatórios, mantidos no Secrets Manager. Os segredos de banco incluem metadados administrativos para inicialização; cada ExternalSecret de aplicação importa somente ConnectionStrings__DefaultConnection ou Mongo__ConnectionString. O operador usa Pod Identity e assume uma role diferente por SecretStore, com acesso aos ARNs exatos do namespace. Deploy não altera SecretStores, RBAC ou namespaces e não lê Secrets diretamente. Criar Jobs/Deployments permite executar código com as credenciais autorizadas ao próprio serviço, portanto entrega é uma permissão privilegiada dentro desse escopo.

Conexões PostgreSQL usam VerifyFull; o bundle público AWS é versionado no Infra. Revisar sua atualização em rotações de CA, conferindo a origem https://truststore.pki.rds.amazonaws.com/global/global-bundle.pem . Hash inicial SHA-256: 90a6f1a7978a1b1b096dca6c57511ca48d3b8cf052f8e704baf358f34ea2f21b.

MongoDB exige TLS com CA própria e hostname validado. Terraform DB gera CA, certificado do servidor (um ano), keyfile e senha administrativa. O segredo fiap-mongodb-admin chega somente ao namespace do servidor e aos Jobs administrativos. Execution recebe a credencial readWrite e o ConfigMap mongodb-ca com a CA pública em /certs/ca.pem; Mongo:UseDocumentDb=false. O Service headless anuncia mongodb-0.mongodb.fiap-mongodb.svc.cluster.local e permite DNS antes da readiness para a eleição inicial. NetworkPolicy limita 27017 à API Execution, ao bootstrap e ao próprio MongoDB. Após renovar certificado/keyfile, reiniciar o StatefulSet para recarregar os arquivos; mudanças de senha exigem atualizar também o usuário persistido, não apenas o segredo.

Terraform gera credenciais, portanto elas também existem no state. O bucket S3 é criptografado, privado, versionado e protegido contra destruição; credenciais e versões antigas do state permanecem sensíveis mesmo após apagar bancos. Infra/DB têm escritas separadas por prefixo e leituras explícitas de dependências. Remote state não constitui isolamento de segredos entre essas roles. As permissões de plataforma EC2/EKS/KMS continuam amplas dentro dos serviços necessários ao provisionamento; isto não é um perfil de produção.

## Destruição, retomada e limitações

Ordem: gateway → aplicações/Ingresses/ALB → Kafka e MongoDB/PVCs/EBS → configurações → add-ons → RDS/segredos → EKS/ECR/rede. Aguardar a exclusão dos volumes Kafka/MongoDB antes de remover o EBS CSI. O Terraform do gateway recebe o listener por input e o registra em output, evitando redescobrir ALB durante destroy. A exclusão assíncrona dos segredos é aguardada antes da reconstrução. Estados vazios são ignorados; AccessDenied, falhas de conectividade e drift não são tratados como recurso ausente.

Apply e destroy compartilham concurrency.group fiap-platform, sem cancelar operação em andamento. As entradas manuais antigas de provisionamento/publicação/deploy foram retiradas; não operar os estágios avulsos durante uma execução central. Em falha parcial, corrigir a causa e repetir a mesma operação. O fluxo não desfaz automaticamente o que já foi criado. Se um cluster for removido externamente enquanto ainda houver objetos Kubernetes rastreados, recuperar o acesso/estado antes de continuar; a action não descarta esses objetos silenciosamente.

Os outputs de banco usam chaves lógicas `postgres_admin`/`mongodb_admin`, mas os ExternalSecrets/Secrets Kubernetes usam `database-postgres-admin`/`database-mongodb-admin`. Terraform e Jobs convertem underscores em hífens sem alterar as chaves do state nem os nomes AWS. Após atualizar Infra/DB com essa correção, iniciar novo apply central; não é necessário reaplicar o bootstrap IAM por causa dos nomes Kubernetes.

O New Relic só é instalado após a licença sincronizar via ExternalSecret Ready. Se kubernetes-configs falhar, a Action coleta condições, eventos e capacidade dos nós sem imprimir valores de segredos. O Helm mantém atomic; seus pods podem já ter sido removidos no diagnóstico, mas os eventos ainda podem mostrar a causa. A consulta manual também exige uma identidade com acesso ao Kubernetes, separado do acesso administrativo à API AWS.

Se o store indicar InvalidProviderConfig, verificar a cadeia IAM do ESO. Em 07/11/2026, CloudTrail confirmou `AccessDenied` em `sts:TagSession` para todas as roles fiap-eso-*. A correção fica na policy do controller em kubernetes-addons e na confiança das roles de namespace em kubernetes-configs, mantendo o escopo dos segredos. Publicar Infra e executar novo apply completo; não repetir apenas configs da execução antiga. Não exige reaplicar bootstrap. Os stores recebem checksum de IAM para reconciliar após a atualização; a licença deve sincronizar antes do Helm e liberar o Collector.

Em messaging, o resumo Plan pode acompanhar um planejamento parcial com erro anterior. A pipeline identifica a fase/linha em uma anotação de erro e coleta diagnóstico de fiap-messaging/fiap-mongodb em falha. O ID Kafka continua gerado em Terraform e é entregue pelo ConfigMap kafka-cluster; o manifesto estático permite o primeiro plano com ID ainda desconhecido, coberto por teste Terraform. Não é necessário aplicar random_id separadamente nem descartar o state para provisionar o broker.

Destroy apaga bancos sem snapshot final, segredos de runtime, mensagens/offsets Kafka, volumes e imagens ECR. RDS tem retenção automática zero; MongoDB usa PVC com retenção Delete ao excluir o StatefulSet. Bootstrap, IAM das pipelines e versões do state permanecem e podem ter custo residual S3. Dados/telemetria já enviados ao New Relic seguem a retenção desse serviço externo.

Validações locais não comprovam IAM efetivo, oferta regional, montagem EBS no EKS, ingestão New Relic ou o ciclo AWS. As Actions validam oferta regional e executam os gates de persistência/smoke; confirmar a ingestão de telemetria no New Relic no ambiente implantado e guardar evidências. Não utilizar este laboratório para dados únicos ou produção.

## Evidências locais desta implementação (2026-11-07)

- Sete raízes Terraform: fmt/validate aprovados, com providers inicializados e backend desabilitado.
- Workflows dos cinco repositórios ativos: actionlint aprovado; scripts Node/Bash com sintaxe verificada.
- Manifests renderizados: 21 recursos nativos aprovados no kubeconform; seis ExternalSecrets excluídos explicitamente da validação nativa (CRDs verificados por dry-run no cluster durante deploy).
- Três APIs compiladas em Release, sem avisos/erros.
- 25 testes Node de entrega aprovados; 11 testes Execution de persistência/transações/TLS aprovados. Os dois testes do gate foram repetidos após o ajuste da limpeza e passaram.
- PostgreSQL 18.3 temporário com TLS: bootstrap e isolamento repetidos, seguido de migrations OS/Billing repetidas. O ensaio final usou administrador com CREATEDB/CREATEROLE e sem SUPERUSER, exercitando o limite de permissões do RDS.
- Kafka 4.1.0 com a imagem/configuração dos manifests, usuário 1000 e volume temporário: quatro tópicos criados e mensagem preservada após reinício.
- Containers/volume/rede temporários removidos após os ensaios; nenhuma operação de provisionamento/destruição foi executada na AWS.

O gate MongoDB, o smoke público, o teste de HPA/Collector e o ensaio criar/destruir/recriar AWS estão implementados para execução pela action. Seus resultados remotos ainda dependem da publicação coordenada/bootstrap e da execução no EKS. O ensaio local scripts/platform/test-mongodb.mjs do Infra verifica TLS obrigatório/hostname, autenticação, grants idempotentes/isolados, commit/rollback e persistência após reinício em recursos Docker exclusivos.

Na substituição por MongoDB, passaram 180 testes Execution com cobertura de linhas de 94,3%, sete testes de entrega Node, fmt/validate das cinco raízes Terraform alteradas, actionlint e validação dos manifests nativos. O ensaio MongoDB passou também com a imagem real Execution executando --verify-persistence via TLS, incluindo limpeza do banco temporário. Containers, volumes e rede exclusivos do ensaio foram removidos. Esses resultados são locais; nenhuma mudança foi aplicada na AWS nesta substituição.
