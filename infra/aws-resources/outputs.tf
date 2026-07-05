output "eks" {
  value = var.create_eks_instance ? {
    name     = module.eks[0].cluster_name
    endpoint = module.eks[0].cluster_endpoint
    ca       = module.eks[0].cluster_certificate_authority_data
  } : null
}

output "backend_secret_arn" {
  description = "ARN do secret utilizado pelo backend"
  value       = try(aws_secretsmanager_secret.secret-manager-backend[0].arn, null)
}
