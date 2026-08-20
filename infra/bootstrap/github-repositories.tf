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
      repository = "fiap-tech-challenge-k8s-infra"
      role_name  = "fiap-role-github-actions-k8s-infra"
    }

    database_infra = {
      repository = "fiap-tech-challenge-database-infra"
      role_name  = "fiap-role-github-actions-database-infra"
    }
  }
}
