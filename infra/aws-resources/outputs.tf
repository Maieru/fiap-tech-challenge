output "eks" {
  value = {
    name     = module.eks[0].cluster_name
    endpoint = module.eks[0].cluster_endpoint
    ca       = module.eks[0].cluster_certificate_authority_data
  }
}

output "backend_secret_arn" {
  description = "ARN do secret utilizado pelo backend"
  value = aws_secretsmanager_secret.secret-manager-backend[0].arn
}