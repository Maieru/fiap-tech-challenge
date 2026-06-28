variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "github_owner" {
  type    = string
  default = "Maieru"
}

variable "github_repository" {
  type    = string
  default = "fiap-tech-challenge"
}

variable "github_branch" {
  type    = string
  default = "main"
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