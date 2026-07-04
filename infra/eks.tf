module "eks" {
  count = var.create_eks_instance ? 1 : 0

  source  = "terraform-aws-modules/eks/aws"
  version = "21.24.0"

  name               = "fiap-eks-cluster"
  kubernetes_version = "1.36"

  endpoint_public_access                   = true
  endpoint_private_access                  = true
  enable_cluster_creator_admin_permissions = true

  vpc_id     = module.vpc.vpc_id
  subnet_ids = module.vpc.public_subnets

  addons = {
    coredns                = {}
    kube-proxy             = {}
    vpc-cni                = {}
    eks-pod-identity-agent = {}
  }

  eks_managed_node_groups = {
    fiap-node-group = {
      instance_types = ["t3.small"]

      min_size     = 1
      max_size     = 2
      desired_size = 1
    }
  }
}
