using System;
using FluentAssertions;
using Xunit;

namespace HousingRepairsSchedulingApi.Tests.HelpersTests
{
    using Helpers;

    public class DrsHelpersTests
    {
        [InlineData(1, 9, 9)] // when london is in GMT+0 no change
        [InlineData(6, 9, 10)] // when london is in GMT+1 shift an hour
        [Theory]
        public void MapsToDrsTime(int month, int hourIn, int expectedHour)
        {
            // Arrange
            var timeIn = new DateTime(2020, month, 01, hourIn, 00, 00, DateTimeKind.Utc);
            var expected = new DateTime(2020, month, 01, expectedHour, 00, 00, DateTimeKind.Utc);

            // Act
            var result = DrsHelpers.ConvertToDrsTimeZone(timeIn);

            // Assert
            result.Should().Be(expected);
        }

        [InlineData(1, 9, 9)] // when london is in GMT+0 no change
        [InlineData(6, 9, 8)] // when london is in GMT+1 shift an hour
        [Theory]
        public void MapsFromDrsTime(int month, int hourIn, int expectedHour)
        {
            // Arrange
            var timeIn = new DateTime(2020, month, 01, hourIn, 00, 00, DateTimeKind.Utc);
            var expected = new DateTime(2020, month, 01, expectedHour, 00, 00, DateTimeKind.Utc);

            // Act
            var result = DrsHelpers.ConvertFromDrsTimeZone(timeIn);

            // Assert
            result.Should().Be(expected);
        }
    }
}
