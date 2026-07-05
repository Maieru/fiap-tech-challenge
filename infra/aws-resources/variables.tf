variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "db_username" {
  type      = string
  sensitive = true
}

variable "db_password" {
  type      = string
  sensitive = true
}

variable "jwt_signing_key" {
  type      = string
  sensitive = true
}

variable "create_rds_instance" {
  type    = bool
  default = true
}

variable "create_eks_instance" {
  type    = bool
  default = true
}