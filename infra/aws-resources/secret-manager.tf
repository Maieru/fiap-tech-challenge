resource "aws_secretsmanager_secret" "secret-manager-backend" {
  count = var.create_rds_instance && var.create_eks_instance ? 1 : 0

  name                    = "fiap-secret-manager-backend"
  recovery_window_in_days = 0
}

resource "aws_secretsmanager_secret_version" "backend" {
  count = var.create_rds_instance && var.create_eks_instance ? 1 : 0

  secret_id = aws_secretsmanager_secret.secret-manager-backend[0].id

  secret_string = jsonencode({
    "ConnectionStrings__DefaultConnection" = "Host=${aws_db_instance.rds-postgresql-instance[0].address};Port=5432;Database=${aws_db_instance.rds-postgresql-instance[0].db_name};Username=${var.db_username};Password=${var.db_password}"
    "Jwt__SigningKey"                      = var.jwt_signing_key
  })
}
