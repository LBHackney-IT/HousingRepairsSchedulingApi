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

        private readonly BookAppointmentUseCase _systemUnderTest;
        private readonly Mock<IAppointmentsGateway> _appointmentsGatewayMock;

        public BookAppointmentUseCaseTests()
        {
            _appointmentsGatewayMock = new Mock<IAppointmentsGateway>();
            _systemUnderTest = new BookAppointmentUseCase(_appointmentsGatewayMock.Object);
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenAnInvalidBookingReference_WhenExecute_ThenExceptionIsThrown<T>(T exception, string bookingReference) where T : Exception
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
            Func<Task> act = async () => await _systemUnderTest.Execute(request);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenAnInvalidSorCode_WhenExecute_ThenExceptionIsThrown<T>(T exception, string sorCode) where T : Exception
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
            Func<Task> act = async () => await _systemUnderTest.Execute(request);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenAnInvalidLocationId_WhenExecute_ThenExceptionIsThrown<T>(T exception, string locationId) where T : Exception
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
            Func<Task> act = async () => await _systemUnderTest.Execute(request);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Fact]
        public async void GivenAnEndDateEarlierThanTheStartDate_WhenExecute_ThenInvalidExceptionIsThrown()
        {
            // Arrange
            var startDate = new DateTime(2022, 1, 21);
            var endDate = startDate.AddDays(-1);

            var request = new BookAppointmentRequest
            {
                BookingReference = BookingReference,
                SorCode = SorCode,
                LocationId = LocationId,
                StartDateTime = startDate,
                EndDateTime = endDate
            };

            // Act
            Func<Task> act = async () =>
                await _systemUnderTest.Execute(request);

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
        public async void GivenValidArguments_WhenExecute_ThenBookingIdIsReturned()
        {
            // Arrange
            _appointmentsGatewayMock
                .Setup(x => x.BookAppointment(It.IsAny<BookAppointmentRequest>()))
                .ReturnsAsync(BookingReference);

            var startDateTime = It.IsAny<DateTime>();

            var request = new BookAppointmentRequest
            {
                BookingReference = BookingReference,
                SorCode = SorCode,
                LocationId = LocationId,
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddDays(1)
            };

            // Act
            var actual = await _systemUnderTest.Execute(request);

            // Assert
            Assert.Equal(BookingReference, actual);
        }
    }
}
