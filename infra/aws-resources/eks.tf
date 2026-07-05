module "eks" {
  count = var.create_eks_instance ? 1 : 0

  source  = "terraform-aws-modules/eks/aws"
  version = "21.24.0"

  name               = "fiap-eks-cluster"
  kubernetes_version = "1.36"

  endpoint_public_access                   = true
  endpoint_private_access                  = true
  enable_cluster_creator_admin_permissions = true

  access_entries = {
    github_actions_infra = {
      principal_arn = data.terraform_remote_state.bootstrap.outputs.github_actions_infra_role_arn

      policy_associations = {
        cluster_admin = {
          policy_arn = "arn:aws:eks::aws:cluster-access-policy/AmazonEKSClusterAdminPolicy"
          access_scope = {
            type = "cluster"
          }
        }
      }
    }
  }

  vpc_id     = module.vpc.vpc_id
  subnet_ids = module.vpc.public_subnets

  addons = {
    coredns    = {}
    kube-proxy = {}
    vpc-cni = {
      before_compute = true
    }

    eks-pod-identity-agent = {
      before_compute = true
    }
  }

  eks_managed_node_groups = {
    fiap-node-group = {
      instance_types = ["t3.small"]

      min_size     = 1
      max_size     = 3
      desired_size = 2

      iam_role_additional_policies = {
        AmazonEKSWorkerNodePolicy          = "arn:aws:iam::aws:policy/AmazonEKSWorkerNodePolicy"
        AmazonEC2ContainerRegistryReadOnly = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly"
        AmazonEKS_CNI_Policy               = "arn:aws:iam::aws:policy/AmazonEKS_CNI_Policy"
      }
    }
  }
}