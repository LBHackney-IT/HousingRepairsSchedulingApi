namespace HousingRepairsSchedulingApi.Tests.GatewaysTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Domain;
    using FluentAssertions;
    using Gateways;
    using Moq;
    using Services.Drs;
    using Xunit;

    public class DrsAppointmentGatewayTests
    {
        private readonly Mock<IDrsService> drsServiceMock = new();
        private DrsAppointmentGateway systemUnderTest;
        private const int RequiredNumberOfAppointmentDays = 5;
        private const int AppointmentSearchTimeSpanInDays = 14;
        private const int AppointmentLeadTimeInDays = 0;
        private const int MaximumNumberOfRequests = 10;
        private const string BookingReference = "Booking Reference";
        private const string SorCode = "SOR Code";
        private const string LocationId = "locationId";

        public DrsAppointmentGatewayTests() => this.systemUnderTest = new DrsAppointmentGateway(
                this.drsServiceMock.Object,
                RequiredNumberOfAppointmentDays,
                AppointmentSearchTimeSpanInDays,
                AppointmentLeadTimeInDays, MaximumNumberOfRequests);

        [Fact]
#pragma warning disable CA1707
        public void GivenNullDrsServiceParameter_WhenInstantiating_ThenArgumentNullExceptionIsThrown()
#pragma warning restore CA1707
        {
            // Arrange

            // Act
            Func<DrsAppointmentGateway> act = () => new DrsAppointmentGateway(
                null,
                default,
                default,
                default,
                default);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
#pragma warning disable CA1707
        public void GivenInvalidRequiredNumberOfAppointmentsParameter_WhenInstantiating_ThenArgumentExceptionIsThrown(int invalidRequiredNumberOfAppointments)
#pragma warning restore CA1707
        {
            // Arrange

            // Act
            Func<DrsAppointmentGateway> act = () => new DrsAppointmentGateway(
                this.drsServiceMock.Object,
                invalidRequiredNumberOfAppointments,
                default,
                default,
                default);

            // Assert
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
#pragma warning disable CA1707
        public void GivenInvalidAppointmentLeadTimeInDaysParameter_WhenInstantiating_ThenArgumentExceptionIsThrown()
#pragma warning restore CA1707
        {
            // Arrange

            // Act
            Func<DrsAppointmentGateway> act = () => new DrsAppointmentGateway(
                this.drsServiceMock.Object,
                1,
                1,
                -1,
                default);

            // Assert
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
#pragma warning disable CA1707
        public void GivenInvalidAppointmentSearchTimeSpanInDaysParameter_WhenInstantiating_ThenArgumentExceptionIsThrown(int invalidAppointmentSearchTimeSpanInDays)
#pragma warning restore CA1707
        {
            // Arrange

            // Act
            Func<DrsAppointmentGateway> act = () => new DrsAppointmentGateway(
                this.drsServiceMock.Object,
                1,
                invalidAppointmentSearchTimeSpanInDays,
                default,
                default);

            // Assert
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
#pragma warning disable CA1707
        public void GivenInvalidMaximumNumberOfRequestsParameter_WhenInstantiating_ThenArgumentExceptionIsThrown(int invalidMaximumNumberOfRequests)
#pragma warning restore CA1707
        {
            // Arrange

            // Act
            Func<DrsAppointmentGateway> act = () => new DrsAppointmentGateway(
                this.drsServiceMock.Object,
                1,
                1,
                default,
                invalidMaximumNumberOfRequests);

            // Assert
            act.Should().ThrowExactly<ArgumentException>().WithParameterName("maximumNumberOfRequests");
        }

        public static IEnumerable<object[]> InvalidArgumentTestData()
        {
            yield return new object[] { new ArgumentNullException(), null };
            yield return new object[] { new ArgumentException(), "" };
            yield return new object[] { new ArgumentException(), " " };
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenInvalidSorCode_WhenGettingAvailableAppointments_ThenExceptionIsThrown<T>(T exception, string sorCode) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await this.systemUnderTest.GetAvailableAppointments(sorCode, It.IsAny<string>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenInvalidLocationId_WhenGettingAvailableAppointments_ThenExceptionIsThrown<T>(T exception, string locationId) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange
            var sorCode = "sorCode";

            // Act
            Func<Task> act = async () => await this.systemUnderTest.GetAvailableAppointments(sorCode, locationId);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenNullFromDate_WhenGettingAvailableAppointments_ThenNoExceptionIsThrown()
#pragma warning restore CA1707
        {
            // Arrange
            var sorCode = "sorCode";
            var locationId = "locationId";
            this.drsServiceMock.Setup(x => x.CheckAvailability(It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>())).ReturnsAsync(CreateAppointmentsForSequentialDays(new DateTime(2022, 1, 17), 5));

            // Act
            Func<Task> act = async () => await this.systemUnderTest.GetAvailableAppointments(sorCode, locationId, null);

            // Assert
            await act.Should().NotThrowAsync<NullReferenceException>();
        }

        [Theory]
        [MemberData(nameof(FiveDaysOfAvailableAppointmentsSingleAppointmentPerDayTestData))]
        [MemberData(nameof(FiveDaysOfAvailableAppointmentsMultipleAppointmentsPerDayTestData))]
        [MemberData(nameof(MoreThanFiveDaysOfAvailableAppointmentsTestData))]
#pragma warning disable CA1707
        public async void GivenDrsServiceHasFiveOrMoreDaysOfAvailableAppointments_WhenGettingAvailableAppointments_ThenFiveDaysOfAppointmentsAreReturned(IEnumerable<IEnumerable<AppointmentSlot>> appointmentReturnSequence)
#pragma warning restore CA1707
        {
            // Arrange
            var sorCode = "sorCode";
            var locationId = "locationId";

            var setupSequentialResult = this.drsServiceMock.SetupSequence(x => x.CheckAvailability(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>()));

            foreach (var appointments in appointmentReturnSequence)
            {
                setupSequentialResult = setupSequentialResult.ReturnsAsync(appointments);
            }

            // Act
            var actualAppointments = await this.systemUnderTest.GetAvailableAppointments(sorCode, locationId);

            // Assert
            Assert.Equal(RequiredNumberOfAppointmentDays, actualAppointments.Select(x => x.StartTime.Date).Distinct().Count());
        }

        public static IEnumerable<object[]> FiveDaysOfAvailableAppointmentsSingleAppointmentPerDayTestData()
        {
            yield return new object[] { new[]
            {
                CreateAppointmentsForDay(new DateTime(2022, 1, 18), true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 19), true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 20), true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 21), true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 22), true),
            }};
            yield return new object[] { new[]
            {
                CreateAppointmentsForDay(new DateTime(2022, 1, 18), include0800To1200: true)
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 19), include0800To1200: true)),
                CreateAppointmentsForDay(new DateTime(2022, 1, 20), include0800To1200: true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 21), include0800To1200: true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 22), include0800To1200: true),
            }};
            yield return new object[] { new[]
            {
                CreateAppointmentsForDay(new DateTime(2022, 1, 18), include0800To1200: true)
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 19), include0800To1200: true)),
                CreateAppointmentsForDay(new DateTime(2022, 1, 20), include0800To1200: true)
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 21), include0800To1200: true)),
                CreateAppointmentsForDay(new DateTime(2022, 1, 22), include0800To1200: true),
            }};
            yield return new object[] { new[]
            {
                CreateAppointmentsForDay(new DateTime(2022, 1, 18), true)
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 19), true)),
                Array.Empty<AppointmentSlot>(),
                CreateAppointmentsForDay(new DateTime(2022, 1, 20), true)
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 21), true)),
                CreateAppointmentsForDay(new DateTime(2022, 1, 22), true),
            }};
        }

        public static IEnumerable<object[]> FiveDaysOfAvailableAppointmentsMultipleAppointmentsPerDayTestData()
        {
            yield return new object[] { new[]
            {
                CreateAppointmentsForDay(new DateTime(2022, 1, 18), true, true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 19), true, true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 20), true, true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 21), true, true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 22), true, true),
            }};
            yield return new object[] { new[]
            {
                CreateAppointmentsForDay(new DateTime(2022, 1, 18), true, true)
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 19), true, true)),
                CreateAppointmentsForDay(new DateTime(2022, 1, 20), true, true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 21), true, true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 22), true, true),
            }};
        }

        public static IEnumerable<object[]> MoreThanFiveDaysOfAvailableAppointmentsTestData()
        {
            yield return new object[] { new[]
            {
                CreateAppointmentsForDay(new DateTime(2022, 1, 18), true, true)
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 19), true, true)),
                CreateAppointmentsForDay(new DateTime(2022, 1, 20), true, true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 21), true, true),
                CreateAppointmentsForDay(new DateTime(2022, 1, 22), true, true)
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 23), true, true)),
            }};
            yield return new object[] { new[]
            {
                CreateAppointmentsForDay(new DateTime(2022, 1, 18), true, true)
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 19), true, true))
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 20), true, true))
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 21), true, true))
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 22), true, true))
                    .Concat(CreateAppointmentsForDay(new DateTime(2022, 1, 23), true, true)),
            }};
        }

        private static IEnumerable<AppointmentSlot> CreateAppointmentsForDay(DateTime dateTime,
            bool include0800To1200 = false,
            bool include1200To1600 = false,
            bool include0930To1430 = false,
            bool include0800To1600 = false)
        {
            var result = new List<AppointmentSlot>();

            if (include0800To1200)
            {
                result.Add(
                    new()
                    {
                        StartTime = dateTime.AddHours(8),
                        EndTime = dateTime.AddHours(12),
                    }
                );
            }

            if (include0930To1430)
            {
                result.Add(
                    new()
                    {
                        StartTime = dateTime.AddHours(9).AddMinutes(30),
                        EndTime = dateTime.AddHours(14).AddMinutes(30),
                    }
                );
            }

            if (include1200To1600)
            {
                result.Add(
                    new()
                    {
                        StartTime = dateTime.AddHours(12),
                        EndTime = dateTime.AddHours(16),
                    }
                );
            }

            if (include0800To1600)
            {
                result.Add(
                    new()
                    {
                        StartTime = dateTime.AddHours(8),
                        EndTime = dateTime.AddHours(16),
                    }
                );
            }

            return result;
        }

        private static AppointmentSlot[] CreateAppointmentsForSequentialDays(DateTime firstDate, int numberOfDays)
        {
            var appointments = Enumerable.Range(0, numberOfDays).Select(x => CreateAppointmentForDay(firstDate.AddDays(x))).ToArray();

            return appointments;

            static AppointmentSlot CreateAppointmentForDay(DateTime date) => CreateAppointmentsForDay(date, true).Single();
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenDrsServiceHasAvailableAppointmentsThatAreNotRequired_WhenGettingAvailableAppointments_ThenOnlyValidAppointmentsAreReturned()
#pragma warning restore CA1707
        {
            // Arrange
            var sorCode = "sorCode";
            var locationId = "locationId";

            this.systemUnderTest = new DrsAppointmentGateway(
                this.drsServiceMock.Object,
                1,
                AppointmentSearchTimeSpanInDays,
                AppointmentLeadTimeInDays, int.MaxValue);

            this.drsServiceMock.SetupSequence(x => x.CheckAvailability(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>()))
                .ReturnsAsync(CreateAppointmentsForDay(new DateTime(2022, 1, 17), true, true, true, true));

            var expected = new[]
                {
                    new AppointmentSlot
                    {
                        StartTime = new DateTime(2022, 1, 17, 8, 0, 0),
                        EndTime = new DateTime(2022, 1, 17, 12, 0, 0),
                    },
                    new AppointmentSlot
                    {
                        StartTime = new DateTime(2022, 1, 17, 12, 0, 0),
                        EndTime = new DateTime(2022, 1, 17, 16, 0, 0),
                    },
                };

            // Act
            var actualAppointments = await this.systemUnderTest.GetAvailableAppointments(sorCode, locationId);

            // Assert
            actualAppointments.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [MemberData(nameof(TimeZoneOffsetTestData))]
#pragma warning disable CA1707
        public async void GivenDrsServiceHasAvailableAppointmentsThatAreNotRequiredDueToTimeZoneOffset_WhenGettingAvailableAppointments_ThenTheyAreFilteredOutOfAppointmentsThatAreReturned(AppointmentSlot unrequiredAppointmentSlot)
#pragma warning restore CA1707
        {
            // Arrange
            var sorCode = "sorCode";
            var locationId = "locationId";

            this.systemUnderTest = new DrsAppointmentGateway(
                this.drsServiceMock.Object,
                1,
                AppointmentSearchTimeSpanInDays,
                AppointmentLeadTimeInDays, 1);

            this.drsServiceMock.SetupSequence(x => x.CheckAvailability(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>()))
                .ReturnsAsync(new[] { unrequiredAppointmentSlot });

            // Act
            var actualAppointments = await this.systemUnderTest.GetAvailableAppointments(sorCode, locationId);

            // Assert
            actualAppointments.Should().BeEmpty();
        }

        public static IEnumerable<object[]> TimeZoneOffsetTestData()
        {
            yield return new object[]
            {
                new AppointmentSlot
                {
                    StartTime = new DateTime(2022, 3, 30, 7, 0, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2022, 3, 30, 15, 0, 0, DateTimeKind.Utc)
                }
            };

            yield return new object[]
            {
                new AppointmentSlot
                {
                    StartTime = new DateTime(2022, 3, 30, 8, 30, 0, DateTimeKind.Utc),
                    EndTime = new DateTime(2022, 3, 30, 13, 30, 0, DateTimeKind.Utc)
                }
            };
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenDrsServiceRequiresMultipleRequests_WhenGettingAvailableAppointments_ThenCorrectTimeSpanIncrementApplied()
#pragma warning restore CA1707
        {
            // Arrange
            var sorCode = "sorCode";
            var locationId = "locationId";

            this.drsServiceMock.Setup(x => x.CheckAvailability(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    new DateTime(2022, 1, 17)))
                .ReturnsAsync(CreateAppointmentsForSequentialDays(new DateTime(2022, 1, 17), RequiredNumberOfAppointmentDays - 2));
            this.drsServiceMock.Setup(x => x.CheckAvailability(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        new DateTime(2022, 1, 31)))
                .ReturnsAsync(CreateAppointmentsForSequentialDays(new DateTime(2022, 1, 31),
                    RequiredNumberOfAppointmentDays - (RequiredNumberOfAppointmentDays - 2)));

            // Act
            _ = await this.systemUnderTest.GetAvailableAppointments(sorCode, locationId, new DateTime(2022, 1, 17));

            // Assert
            this.drsServiceMock.VerifyAll();
        }

        [Fact]
        public async void GivenNoAppointmentSlotsWhenGettingAvailableApointmentsThenExactlyMaximumNumberOfRequestsAreSent()
        {
            // Arrange
            var sorCode = "sorCode";
            var locationId = "locationId";

            // Act
            _ = await this.systemUnderTest.GetAvailableAppointments(sorCode, locationId, new DateTime(2022, 1, 17));

            // Assert
            this.drsServiceMock.Verify(x => x.CheckAvailability(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>()), Times.Exactly(10));
        }

        [Fact]
        public async void GivenAppointmentSlotsInFutureWhenGettingAvailableApointmentsThenNoMoreThanMaximumNumberOfRequestsAreMade()
        {
            // Arrange
            var sorCode = "sorCode";
            var locationId = "locationId";

            Expression<Func<IDrsService, Task<IEnumerable<AppointmentSlot>>>> expression = x => x.CheckAvailability(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>());
            this.drsServiceMock.SetupSequence(expression)
                .ReturnsAsync(Enumerable.Empty<AppointmentSlot>())
                .ReturnsAsync(Enumerable.Empty<AppointmentSlot>())
                .ReturnsAsync(Enumerable.Empty<AppointmentSlot>())
                .ReturnsAsync(Enumerable.Empty<AppointmentSlot>())
                .ReturnsAsync(CreateAppointmentsForSequentialDays(new DateTime(2022, 1, 17), 5));

            // Act
            _ = await this.systemUnderTest.GetAvailableAppointments(sorCode, locationId, new DateTime(2022, 1, 17));

            // Assert
            this.drsServiceMock.Verify(x => x.CheckAvailability(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>()), Times.AtMost(10));
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

            // Act
            Func<Task> act = async () => await this.systemUnderTest.BookAppointment(bookingReference, SorCode, LocationId,
                It.IsAny<DateTime>(), It.IsAny<DateTime>());

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

            // Act
            Func<Task> act = async () => await this.systemUnderTest.BookAppointment(BookingReference, sorCode, LocationId,
                It.IsAny<DateTime>(), It.IsAny<DateTime>());

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

            // Act
            Func<Task> act = async () => await this.systemUnderTest.BookAppointment(BookingReference, SorCode, locationId,
                It.IsAny<DateTime>(), It.IsAny<DateTime>());

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
            var endDate = startDate.AddDays(-1);

            // Act
            Func<Task> act = async () =>
                await this.systemUnderTest.BookAppointment(BookingReference, SorCode, LocationId, startDate, endDate);

            // Assert
            await act.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenValidArguments_WhenExecute_ThenBookingReferenceIsReturned()
#pragma warning restore CA1707
        {
            // Arrange
            var booking = new booking
            {
                bookingId = 12345
            };

            this.drsServiceMock.Setup(x =>
                x.CreateOrder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())
            ).ReturnsAsync(booking);

            // Act
            var startDateTime = It.IsAny<DateTime>();
            var actual = await this.systemUnderTest.BookAppointment(BookingReference, SorCode, LocationId,
                startDateTime, startDateTime.AddDays(1));

            // Assert
            Assert.Equal(BookingReference, actual.BookingReference);
        }
    }
}
