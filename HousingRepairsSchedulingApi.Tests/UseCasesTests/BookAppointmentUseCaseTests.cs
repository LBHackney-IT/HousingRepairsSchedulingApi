namespace HousingRepairsSchedulingApi.Tests.UseCasesTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Gateways;
    using HousingRepairsSchedulingApi.Boundary.Requests;
    using HousingRepairsSchedulingApi.Gateways.Interfaces;
    using Moq;
    using UseCases;
    using Xunit;

    public class BookAppointmentUseCaseTests
    {
        private const string BookingReference = "BookingReference";
        private const string SorCode = "SOR Code";
        private const string LocationId = "locationId";

        private BookAppointmentUseCase systemUnderTest;
        private Mock<IAppointmentsGateway> appointmentsGatewayMock;

        public BookAppointmentUseCaseTests()
        {
            appointmentsGatewayMock = new Mock<IAppointmentsGateway>();
            systemUnderTest = new BookAppointmentUseCase(appointmentsGatewayMock.Object);
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
            var request = new BookAppointmentRequest
            {
                BookingReference = bookingReference,
                SorCode = SorCode,
                LocationId = LocationId,
                StartDateTime = It.IsAny<DateTime>(),
                EndDateTime = It.IsAny<DateTime>()
            };

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(request);

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
            var request = new BookAppointmentRequest
            {
                BookingReference = BookingReference,
                SorCode = sorCode,
                LocationId = LocationId,
                StartDateTime = It.IsAny<DateTime>(),
                EndDateTime = It.IsAny<DateTime>()
            };

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(request);

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
            var request = new BookAppointmentRequest
            {
                BookingReference = BookingReference,
                SorCode = SorCode,
                LocationId = locationId,
                StartDateTime = It.IsAny<DateTime>(),
                EndDateTime = It.IsAny<DateTime>()
            };

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(request);

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

            var request = new BookAppointmentRequest
            {
                BookingReference = BookingReference,
                SorCode = SorCode,
                LocationId = LocationId,
                StartDateTime = startDate,
                EndDateTime = startDate.AddDays(-1)
            };

            // Act
            Func<Task> act = async () =>
                await systemUnderTest.Execute(request);

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
        public async void GivenValidArguments_WhenExecute_ThenBookingIdIsReturned()
#pragma warning restore CA1707
        {
            // Arrange
            var startDateTime = It.IsAny<DateTime>();

            var request = new BookAppointmentRequest
            {
                BookingReference = BookingReference,
                SorCode = SorCode,
                LocationId = LocationId,
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddDays(1)
            };

            this.appointmentsGatewayMock
                .Setup(x => x.BookAppointment(request)).
                ReturnsAsync(BookingReference);


            // Act
            var actual = await systemUnderTest.Execute(request);

            // Assert
            Assert.Equal(BookingReference, actual);
        }
    }
}
