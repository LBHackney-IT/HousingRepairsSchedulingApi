namespace HousingRepairsSchedulingApi.Tests.ControllersTests
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using HousingRepairsSchedulingApi.Controllers;
    using HousingRepairsSchedulingApi.UseCases;
    using Moq;
    using Xunit;

    public class AppointmentsControllerTests : ControllerTests
    {
        private const string SorCode = "SOR Code";
        private const string LocationId = "locationId";
        private readonly AppointmentsController systemUndertest;
        private readonly Mock<IRetrieveAvailableAppointmentsUseCase> availableAppointmentsUseCaseMock;
        private readonly Mock<IBookAppointmentUseCase> bookAppointmentUseCaseMock;

        public AppointmentsControllerTests()
        {
            this.availableAppointmentsUseCaseMock = new Mock<IRetrieveAvailableAppointmentsUseCase>();
            this.bookAppointmentUseCaseMock = new Mock<IBookAppointmentUseCase>();
            this.systemUndertest = new AppointmentsController(
                this.availableAppointmentsUseCaseMock.Object,
                this.bookAppointmentUseCaseMock.Object);
        }

        [Fact]
        public async Task TestAvailableAppointmentsEndpoint()
        {
            var result = await this.systemUndertest.AvailableAppointments(SorCode, LocationId);
            GetStatusCode(result).Should().Be(200);
            this.availableAppointmentsUseCaseMock.Verify(x => x.Execute(SorCode, LocationId, null), Times.Once);
        }


        [Fact]
        public async Task ReturnsErrorWhenFailsToGetAvailableAppointments()
        {

            const string errorMessage = "An error message";
            this.availableAppointmentsUseCaseMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), null)).Throws(new Exception(errorMessage));

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
            this.availableAppointmentsUseCaseMock.Verify(x => x.Execute(sorCode, locationId, fromDate), Times.Once);
        }

        [Fact]
        public async Task ReturnsErrorWhenFailsToBookAppointments()
        {
            const string bookingReference = "bookingReference";
            var startDateTime = It.IsAny<DateTime>();
            var endDateTime = It.IsAny<DateTime>();

            const string errorMessage = "An error message";
            this.bookAppointmentUseCaseMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Throws(new Exception(errorMessage));

            var result = await this.systemUndertest.BookAppointment(bookingReference, SorCode, LocationId, startDateTime, endDateTime);

            GetStatusCode(result).Should().Be(500);
            GetResultData<string>(result).Should().Be(errorMessage);
        }
    }
}
