locals {
  namespace = "HousingRepairsSchedulingApi-Businesslogic-Metrics"
  log_group_name = "/aws/lambda/HousingRepairsSchedulingApi-staging"
}

resource "aws_sns_topic" "housing-repairs-scheduling-canary" {
  name = "housing-repairs-scheduling-canary"
}

resource "aws_cloudwatch_log_metric_filter" "housingrepairsschedulingapi-drs-createorderasync-exceptions-filter" {
  name           = "HousingRepairsSchedulingAPI DRS createOrderAsync Exceptions"
  pattern        = "createOrderAsync returned an invalid response for"
  log_group_name = local.log_group_name

  metric_transformation {
    name      = "HousingRepairsSchedulingAPI DRS createOrderAsync Exceptions"
    namespace = local.namespace
    value     = "1"
  }
}

resource "aws_cloudwatch_log_metric_filter" "housingrepairsschedulingapi-drs-checkavailabilityasync-exceptions-filter" {
  name           = "HousingRepairsSchedulingAPI DRS checkAvailabilityAsync Exceptions"
  pattern        = "checkAvailabilityAsync returned an invalid response for"
  log_group_name = local.log_group_name

  metric_transformation {
    name      = "HousingRepairsSchedulingAPI DRS checkAvailabilityAsync Exceptions"
    namespace = local.namespace
    value     = "1"
  }
}

resource "aws_cloudwatch_metric_alarm" "housingrepairsschedulingapi-drs-createorderasync-exceptions-alarm" {
  alarm_name          = "housingrepairsschedulingapi-drs-createorderasync-exceptions"
  metric_name         = aws_cloudwatch_log_metric_filter.housingrepairsschedulingapi-drs-createorderasync-exceptions-filter.name
  threshold           = "3"
  statistic           = "Sum"
  comparison_operator = "GreaterThanThreshold"
  datapoints_to_alarm = "1"
  evaluation_periods  = "1"
  period              = "30"
  namespace           = local.namespace
  alarm_actions       = [aws_sns_topic.housing-repairs-scheduling-canary.arn]
}

resource "aws_cloudwatch_metric_alarm" "housingrepairsschedulingapi-drs-checkavailabilityasync-exceptions-alarm" {
  alarm_name          = "housingrepairsschedulingapi-drs-checkavailabilityasync-exceptions"
  metric_name         = aws_cloudwatch_log_metric_filter.housingrepairsschedulingapi-drs-checkavailabilityasync-exceptions-filter.name
  threshold           = "3"
  statistic           = "Sum"
  comparison_operator = "GreaterThanThreshold"
  datapoints_to_alarm = "1"
  evaluation_periods  = "1"
  period              = "30"
  namespace           = local.namespace
  alarm_actions       = [aws_sns_topic.housing-repairs-scheduling-canary.arn]
}
