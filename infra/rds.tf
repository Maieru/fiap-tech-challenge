# resource "aws_db_instance" "rds-postgresql-instance" {
#   identifier              = "fiap-rds-postgresql-instance"
#   allocated_storage       = 10
#   db_name                 = "fiap_tech_challenge"
#   engine                  = "postgres"
#   engine_version          = "18.3"
#   instance_class          = "db.t4g.micro"
#   username                = "teste"
#   password                = "b1db1be1-2564-42e2-892b-57901739f797"
#   skip_final_snapshot     = true
#   backup_retention_period = 0
#   apply_immediately       = true
# }
