resource "aws_ssm_parameter" "person_api_url" {
  name  = "/housing-tl/pre-production/person-api-url"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "person_api_token" {
  name  = "/housing-tl/pre-production/person-api-token"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}
