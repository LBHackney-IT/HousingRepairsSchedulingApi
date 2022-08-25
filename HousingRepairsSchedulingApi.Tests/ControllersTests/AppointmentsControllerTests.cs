using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace HousingRepairsSchedulingApi.Tests.ControllersTests
{
    using System;
    using Controllers;
    using HousingRepairsSchedulingApi.Boundary.Requests;
    using HousingRepairsSchedulingApi.UseCases.Interfaces;
    using UseCases;

    public class AppointmentsControllerTests : ControllerTests
    {
        private const string SorCode = "SOR Code";
        private const string LocationId = "locationId";

        private AppointmentsController _systemUndertest;
        private Mock<IRetrieveAvailableAppointmentsUseCase> _availableAppointmentsUseCaseMock;
        private Mock<IBookAppointmentUseCase> _bookAppointmentUseCaseMock;

        public AppointmentsControllerTests()
        {
            _availableAppointmentsUseCaseMock = new Mock<IRetrieveAvailableAppointmentsUseCase>();
            _bookAppointmentUseCaseMock = new Mock<IBookAppointmentUseCase>();

            _systemUndertest = new AppointmentsController(
                _availableAppointmentsUseCaseMock.Object,
                _bookAppointmentUseCaseMock.Object);
        }

        [Fact]
        public async Task TestAvailableAppointmentsEndpoint()
        {
            // Arrange
            var request = new GetAvailableAppointmentsRequest {
                SorCode = SorCode,
                LocationId = LocationId
            };

            // Act
            var result = await _systemUndertest.AvailableAppointments(request);

            // Assert
            GetStatusCode(result).Should().Be(200);

            _availableAppointmentsUseCaseMock.Verify(x => x.Execute(request), Times.Once);
        }


        [Fact]
        public async Task ReturnsErrorWhenFailsToGetAvailableAppointments()
        {
            // Arrange
            const string errorMessage = "An error message";

            _availableAppointmentsUseCaseMock
                .Setup(x => x.Execute(It.IsAny<GetAvailableAppointmentsRequest>()))
                .Throws(new Exception(errorMessage));

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = SorCode,
                LocationId = LocationId
            };
            // Act
            var result = await _systemUndertest.AvailableAppointments(request);

            // Assert
            GetStatusCode(result).Should().Be(500);
        }

        [Fact]
        public async Task TestBookAppointmentEndpoint()
        {
            // Arrange
            const string bookingReference = "bookingReference";
            var startDateTime = It.IsAny<DateTime>();
            var endDateTime = It.IsAny<DateTime>();

            var request = new BookAppointmentRequest
            {
                BookingReference = bookingReference,
                SorCode = SorCode,
                LocationId = LocationId,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime
            };

            // Act
            var result = await this._systemUndertest.BookAppointment(request);

            // Assert
            GetStatusCode(result).Should().Be(200);
        }

        [Fact]
#pragma warning disable CA1707
        public async Task GivenAFromDate_WhenRequestingAvailableAppointment_ThenResultsAreReturned()
#pragma warning restore CA1707
        {
            // Arrange
            const string sorCode = "sorCode";
            const string locationId = "locationId";
            var fromDate = new DateTime(2021, 12, 15);

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = sorCode,
                LocationId = locationId,
                FromDate = fromDate
            };

            // Act
            var result = await _systemUndertest.AvailableAppointments(request);

            // Assert
            GetStatusCode(result).Should().Be(200);
            _availableAppointmentsUseCaseMock.Verify(x => x.Execute(request), Times.Once);
        }

        [Fact]
        public async Task ReturnsErrorWhenFailsToBookAppointments()
        {
            // Arrange
            const string bookingReference = "bookingReference";
            var startDateTime = It.IsAny<DateTime>();
            var endDateTime = It.IsAny<DateTime>();

            const string errorMessage = "An error message";

            _bookAppointmentUseCaseMock.Setup(x => x.Execute(It.IsAny<BookAppointmentRequest>())).Throws(new Exception(errorMessage));

            var request = new BookAppointmentRequest
            {
                BookingReference = bookingReference,
                SorCode = SorCode,
                LocationId = LocationId,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime
            };

            // Act
            var result = await _systemUndertest.BookAppointment(request);

            // Assert
            GetStatusCode(result).Should().Be(500);
        }
    }
}
