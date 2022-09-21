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
    using Microsoft.Extensions.Logging.Abstractions;

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
                _bookAppointmentUseCaseMock.Object,
                new NullLogger<AppointmentsController>());
        }

        [Fact]
        public async Task TestAvailableAppointmentsEndpoint()
        {
            // Arrange
            var request = new GetAvailableAppointmentsRequest
            {
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
            var request = new BookAppointmentRequest
            {
                BookingReference = "bookingReference",
                SorCode = SorCode,
                LocationId = LocationId,
                StartDateTime = It.IsAny<DateTime>(),
                EndDateTime = It.IsAny<DateTime>()
            };

            // Act
            var result = await this._systemUndertest.BookAppointment(request);

            // Assert
            GetStatusCode(result).Should().Be(200);
        }

        [Fact]
        public async Task GivenAFromDate_WhenRequestingAvailableAppointment_ThenResultsAreReturned()
        {
            // Arrange
            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId",
                FromDate = new DateTime(2021, 12, 15)
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
            const string errorMessage = "An error message";

            _bookAppointmentUseCaseMock
                .Setup(x => x.Execute(It.IsAny<BookAppointmentRequest>()))
                .Throws(new Exception(errorMessage));

            var request = new BookAppointmentRequest
            {
                BookingReference = "bookingReference",
                SorCode = SorCode,
                LocationId = LocationId,
                StartDateTime = It.IsAny<DateTime>(),
                EndDateTime = It.IsAny<DateTime>()
            };

            // Act
            var result = await _systemUndertest.BookAppointment(request);

            // Assert
            GetStatusCode(result).Should().Be(500);
        }
    }
}
