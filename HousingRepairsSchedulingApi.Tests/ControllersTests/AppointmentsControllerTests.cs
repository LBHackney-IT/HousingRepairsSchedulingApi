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
    using UseCases;

    public class AppointmentsControllerTests : ControllerTests
    {
        private const string SorCode = "SOR Code";
        private const string LocationId = "locationId";
        private AppointmentsController systemUndertest;
        private Mock<IRetrieveAvailableAppointmentsUseCase> availableAppointmentsUseCaseMock;
        private Mock<IBookAppointmentUseCase> bookAppointmentUseCaseMock;

        public AppointmentsControllerTests()
        {
            availableAppointmentsUseCaseMock = new Mock<IRetrieveAvailableAppointmentsUseCase>();
            bookAppointmentUseCaseMock = new Mock<IBookAppointmentUseCase>();
            this.systemUndertest = new AppointmentsController(
                availableAppointmentsUseCaseMock.Object,
                bookAppointmentUseCaseMock.Object,
                new NullLogger<AppointmentsController>());
        }

        [Fact]
        public async Task TestAvailableAppointmentsEndpoint()
        {
            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = SorCode,
                LocationId = LocationId
            };

            var result = await this.systemUndertest.AvailableAppointments(request);
            GetStatusCode(result).Should().Be(200);
            availableAppointmentsUseCaseMock.Verify(x => x.Execute(request), Times.Once);
        }


        [Fact]
        public async Task ReturnsErrorWhenFailsToGetAvailableAppointments()
        {
            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = It.IsAny<string>(),
                LocationId = It.IsAny<string>(),
            };

            const string errorMessage = "An error message";
            this.availableAppointmentsUseCaseMock.Setup(x => x.Execute(request)).Throws(new Exception(errorMessage));

            var result = await this.systemUndertest.AvailableAppointments(request);

            GetStatusCode(result).Should().Be(500);
        }

        [Fact]
        public async Task TestBookAppointmentEndpoint()
        {
            const string bookingReference = "bookingReference";
            var startDateTime = It.IsAny<DateTime>();
            var endDateTime = It.IsAny<DateTime>();

            var result = await this.systemUndertest.BookAppointment(bookingReference, SorCode, LocationId, startDateTime, endDateTime);
            GetStatusCode(result).Should().Be(200);
        }

        [Fact]
#pragma warning disable CA1707
        public async Task GivenAFromDate_WhenRequestingAvailableAppointment_ThenResultsAreReturned()
#pragma warning restore CA1707
        {
            // Arrange
            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId",
                FromDate = new DateTime(2021, 12, 15)
            };

            // Act
            var result = await this.systemUndertest.AvailableAppointments(request);

            // Assert
            GetStatusCode(result).Should().Be(200);
            availableAppointmentsUseCaseMock.Verify(x => x.Execute(request), Times.Once);
        }

        [Fact]
        public async Task ReturnsErrorWhenFailsToBookAppointments()
        {
            const string bookingReference = "bookingReference";
            var startDateTime = It.IsAny<DateTime>();
            var endDateTime = It.IsAny<DateTime>();

            const string errorMessage = "An error message";
            this.bookAppointmentUseCaseMock.Setup(x => x.Execute(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Throws(new Exception(errorMessage));

            var result = await this.systemUndertest.BookAppointment(bookingReference, SorCode, LocationId, startDateTime, endDateTime);

            GetStatusCode(result).Should().Be(500);
        }
    }
}
