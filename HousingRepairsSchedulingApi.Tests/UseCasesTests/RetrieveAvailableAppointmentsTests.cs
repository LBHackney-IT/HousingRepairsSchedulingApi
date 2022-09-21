namespace HousingRepairsSchedulingApi.Tests.UseCasesTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using HousingRepairsSchedulingApi.Boundary.Requests;
    using HousingRepairsSchedulingApi.Gateways.Interfaces;
    using HousingRepairsSchedulingApi.UseCases;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Xunit;

    public class RetrieveAvailableAppointmentsTests
    {
        private readonly RetrieveAvailableAppointmentsUseCase _sytemUndertest;
        private readonly Mock<IAppointmentsGateway> _appointmentsGatewayMock;

        public RetrieveAvailableAppointmentsTests()
        {
            _appointmentsGatewayMock = new Mock<IAppointmentsGateway>();
            _sytemUndertest = new RetrieveAvailableAppointmentsUseCase(_appointmentsGatewayMock.Object, new NullLogger<RetrieveAvailableAppointmentsUseCase>());
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenAnInvalidSorCode_WhenExecute_ThenExceptionIsThrown<T>(T exception, string sorCode) where T : Exception
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(_appointmentsGatewayMock.Object, new NullLogger<RetrieveAvailableAppointmentsUseCase>());

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = sorCode,
                LocationId = "locationId",
            };

            // Act
            Func<Task> act = async () => await systemUnderTest.Execute(request);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenAnInvalidLocationId_WhenExecute_ThenExceptionIsThrown<T>(T exception, string locationId) where T : Exception
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(_appointmentsGatewayMock.Object, new NullLogger<RetrieveAvailableAppointmentsUseCase>());

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = locationId,
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
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(_appointmentsGatewayMock.Object, new NullLogger<RetrieveAvailableAppointmentsUseCase>());

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId",
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
            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId",
            };

            await _sytemUndertest.Execute(request);

            _appointmentsGatewayMock.Verify(x => x.GetAvailableAppointments(request), Times.Once);
        }
    }
}
