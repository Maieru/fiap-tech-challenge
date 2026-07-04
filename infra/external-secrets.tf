resource "aws_iam_role" "external_secrets" {
  count = var.create_eks_instance ? 1 : 0

  name = "fiap-role-external-secrets"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "pods.eks.amazonaws.com"
        }
        Action = [
          "sts:AssumeRole",
          "sts:TagSession"
        ]
      }
    ]
  })
}

resource "aws_iam_role_policy" "external_secrets" {
  count = var.create_eks_instance ? 1 : 0

  name = "fiap-policy-external-secrets"
  role = aws_iam_role.external_secrets[0].id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "secretsmanager:DescribeSecret",
          "secretsmanager:GetSecretValue"
        ]
        Resource = "arn:aws:secretsmanager:${var.aws_region}:${data.aws_caller_identity.current.account_id}:secret:fiap-secret-manager-backend-*"
      }
    ]
  })
}

resource "aws_eks_pod_identity_association" "external_secrets" {
  count = var.create_eks_instance ? 1 : 0

  cluster_name    = module.eks[0].cluster_name
  namespace       = "external-secrets"
  service_account = "external-secrets"
  role_arn        = aws_iam_role.external_secrets[0].arn

  depends_on = [
    aws_iam_role_policy.external_secrets,
    helm_release.external_secrets
  ]
}
