output "github_actions_infra_role_arn" {
  value       = aws_iam_role.github_actions_infra.arn
  description = "ARN da IAM Role usada pelo GitHub Actions para Terraform infra"
}

output "github_actions_role_arn" {
  value       = aws_iam_role.github_actions.arn
  description = "ARN da IAM Role usada pelo GitHub Actions"
}