namespace HousingRepairsSchedulingApi.Tests.ServicesTests.Drs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Domain;
    using FluentAssertions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Moq;
    using Services.Drs;
    using Xunit;

    [SuppressMessage("Naming", "CA1707", MessageId = "Identifiers should not contain underscores")]
    public class DrsServiceTests
    {
        private const string SorCode = "SorCode";
        private const string LocationId = "LocationId";
        private const string BookingReference = "BookingReference";
        private const int BookingId = 12345;

        private readonly Mock<SOAP> _soapMock;

        private readonly DrsService _systemUnderTest;

        public DrsServiceTests()
        {
            var drsOptionsMock = new Mock<IOptions<DrsOptions>>();
            _soapMock = new Mock<SOAP>();


            var drsMockResponse = new DrsOptions { Login = "login", Password = "password" };

            drsOptionsMock
                .Setup(x => x.Value)
                .Returns(drsMockResponse);

            var soapMockResponse = new openSessionResponse
            {
                @return = new xmbOpenSessionResponse { sessionId = "sessionId" }
            };

            _soapMock
                .Setup(x => x.openSessionAsync(It.IsAny<openSession>()))
                .ReturnsAsync(soapMockResponse);

            _systemUnderTest = new DrsService(_soapMock.Object, drsOptionsMock.Object, new NullLogger<DrsService>());
        }

        [Fact]
        public void GivenNullDrsSoapClientParameter_WhenInstantiating_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange

            // Act
            Func<DrsService> act = () => new DrsService(null, It.IsAny<IOptions<DrsOptions>>(), new NullLogger<DrsService>());

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("drsSoapClient");
        }

        [Fact]
        public void GivenNullDrsOptionsParameter_WhenInstantiating_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange

            // Act
            Func<DrsService> act = () => new DrsService(new Mock<SOAP>().Object, null, new NullLogger<DrsService>());

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("drsOptions");
        }

        [Theory]
        [MemberData(nameof(UnavailableSlotsTestData))]
        public async void GivenDrsCheckAvailabilityResponseContainsUnavailableSlots_WhenCheckingAvailability_ThenOnlyAvailableSlotsAreReturned(DateTime searchDate, daySlotsInfo[] daySlots, IEnumerable<AppointmentSlot> expected)
        {
            // Arrange
            var soapResponse = new checkAvailabilityResponse(
                    new xmbCheckAvailabilityResponse { theSlots = daySlots }
            );

            _soapMock
                .Setup(x => x.checkAvailabilityAsync(It.IsAny<checkAvailability>()))
                .ReturnsAsync(soapResponse);

            // Act
            var appointmentSlots = await _systemUnderTest.CheckAvailability(SorCode, LocationId, searchDate);

            // Assert
            appointmentSlots.Should().BeEquivalentTo(expected);
        }

        public static IEnumerable<object[]> UnavailableSlotsTestData()
        {
            var date = new DateTime(2022, 1, 19);

            yield return new object[]
            {
                date,
                new[]
                {
                    new daySlotsInfo
                    {
                        day = date,
                        slotsForDay = new[]
                        {
                            new slotInfo
                            {
                                available = availableValue.NO,
                                beginDate = date.AddHours(8),
                                endDate = date.AddHours(12),
                            },
                        }
                    }
                },
                Enumerable.Empty<AppointmentSlot>()
            };

            yield return new object[]
            {
                date,
                new[]
                {
                    new daySlotsInfo
                    {
                        day = date,
                        slotsForDay = new[]
                        {
                            new slotInfo
                            {
                                available = availableValue.YES,
                                beginDate = date.AddHours(8),
                                endDate = date.AddHours(12),
                            },
                        }
                    }
                },
                new []{new AppointmentSlot{StartTime = date.AddHours(8), EndTime = date.AddHours(12)}}
            };

            yield return new object[]
            {
                date,
                new[]
                {
                    new daySlotsInfo
                    {
                        day = date,
                        slotsForDay = new[]
                        {
                            new slotInfo
                            {
                                available = availableValue.NO,
                                beginDate = date.AddHours(8),
                                endDate = date.AddHours(12),
                            },
                            new slotInfo
                            {
                                available = availableValue.YES,
                                beginDate = date.AddHours(12),
                                endDate = date.AddHours(16),
                            },
                        }
                    }
                },
                new []{new AppointmentSlot{StartTime = date.AddHours(12), EndTime = date.AddHours(16)}}
            };
        }

        [Fact]
        public async void GivenDrsCheckAvailabilityResponseContainsDaysWithNoSlots_WhenCheckingAvailability_ThenArgumentNullExceptionIsNotThrown()
        {
            // Arrange
            var dateTime = new DateTime(2022, 1, 19);

            var soapResponse = new checkAvailabilityResponse(
                new xmbCheckAvailabilityResponse { theSlots = new[] { new daySlotsInfo { day = dateTime } } }
            );

            _soapMock
                .Setup(x => x.checkAvailabilityAsync(It.IsAny<checkAvailability>()))
                .ReturnsAsync(soapResponse);

            // Act
            var appointmentSlots = await _systemUnderTest.CheckAvailability(SorCode, LocationId, dateTime);
            Func<IEnumerable<AppointmentSlot>> act = () => appointmentSlots.ToArray();

            // Assert
            act.Should().NotThrow<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenInvalidBookingReference_WhenCreatingAnOrder_ThenExceptionIsThrown<T>(T exception, string bookingReference) where T : Exception
        {
            // Arrange

            // Act
            Func<Task> act = async () => await _systemUnderTest.CreateOrder(bookingReference, It.IsAny<string>(), It.IsAny<string>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }


        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenInvalidSorCode_WhenCreatingAnOrder_ThenExceptionIsThrown<T>(T exception, string sorCode) where T : Exception
        {
            // Arrange

            // Act
            Func<Task> act = async () => await _systemUnderTest.CreateOrder(BookingReference, sorCode, It.IsAny<string>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }


        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenInvalidLocationId_WhenCreatingAnOrder_ThenExceptionIsThrown<T>(T exception, string locationId) where T : Exception
        {
            // Arrange

            // Act
            Func<Task> act = async () => await _systemUnderTest.CreateOrder(BookingReference, SorCode, locationId);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Fact]
        public async void GivenCreateOrderResponse_WhenCreatingAnOrder_ThenBookingIdIsPresentInTheResponse()
        {
            // Arrange
            var soapResponse = new createOrderResponse(new xmbCreateOrderResponse
            {
                status = responseStatus.success,
                theOrder = new order { theBookings = new[] { new booking { bookingId = BookingId } } }
            });

            _soapMock
                .Setup(x => x.createOrderAsync(It.IsAny<createOrder>()))
                .ReturnsAsync(soapResponse);

            // Act
            var actual = await _systemUnderTest.CreateOrder(BookingReference, SorCode, LocationId);

            // Assert
            Assert.Equal(BookingId, actual);
        }


        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
        public async void GivenInvalidBookingReference_WhenSchedulingABooking_ThenExceptionIsThrown<T>(T exception, string bookingReference) where T : Exception
        {
            // Arrange

            // Act
            Func<Task> act = async () => await _systemUnderTest.ScheduleBooking(bookingReference, It.IsAny<int>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Fact]
        public async void GivenAnEndDateEarlierThanTheStartDate_WhenExecute_ThenInvalidExceptionIsThrown()
        {
            // Arrange
            var startDate = new DateTime(2022, 1, 21);
            var endDate = startDate.AddDays(-1);

            // Act
            Func<Task> act = async () =>
                await _systemUnderTest.ScheduleBooking(BookingReference, BookingId, startDate, endDate);

            // Assert
            await act.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async void GivenScheduleBookingResponse_WhenSchedulingABooking_ThenDrsSoapScheduleBookingIsCalled()
        {
            // Arrange
            scheduleBooking request = null;

            Expression<Action<SOAP>> schedulingBookingExpression = x => x.scheduleBookingAsync(It.IsAny<scheduleBooking>());

            _soapMock
                .Setup(schedulingBookingExpression)
                .Callback<scheduleBooking>(r => request = r);

            var startDate = new DateTime(2022, 1, 21);
            var endDate = startDate.AddDays(1);

            // Act
            await _systemUnderTest.ScheduleBooking(BookingReference, BookingId, startDate, endDate);

            // Assert
            _soapMock.Verify(schedulingBookingExpression);
            Assert.True(request.scheduleBooking1.theBooking.assignedStartSpecified);
            Assert.True(request.scheduleBooking1.theBooking.assignedEndSpecified);

        }

        public static IEnumerable<object[]> InvalidArgumentTestData()
        {
            yield return new object[] { new ArgumentNullException(), null };
            yield return new object[] { new ArgumentException(), "" };
            yield return new object[] { new ArgumentException(), " " };
        }
    }
}
