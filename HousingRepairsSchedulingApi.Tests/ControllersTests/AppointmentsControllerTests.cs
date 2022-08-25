using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace HousingRepairsSchedulingApi.Tests.ControllersTests
{
    using System;
    using Controllers;
    using HousingRepairsSchedulingApi.UseCases.Interfaces;
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
                bookAppointmentUseCaseMock.Object);
        }

        [Fact]
        public async Task TestAvailableAppointmentsEndpoint()
        {
            var result = await this.systemUndertest.AvailableAppointments(SorCode, LocationId);
            GetStatusCode(result).Should().Be(200);
            availableAppointmentsUseCaseMock.Verify(x => x.Execute(SorCode, LocationId, null), Times.Once);
        }


        [Fact]
        public async Task ReturnsErrorWhenFailsToGetAvailableAppointments()
        {

            const string errorMessage = "An error message";
            this.availableAppointmentsUseCaseMock.Setup(x => x.Execute(It.IsAny<String>(), It.IsAny<String>(), null)).Throws(new Exception(errorMessage));

            var result = await this.systemUndertest.AvailableAppointments(SorCode, LocationId);

            GetStatusCode(result).Should().Be(500);
            GetResultData<string>(result).Should().Be(errorMessage);
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
            const string sorCode = "sorCode";
            const string locationId = "locationId";
            var fromDate = new DateTime(2021, 12, 15);

            // Act
            var result = await this.systemUndertest.AvailableAppointments(sorCode, locationId, fromDate);

            // Assert
            GetStatusCode(result).Should().Be(200);
            availableAppointmentsUseCaseMock.Verify(x => x.Execute(sorCode, locationId, fromDate), Times.Once);
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
            GetResultData<string>(result).Should().Be(errorMessage);
        }
    }
}
