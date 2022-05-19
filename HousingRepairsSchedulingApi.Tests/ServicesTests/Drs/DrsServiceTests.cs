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

        private readonly Mock<SOAP> soapMock;

        private readonly DrsService systemUnderTest;

        public DrsServiceTests()
        {
            var drsOptionsMock = new Mock<IOptions<DrsOptions>>();
            drsOptionsMock.Setup(x => x.Value)
                .Returns(new DrsOptions { Login = "login", Password = "password" });

            this.soapMock = new Mock<SOAP>();
            this.soapMock.Setup(x => x.openSessionAsync(It.IsAny<openSession>()))
                .ReturnsAsync(new openSessionResponse
                {
                    @return = new xmbOpenSessionResponse { sessionId = "sessionId" }
                });

            this.systemUnderTest = new DrsService(this.soapMock.Object, drsOptionsMock.Object);

        }

        [Fact]
        public void GivenNullDrsSoapClientParameter_WhenInstantiating_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange

            // Act
            Func<DrsService> act = () => new DrsService(null, It.IsAny<IOptions<DrsOptions>>());

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("drsSoapClient");
        }

        [Fact]
        public void GivenNullDrsOptionsParameter_WhenInstantiating_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange

            // Act
            Func<DrsService> act = () => new DrsService(new Mock<SOAP>().Object, null);

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("drsOptions");
        }

        [Theory]
        [MemberData(nameof(UnavailableSlotsTestData))]
        public async void
            GivenDrsCheckAvailabilityResponseContainsUnavailableSlots_WhenCheckingAvailability_ThenOnlyAvailableSlotsAreReturned(DateTime searchDate, daySlotsInfo[] daySlots, IEnumerable<AppointmentSlot> expected)
        {
            // Arrange

            this.soapMock.Setup(x => x.checkAvailabilityAsync(It.IsAny<checkAvailability>()))
                .ReturnsAsync(new checkAvailabilityResponse(
                    new xmbCheckAvailabilityResponse { theSlots = daySlots }));

            // Act
            var appointmentSlots = await this.systemUnderTest.CheckAvailability(SorCode, LocationId, searchDate);

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
        public async void
            GivenDrsCheckAvailabilityResponseContainsDaysWithNoSlots_WhenCheckingAvailability_ThenArgumentNullExceptionIsNotThrown()
        {
            // Arrange
            var dateTime = new DateTime(2022, 1, 19);

            this.soapMock.Setup(x => x.checkAvailabilityAsync(It.IsAny<checkAvailability>()))
                .ReturnsAsync(new checkAvailabilityResponse(
                    new xmbCheckAvailabilityResponse { theSlots = new[] { new daySlotsInfo { day = dateTime } } }));

            // Act
            var appointmentSlots = await this.systemUnderTest.CheckAvailability(SorCode, LocationId, dateTime);
            Func<IEnumerable<AppointmentSlot>> act = () => appointmentSlots.ToArray();

            // Assert
            act.Should().NotThrow<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenInvalidBookingReference_WhenCreatingAnOrder_ThenExceptionIsThrown<T>(T exception, string bookingReference) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await this.systemUnderTest.CreateOrder(bookingReference, It.IsAny<string>(), It.IsAny<string>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }


        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenInvalidSorCode_WhenCreatingAnOrder_ThenExceptionIsThrown<T>(T exception, string sorCode) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await this.systemUnderTest.CreateOrder(BookingReference, sorCode, It.IsAny<string>());

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }


        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenInvalidLocationId_WhenCreatingAnOrder_ThenExceptionIsThrown<T>(T exception, string locationId) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await this.systemUnderTest.CreateOrder(BookingReference, SorCode, locationId);

            // Assert
            await act.Should().ThrowExactlyAsync<T>();
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenCreateOrderResponse_WhenCreatingAnOrder_ThenBookingIdIsPresentInTheResponse()
#pragma warning restore CA1707
        {
            // Arrange
            this.soapMock.Setup(x => x.createOrderAsync(It.IsAny<createOrder>()))
                .ReturnsAsync(new createOrderResponse(new xmbCreateOrderResponse
                {
                    theOrder = new order { theBookings = new[] { new booking { bookingId = BookingId } } }
                }));

            // Act
            var actual = await this.systemUnderTest.CreateOrder(BookingReference, SorCode, LocationId);

            // Assert
            Assert.Equal(BookingId, actual.bookingId);

        }

        [Fact]
        public async void GivenValidCreateOrderResponse_WhenCreatingAnOrder_ThenDrsIdIsPresentInTheResponse()
        {
            // Arrange
            var expectedGuid = Guid.NewGuid().ToString();

            this.soapMock.Setup(x => x.createOrderAsync(It.IsAny<createOrder>()))
                .ReturnsAsync(new createOrderResponse(new xmbCreateOrderResponse
                {
                    theOrder = new order
                    {
                        theBookings = new[] { new booking {
                        bookingId = BookingId,
                        tokenId = expectedGuid
                    } }
                    }
                }));

            // Act
            var actual = await this.systemUnderTest.CreateOrder(BookingReference, SorCode, LocationId);

            // Assert
            Assert.Equal(expectedGuid, actual.tokenId);
        }


        [Theory]
        [MemberData(nameof(InvalidArgumentTestData))]
#pragma warning disable xUnit1026
#pragma warning disable CA1707
        public async void GivenInvalidBookingReference_WhenSchedulingABooking_ThenExceptionIsThrown<T>(T exception, string bookingReference) where T : Exception
#pragma warning restore CA1707
#pragma warning restore xUnit1026
        {
            // Arrange

            // Act
            Func<Task> act = async () => await this.systemUnderTest.ScheduleBooking(bookingReference, It.IsAny<int>(),
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
                await this.systemUnderTest.ScheduleBooking(BookingReference, BookingId, startDate, endDate);

            // Assert
            await act.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
#pragma warning disable CA1707
        public async void GivenScheduleBookingResponse_WhenSchedulingABooking_ThenDrsSoapScheduleBookingIsCalled()
#pragma warning restore CA1707
        {
            // Arrange
            Expression<Action<SOAP>> schedulingBookingExpression = x => x.scheduleBookingAsync(It.IsAny<scheduleBooking>());
            this.soapMock.Setup(schedulingBookingExpression);
            var startDate = new DateTime(2022, 1, 21);
            var endDate = startDate.AddDays(1);

            // Act
            await this.systemUnderTest.ScheduleBooking(BookingReference, BookingId, startDate, endDate);

            // Assert
            this.soapMock.Verify(schedulingBookingExpression);
        }

        public static IEnumerable<object[]> InvalidArgumentTestData()
        {
            yield return new object[] { new ArgumentNullException(), null };
            yield return new object[] { new ArgumentException(), "" };
            yield return new object[] { new ArgumentException(), " " };
        }
    }
}
