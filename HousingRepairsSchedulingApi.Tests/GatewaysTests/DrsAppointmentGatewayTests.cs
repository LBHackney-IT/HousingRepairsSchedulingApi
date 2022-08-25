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
    using HousingRepairsSchedulingApi.Boundary.Requests;
    using Moq;
    using Services.Drs;
    using Xunit;

    public class DrsAppointmentGatewayTests
    {
        private Mock<IDrsService> _drsServiceMock = new();
        private DrsAppointmentGateway _systemUnderTest;

        private const int RequiredNumberOfAppointmentDays = 5;
        private const int AppointmentSearchTimeSpanInDays = 14;
        private const int AppointmentLeadTimeInDays = 0;
        private const int MaximumNumberOfRequests = 10;
        private const string BookingReference = "Booking Reference";
        private const string SorCode = "SOR Code";
        private const string LocationId = "locationId";

        public DrsAppointmentGatewayTests()
        {
            _systemUnderTest = new DrsAppointmentGateway(
                _drsServiceMock.Object,
                RequiredNumberOfAppointmentDays,
                AppointmentSearchTimeSpanInDays,
                AppointmentLeadTimeInDays, MaximumNumberOfRequests);
        }

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
        public void GivenInvalidRequiredNumberOfAppointmentsParameter_WhenInstantiating_ThenArgumentExceptionIsThrown(int invalidRequiredNumberOfAppointments)
        {
            // Arrange

            // Act
            Func<DrsAppointmentGateway> act = () => new DrsAppointmentGateway(_drsServiceMock.Object, invalidRequiredNumberOfAppointments, default, default, default);

            // Assert
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Fact]
        public void GivenInvalidAppointmentLeadTimeInDaysParameter_WhenInstantiating_ThenArgumentExceptionIsThrown()
        {
            // Arrange

            // Act
            Func<DrsAppointmentGateway> act = () => new DrsAppointmentGateway(_drsServiceMock.Object, 1, 1, -1, default);

            // Assert
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GivenInvalidAppointmentSearchTimeSpanInDaysParameter_WhenInstantiating_ThenArgumentExceptionIsThrown(int invalidAppointmentSearchTimeSpanInDays)
        {
            // Arrange

            // Act
            Func<DrsAppointmentGateway> act = () => new DrsAppointmentGateway(_drsServiceMock.Object, 1, invalidAppointmentSearchTimeSpanInDays, default, default);

            // Assert
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GivenInvalidMaximumNumberOfRequestsParameter_WhenInstantiating_ThenArgumentExceptionIsThrown(int invalidMaximumNumberOfRequests)
        {
            // Arrange

            // Act
            Func<DrsAppointmentGateway> act = () => new DrsAppointmentGateway(_drsServiceMock.Object, 1, 1, default, invalidMaximumNumberOfRequests);

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
        public async void GivenInvalidSorCode_WhenGettingAvailableAppointments_ThenExceptionIsThrown<T>(T exception, string sorCode) where T : Exception
        {
            // Arrange
            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = sorCode,
                LocationId = It.IsAny<string>()
            };

            // Act
            Func<Task> act = async () => await _systemUnderTest.GetAvailableAppointments(request);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenInvalidLocationId_WhenGettingAvailableAppointments_ThenExceptionIsThrown<T>(T exception, string locationId) where T : Exception
        {
            // Arrange
            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = locationId
            };

            // Act
            Func<Task> act = async () => await _systemUnderTest.GetAvailableAppointments(request);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Fact]
        public async void GivenNullFromDate_WhenGettingAvailableAppointments_ThenNoExceptionIsThrown()
        {
            // Arrange
            _drsServiceMock
                .Setup(x => x.CheckAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(CreateAppointmentsForSequentialDays(new DateTime(2022, 1, 17), 5));

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId"
            };

            // Act
            Func<Task> act = async () => await _systemUnderTest.GetAvailableAppointments(request);

            // Assert
            await act.Should().NotThrowAsync<NullReferenceException>();
        }

        [Theory]
        [MemberData(nameof(FiveDaysOfAvailableAppointmentsSingleAppointmentPerDayTestData))]
        [MemberData(nameof(FiveDaysOfAvailableAppointmentsMultipleAppointmentsPerDayTestData))]
        [MemberData(nameof(MoreThanFiveDaysOfAvailableAppointmentsTestData))]
        public async void GivenDrsServiceHasFiveOrMoreDaysOfAvailableAppointments_WhenGettingAvailableAppointments_ThenFiveDaysOfAppointmentsAreReturned(IEnumerable<IEnumerable<AppointmentSlot>> appointmentReturnSequence)
        {
            // Arrange
            var setupSequentialResult = _drsServiceMock
                .SetupSequence(x => x.CheckAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()));

            foreach (var appointments in appointmentReturnSequence)
            {
                setupSequentialResult = setupSequentialResult.ReturnsAsync(appointments);
            }

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId"
            };

            // Act
            var actualAppointments = await _systemUnderTest.GetAvailableAppointments(request);

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

            AppointmentSlot CreateAppointmentForDay(DateTime date)
            {
                return CreateAppointmentsForDay(date, true).Single();
            }
        }

        [Fact]
        public async void GivenDrsServiceHasAvailableAppointmentsThatAreNotRequired_WhenGettingAvailableAppointments_ThenOnlyValidAppointmentsAreReturned()
        {
            // Arrange
            _systemUnderTest = new DrsAppointmentGateway(_drsServiceMock.Object, 1, AppointmentSearchTimeSpanInDays, AppointmentLeadTimeInDays, int.MaxValue);

            _drsServiceMock
                .SetupSequence(x => x.CheckAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
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

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId"
            };

            // Act
            var actualAppointments = await _systemUnderTest.GetAvailableAppointments(request);

            // Assert
            actualAppointments.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [MemberData(nameof(TimeZoneOffsetTestData))]
        public async void GivenDrsServiceHasAvailableAppointmentsThatAreNotRequiredDueToTimeZoneOffset_WhenGettingAvailableAppointments_ThenTheyAreFilteredOutOfAppointmentsThatAreReturned(AppointmentSlot unrequiredAppointmentSlot)
        {
            // Arrange
            _systemUnderTest = new DrsAppointmentGateway(_drsServiceMock.Object, 1, AppointmentSearchTimeSpanInDays, AppointmentLeadTimeInDays, 1);

            _drsServiceMock
                .SetupSequence(x => x.CheckAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new[] { unrequiredAppointmentSlot });

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId"
            };

            // Act
            var actualAppointments = await _systemUnderTest.GetAvailableAppointments(request);

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
        public async void GivenDrsServiceRequiresMultipleRequests_WhenGettingAvailableAppointments_ThenCorrectTimeSpanIncrementApplied()
        {
            // Arrange
            _drsServiceMock
                .Setup(x => x.CheckAvailability(It.IsAny<string>(), It.IsAny<string>(), new DateTime(2022, 1, 17)))
                .ReturnsAsync(CreateAppointmentsForSequentialDays(new DateTime(2022, 1, 17), RequiredNumberOfAppointmentDays - 2));

            _drsServiceMock
                .Setup(x => x.CheckAvailability(It.IsAny<string>(), It.IsAny<string>(), new DateTime(2022, 1, 31)))
                .ReturnsAsync(CreateAppointmentsForSequentialDays(new DateTime(2022, 1, 31), RequiredNumberOfAppointmentDays - (RequiredNumberOfAppointmentDays - 2)));

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId",
                FromDate = new DateTime(2022, 1, 17)
            };

            // Act
            _ = await _systemUnderTest.GetAvailableAppointments(request);

            // Assert
            _drsServiceMock.VerifyAll();
        }

        [Fact]
        public async void GivenNoAppointmentSlots_WhenGettingAvailableApointments_ThenExactlyMaximumNumberOfRequestsAreSent()
        {
            // Arrange
            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId",
                FromDate = new DateTime(2022, 1, 17)
            };

            // Act
            _ = await _systemUnderTest.GetAvailableAppointments(request);

            // Assert
            _drsServiceMock.Verify(x => x.CheckAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Exactly(10));
        }

        [Fact]
        public async void GivenAppointmentSlotsInFuture_WhenGettingAvailableApointments_ThenNoMoreThanMaximumNumberOfRequestsAreMade()
        {
            // Arrange
            
            Expression<Func<IDrsService, Task<IEnumerable<AppointmentSlot>>>> expression = x =>
                x.CheckAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>());

            _drsServiceMock.SetupSequence(expression)
                .ReturnsAsync(Enumerable.Empty<AppointmentSlot>())
                .ReturnsAsync(Enumerable.Empty<AppointmentSlot>())
                .ReturnsAsync(Enumerable.Empty<AppointmentSlot>())
                .ReturnsAsync(Enumerable.Empty<AppointmentSlot>())
                .ReturnsAsync(CreateAppointmentsForSequentialDays(new DateTime(2022, 1, 17), 5));

            var request = new GetAvailableAppointmentsRequest
            {
                SorCode = "sorCode",
                LocationId = "locationId",
                FromDate = new DateTime(2022, 1, 17)
            };

            // Act
            _ = await _systemUnderTest.GetAvailableAppointments(request);

            // Assert
            _drsServiceMock.Verify(x => x.CheckAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.AtMost(10));
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
                EndDateTime = It.IsAny<DateTime>(),
            };

            // Act
            Func<Task> act = async () => await _systemUnderTest.BookAppointment(request);

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
                EndDateTime = It.IsAny<DateTime>(),
            };

            // Act
            Func<Task> act = async () => await _systemUnderTest.BookAppointment(request);

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
                EndDateTime = It.IsAny<DateTime>(),
            };

            // Act
            Func<Task> act = async () => await _systemUnderTest.BookAppointment(request);

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
                EndDateTime = endDate,
            };

            // Act
            Func<Task> act = async () =>
                await _systemUnderTest.BookAppointment(request);

            // Assert
            await act.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async void GivenValidArguments_WhenExecute_ThenBookingReferenceIsReturned()
        {
            // Arrange
            const int bookingId = 12345;

            _drsServiceMock
                .Setup(x => x.CreateOrder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(bookingId);

            var startDateTime = new DateTime(2022, 05, 01);

            var request = new BookAppointmentRequest
            {
                BookingReference = BookingReference,
                SorCode = SorCode,
                LocationId = LocationId,
                StartDateTime = startDateTime,
                EndDateTime = startDateTime.AddDays(1),
            };

            // Act
            var actual = await _systemUnderTest.BookAppointment(request);

            // Assert
            Assert.Equal(BookingReference, actual);
        }
    }
}
