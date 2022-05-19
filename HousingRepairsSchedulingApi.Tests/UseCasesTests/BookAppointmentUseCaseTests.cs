namespace HousingRepairsSchedulingApi.Tests.UseCasesTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Gateways;
    using HousingRepairsSchedulingApi.Domain;
    using Moq;
    using UseCases;
    using Xunit;

    public class BookAppointmentUseCaseTests
    {
        private const string BookingReference = "BookingReference";
        private const string SorCode = "SOR Code";
        private const string LocationId = "locationId";

        private readonly BookAppointmentUseCase systemUnderTest;
        private readonly Mock<IAppointmentsGateway> appointmentsGatewayMock;

        public BookAppointmentUseCaseTests()
        {
            this.appointmentsGatewayMock = new Mock<IAppointmentsGateway>();
            this.systemUnderTest = new BookAppointmentUseCase(this.appointmentsGatewayMock.Object);
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenAnInvalidBookingReference_WhenExecute_ThenExceptionIsThrown<T>(T exception, string bookingReference) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await this.systemUnderTest.Execute(bookingReference, SorCode, LocationId,
                It.IsAny<DateTime>(), It.IsAny<DateTime>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenAnInvalidSorCode_WhenExecute_ThenExceptionIsThrown<T>(T exception, string sorCode) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await this.systemUnderTest.Execute(BookingReference, sorCode, LocationId,
                It.IsAny<DateTime>(), It.IsAny<DateTime>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenAnInvalidLocationId_WhenExecute_ThenExceptionIsThrown<T>(T exception, string locationId) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await this.systemUnderTest.Execute(BookingReference, SorCode, locationId,
                It.IsAny<DateTime>(), It.IsAny<DateTime>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenAnEndDateEarlierThanTheStartDate_WhenExecute_ThenInvalidExceptionIsThrown()
#pragma warning restore CA1707
        {
            // Arrange
            var startDate = new DateTime(2022, 1, 21);
            var endDate = startDate.AddDays(-1);

            // Act
            Func<Task> act = async () =>
                await this.systemUnderTest.Execute(BookingReference, SorCode, LocationId, startDate, endDate);

            // Assert
            await act.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
        }

        public static IEnumerable<object[]> InvalidArgumentTestData()
        {
            yield return new object[] { new ArgumentNullException(), null };
            yield return new object[] { new ArgumentException(), "" };
            yield return new object[] { new ArgumentException(), " " };
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenValidArguments_WhenExecute_ThenCorrectBookingIdIsReturned()
#pragma warning restore CA1707
        {
            // Arrange

            var response = new SchedulingApiBookingResponse
            {
                BookingReference = BookingReference
            };

            this.appointmentsGatewayMock.Setup(x => x.BookAppointment(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()
                )
            ).ReturnsAsync(response);

            // Act
            var startDateTime = It.IsAny<DateTime>();
            var actual = await this.systemUnderTest.Execute(BookingReference, SorCode, LocationId,
                startDateTime, startDateTime.AddDays(1));

            // Assert
            Assert.Equal(BookingReference, actual.BookingReference);
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenValidArguments_WhenExecute_ThenCorrectDetailsAreReturned()
#pragma warning restore CA1707
        {
            // Arrange
            var expectedGuid = Guid.NewGuid().ToString();

            var response = new SchedulingApiBookingResponse
            {
                BookingReference = BookingReference,
                TokenId = expectedGuid
            };

            this.appointmentsGatewayMock.Setup(x => x.BookAppointment(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()
                )
            ).ReturnsAsync(response);

            // Act
            var startDateTime = It.IsAny<DateTime>();
            var actual = await this.systemUnderTest.Execute(BookingReference, SorCode, LocationId,
                startDateTime, startDateTime.AddDays(1));

            // Assert
            Assert.Equal(BookingReference, actual.BookingReference);
            Assert.Equal(expectedGuid, actual.TokenId);
        }
    }
}
