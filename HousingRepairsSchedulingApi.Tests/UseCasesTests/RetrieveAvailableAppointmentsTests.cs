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
        private readonly RetrieveAvailableAppointmentsUseCase sytemUndertest;
        private readonly Mock<IAppointmentsGateway> appointmentsGatewayMock;

        public RetrieveAvailableAppointmentsTests()
        {
            appointmentsGatewayMock = new Mock<IAppointmentsGateway>();
            sytemUndertest = new RetrieveAvailableAppointmentsUseCase(appointmentsGatewayMock.Object, new NullLogger<RetrieveAvailableAppointmentsUseCase>());
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
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(appointmentsGatewayMock.Object, new NullLogger<RetrieveAvailableAppointmentsUseCase>());

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
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenAnInvalidLocationId_WhenExecute_ThenExceptionIsThrown<T>(T exception, string locationId) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(appointmentsGatewayMock.Object, new NullLogger<RetrieveAvailableAppointmentsUseCase>());

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
#pragma warning disable CA1707
        public async void GivenANullFromDate_WhenExecute_ThenNoExceptionIsThrown()
#pragma warning restore CA1707
        {
            // Arrange
            var systemUnderTest = new RetrieveAvailableAppointmentsUseCase(appointmentsGatewayMock.Object, new NullLogger<RetrieveAvailableAppointmentsUseCase>());

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
#pragma warning disable CA1707
        public async void GivenParameters_WhenExecute_ThenGetAvailableAppointmentsGatewayIsCalled()
#pragma warning restore CA1707
        {
            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId", 
            };

            await sytemUndertest.Execute(request);
            appointmentsGatewayMock.Verify(x => x.GetAvailableAppointments(request), Times.Once);
        }
    }
}
