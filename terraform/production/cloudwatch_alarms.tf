locals {
  namespace = "HousingRepairsSchedulingApi-Businesslogic-Metrics"
  log_group_name = "HousingRepairsSchedulingApi-production"
}

resource "aws_sns_topic" "housing-repairs-scheduling-canary" {
  name = "housing-repairs-scheduling-canary"
}

// Matching Exceptions
// "An error was thrown when calling _drsSoapClient.scheduleBookingAsync"
// "An error was thrown when calling _drsSoapClient.createOrderAsync"
// "An error was thrown when calling _drsSoapClient.checkAvailabilityAsync"

resource "aws_cloudwatch_log_metric_filter" "housingrepairsschedulingapi-drs-exceptions-filter" {
  name           = "HousingRepairsSchedulingAPI DRS Exceptions"
  pattern        = "An error was thrown when calling _drsSoapClient"
  log_group_name = local.log_group_name

  metric_transformation {
    name      = "HousingRepairsSchedulingAPI DRS Exceptions"
    namespace = local.namespace
    value     = "1"
  }
}

resource "aws_cloudwatch_metric_alarm" "housingrepairsschedulingapi-drs-exceptions-alarm" {
  alarm_name          = "housingrepairsschedulingapi-drs-exceptions"
  metric_name         = aws_cloudwatch_log_metric_filter.housingrepairsschedulingapi-drs-exceptions-filter.name
  threshold           = "3"
  statistic           = "Sum"
  comparison_operator = "GreaterThanThreshold"
  datapoints_to_alarm = "1"
  evaluation_periods  = "1"
  period              = "30"
  namespace           = local.namespace
  alarm_actions       = [aws_sns_topic.housing-repairs-scheduling-canary.arn]
}
