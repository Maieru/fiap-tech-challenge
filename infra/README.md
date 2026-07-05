# Infraestrutura AWS

Esta pasta provisiona a infraestrutura do projeto na AWS com Terraform. Os estados são separados e possuem dependências entre si; por isso, a ordem de execução deve ser respeitada.

## Componentes

| Diretório | Responsabilidade |
| --- | --- |
| `bootstrap` | Cria o bucket S3 do state, o provedor OIDC do GitHub e as IAM Roles usadas pelas pipelines. |
| `aws-resources` | Cria VPC, EKS, RDS PostgreSQL, repositórios ECR e o segredo do backend no Secrets Manager. |
| `kubernetes-addons` | Instala External Secrets e Metrics Server no EKS. |
| `kubernetes-configs` | Cria os namespaces e o SecretStore definidos em `../k8s/*/infra`. |

A ordem obrigatória é:

```text
bootstrap → aws-resources → kubernetes-addons → kubernetes-configs
```

## Pré-requisitos

- uma conta AWS e credenciais com permissão para criar S3, IAM, VPC/EC2, EKS, RDS, ECR, Secrets Manager e recursos relacionados;
- Terraform `1.13.x`;
- AWS CLI v2 configurada;
- `kubectl`, para validar o cluster e aplicar os manifests da aplicação;
- Docker, caso as imagens sejam publicadas localmente;
- acesso administrativo ao repositório GitHub, caso as pipelines sejam utilizadas.

Todos os recursos usam, por padrão, a região `us-east-1`.

Antes de executar, revise os valores fixos do projeto:

- `bootstrap/variables.tf`: proprietário, nome do repositório e branch do GitHub;
- o nome `fiap-s3-terraform-backend`: buckets S3 têm nomes globalmente únicos. Se ele for alterado, atualize todos os arquivos `backend.tf` e `remote-state.tf`;
- as imagens em `../k8s/*/application/deployment.yaml`: o ID da conta e os endereços dos repositórios ECR devem corresponder à conta usada no deploy.

## Primeira execução

Execute os comandos abaixo a partir da raiz do repositório.

### 1. Autenticar na AWS

Use o método de autenticação adotado pela sua conta, por exemplo `aws configure` ou AWS IAM Identity Center, e confirme a identidade ativa:

```powershell
aws login
```

### 2. Criar o backend do Terraform

Na primeira execução, o bucket S3 ainda não existe. Inicialize o `bootstrap` sem backend remoto, crie os recursos e depois migre o state local para o S3:

```powershell
terraform -chdir=infra/bootstrap init -backend=false
terraform -chdir=infra/bootstrap fmt
terraform -chdir=infra/bootstrap validate
terraform -chdir=infra/bootstrap plan -out=.terraform/tfplan
terraform -chdir=infra/bootstrap apply .terraform/tfplan
terraform -chdir=infra/bootstrap init -migrate-state
```

Confirme a migração quando o Terraform solicitar. Ao final, confira os outputs:

```powershell
terraform -chdir=infra/bootstrap output
```

> Nas execuções seguintes, não use `-backend=false`: o backend já estará disponível no S3.

### 3. Informar as variáveis sensíveis

Crie `infra/aws-resources/terraform.tfvars` com os valores do ambiente:

```hcl
db_username     = "usuario_do_banco"
db_password     = "uma_senha_forte"
jwt_signing_key = "uma_chave_jwt_longa_e_aleatoria"
```

O arquivo `*.tfvars` é ignorado pelo Git e não deve ser versionado. Os valores sensíveis também ficam armazenados no state; mantenha o bucket privado e o acesso restrito.

### 4. Criar os recursos AWS

```powershell
terraform -chdir=infra/aws-resources init
terraform -chdir=infra/aws-resources fmt
terraform -chdir=infra/aws-resources validate
terraform -chdir=infra/aws-resources plan -out=.terraform/tfplan
terraform -chdir=infra/aws-resources apply .terraform/tfplan
```

Essa etapa pode levar vários minutos, principalmente durante a criação do EKS e do RDS.

### 5. Configurar o acesso ao EKS

```powershell
aws eks update-kubeconfig --name fiap-eks-cluster --region us-east-1
kubectl get nodes
```

Prossiga somente quando os nós estiverem com status `Ready`.

### 6. Instalar os add-ons do Kubernetes

O External Secrets precisa existir antes da criação do `SecretStore`.

