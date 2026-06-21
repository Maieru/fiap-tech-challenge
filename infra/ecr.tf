resource "aws_ecr_repository" "ecr-repository" {
  name                 = "fiap-ecr-repository"
  image_tag_mutability = "IMMUTABLE_WITH_EXCLUSION"

  image_tag_mutability_exclusion_filter {
    filter      = "latest*"
    filter_type = "WILDCARD"
  }
}
