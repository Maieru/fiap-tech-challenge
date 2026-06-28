resource "aws_db_instance" "rds-postgresql-instance" {
  count = var.create_rds_instance ? 1 : 0

  identifier              = "fiap-rds-postgresql-instance"
  allocated_storage       = 10
  db_name                 = "fiap_tech_challenge"
  engine                  = "postgres"
  engine_version          = "18.3"
  instance_class          = "db.t4g.micro"
  username                = var.db_username
  password                = var.db_password
  skip_final_snapshot     = true
  backup_retention_period = 0
  apply_immediately       = true
}
