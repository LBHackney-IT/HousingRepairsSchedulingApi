provider "aws" {
  region  = "eu-west-2"
}
data "aws_caller_identity" "current" {}
data "aws_region" "current" {}
locs {
  application_name = "housing-repairs-scheduling-api" # The name to use for your application
}

terraform {
  required_providers {
    aws = {
      source = "registry.terraform.io/hashicorp/aws"
      version = "~> 4.23.0"
    }
  }
  backend "s3" {
    bucket  = "terraform-state-housing-staging"
    encrypt = true
    region  = "eu-west-2"
    key     = "services/housing-repairs-scheduling-api/state"
  }
}