resource "aws_security_group" "rds_postgresql" {
  count = var.create_rds_instance ? 1 : 0

  name        = "fiap-rds-postgresql-sg"
  description = "Allow PostgreSQL access from the EKS nodes"
  vpc_id      = module.vpc.vpc_id

  tags = {
    Name = "fiap-rds-postgresql-sg"
  }
}

resource "aws_vpc_security_group_ingress_rule" "rds_postgresql_from_eks" {
  count = var.create_rds_instance && var.create_eks_instance ? 1 : 0

  security_group_id            = aws_security_group.rds_postgresql[0].id
  referenced_security_group_id = module.eks[0].node_security_group_id
  from_port                    = 5432
  to_port                      = 5432
  ip_protocol                  = "tcp"
}

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
  db_subnet_group_name    = module.vpc.database_subnet_group_name
  vpc_security_group_ids  = [aws_security_group.rds_postgresql[0].id]
  publicly_accessible     = true
  skip_final_snapshot     = true
  backup_retention_period = 0
  apply_immediately       = true
}
