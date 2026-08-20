locals {
  github_repositories = {
    app = {
      repository = "fiap-tech-challenge"
      role_name  = "fiap-role-github-actions-app"
    }

    auth = {
      repository = "fiap-tech-challenge-auth"
      role_name  = "fiap-role-github-actions-auth"
    }

    k8s_infra = {
      repository = "fiap-tech-challenge-infra"
      role_name  = "fiap-role-github-actions-infra"
    }

    database_infra = {
      repository = "fiap-tech-challenge-database"
      role_name  = "fiap-role-github-actions-database"
    }
  }
}
