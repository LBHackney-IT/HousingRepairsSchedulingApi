using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using HousingRepairsSchedulingApi.Boundary.Requests;
using HousingRepairsSchedulingApi.Gateways.Interfaces;
using HousingRepairsSchedulingApi.UseCases;
using Moq;
using Xunit;
namespace HousingRepairsSchedulingApi.Tests.UseCasesTests
{

    public class RetrieveAvailableAppointmentsTests
    {
        private readonly RetrieveAvailableAppointmentsUseCase _sytemUndertest;
        private readonly Mock<IAppointmentsGateway> _appointmentsGatewayMock;

        public RetrieveAvailableAppointmentsTests()
        {
            _appointmentsGatewayMock = new Mock<IAppointmentsGateway>();
            _sytemUndertest = new RetrieveAvailableAppointmentsUseCase(_appointmentsGatewayMock.Object);
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenAnInvalidSorCode_WhenExecute_ThenExceptionIsThrown<T>(string sorCode) where T : Exception
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(_appointmentsGatewayMock.Object);

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = sorCode,
                LocationId = "locationId"
            };

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(request);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenAnInvalidLocationId_WhenExecute_ThenExceptionIsThrown<T>(string locationId) where T : Exception
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(_appointmentsGatewayMock.Object);

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "uprn",
                LocationId = locationId
            };

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(request);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Fact]
        public async void GivenANullFromDate_WhenExecute_ThenNoExceptionIsThrown()
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(_appointmentsGatewayMock.Object);

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "SoR Code",
                LocationId = "location Id"
            };

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(request);

            // Assert
            await act.Should().NotThrowAsync();
        }

        public static IEnumerable<object[]> InvalidArgumentTestData()
        {
            yield return new object[] { new ArgumentNullException(), null };
            yield return new object[] { new ArgumentException(), "" };
            yield return new object[] { new ArgumentException(), " " };
        }

        [Fact]
        public async void GivenParameters_WhenExecute_ThenGetAvailableAppointmentsGatewayIsCalled()
        {
            // Arrange
            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "uprn",
                LocationId = "locationId"
            };

            // Act
            await _sytemUndertest.Execute(request);

            // Assert
            _appointmentsGatewayMock.Verify(x => x.GetAvailableAppointments(request), Times.Once);
        }
    }
}
