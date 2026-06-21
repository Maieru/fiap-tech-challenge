terraform {
  backend "s3" {
    bucket = "fiap-s3-terraform-backend"
    key    = "terraform/terraform.tfstate"
    region = "us-east-1"
  }
}