```powershell
terraform -chdir=infra/kubernetes-addons init
terraform -chdir=infra/kubernetes-addons fmt
terraform -chdir=infra/kubernetes-addons validate
terraform -chdir=infra/kubernetes-addons plan -out=.terraform/tfplan
terraform -chdir=infra/kubernetes-addons apply .terraform/tfplan
```

Valide a instalação:

```powershell
kubectl rollout status deployment/external-secrets -n external-secrets --timeout=2m
kubectl get deployment metrics-server -n kube-system
```

### 7. Criar namespaces e SecretStore

```powershell
terraform -chdir=infra/kubernetes-configs init
terraform -chdir=infra/kubernetes-configs fmt
terraform -chdir=infra/kubernetes-configs validate
terraform -chdir=infra/kubernetes-configs plan -out=.terraform/tfplan
terraform -chdir=infra/kubernetes-configs apply .terraform/tfplan
```

Confira os recursos criados:

```powershell
kubectl get namespaces fiap-backend fiap-frontend
kubectl get secretstore -n fiap-backend
```

### 8. Configurar o GitHub Actions

Obtenha as IAM Roles criadas pelo `bootstrap`:

```powershell
terraform -chdir=infra/bootstrap output -raw github_actions_role_arn
terraform -chdir=infra/bootstrap output -raw github_actions_infra_role_arn
```

Cadastre no repositório GitHub:

| Secret | Valor |
| --- | --- |
| `ACTION_ROLE_ARN` | output `github_actions_role_arn` |
| `INFRA_ACTION_ROLE` | output `github_actions_infra_role_arn` |
| `db_username` | usuário do RDS |
| `db_password` | senha do RDS |
| `jwt_signing_key` | chave de assinatura do JWT |

Crie também o environment `production`, utilizado pelas pipelines de infraestrutura e deploy.

Nas execuções automatizadas, o workflow `Apply Infrastructure` processa todos os estados na ordem obrigatória:

```text
bootstrap → aws-resources → kubernetes-addons → kubernetes-configs
```

Cada estágio gera seu próprio plano, aguarda a aprovação configurada no environment `production`, aplica o plano e somente então libera o estágio seguinte. O primeiro `bootstrap` continua sendo manual, pois o bucket do state e a IAM Role usada pelo próprio GitHub Actions ainda não existem nesse momento.

### 9. Publicar e implantar as aplicações

Depois que os repositórios ECR existirem, execute no GitHub Actions, nesta ordem:

1. `Build, Test And Push Backend`;
2. `Push Frontend`;
3. `Deploy Applications`.

Para aplicar os manifests manualmente, depois de publicar as imagens:

```powershell
kubectl apply -f k8s/backend/application
kubectl apply -f k8s/frontend/application
kubectl rollout status deployment/fiap-backend-deployment -n fiap-backend --timeout=5m
kubectl rollout status deployment/fiap-frontend-deployment -n fiap-frontend --timeout=5m
```

## Execuções seguintes

Em alterações rotineiras, execute somente os módulos afetados, sempre preservando a ordem das dependências:

1. `bootstrap`, apenas se houver mudanças no backend, OIDC ou IAM das pipelines;
2. `aws-resources`;
3. atualize o acesso ao EKS com `aws eks update-kubeconfig`;
4. `kubernetes-addons`;
5. `kubernetes-configs`;
6. publique as imagens e aplique os manifests da aplicação, quando necessário.

Para cada módulo Terraform alterado, use o mesmo ciclo:

```powershell
terraform -chdir=infra/<modulo> init
terraform -chdir=infra/<modulo> fmt
terraform -chdir=infra/<modulo> validate
terraform -chdir=infra/<modulo> plan -out=.terraform/tfplan
terraform -chdir=infra/<modulo> apply .terraform/tfplan
```

Nunca execute `apply` sem revisar o plano, especialmente para EKS, RDS e IAM.

## Verificação do ambiente

```powershell
aws sts get-caller-identity
terraform -chdir=infra/aws-resources state list
kubectl get nodes
kubectl get pods -A
kubectl get externalsecret -n fiap-backend
kubectl get services -n fiap-backend
kubectl get services -n fiap-frontend
```

Para obter o endereço público do frontend:

```powershell
kubectl get service fiap-frontend-service -n fiap-frontend
```

## Destruição

Se for necessário remover todo o ambiente, destrua os módulos na ordem inversa para respeitar as dependências:

```text
kubernetes-configs → kubernetes-addons → aws-resources → bootstrap
```

Revise cuidadosamente cada plano de destruição. O RDS está configurado sem snapshot final e o bucket do state usa `force_destroy`, portanto a exclusão pode causar perda permanente de dados e do histórico de estado.
