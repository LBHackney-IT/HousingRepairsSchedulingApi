namespace HousingRepairsSchedulingApi.Services.Drs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Domain;
    using Microsoft.Extensions.Options;

    public class DrsService : IDrsService
    {
        private const string DrsContract = "H01";
        private const string DummyPrimaryOrderNumber = "HousingRepairsOnlineDummyPrimaryOrderNumber";
        private const string DummyUserId = "HousingRepairsOnlineUserId";
        private const string Priority = "N";

        private readonly SOAP drsSoapClient;
        private readonly IOptions<DrsOptions> drsOptions;

        private string sessionId;

        public DrsService(SOAP drsSoapClient, IOptions<DrsOptions> drsOptions)
        {
            Guard.Against.Null(drsSoapClient, nameof(drsSoapClient));
            Guard.Against.Null(drsOptions, nameof(drsOptions));

            this.drsSoapClient = drsSoapClient;
            this.drsOptions = drsOptions;
        }

        public async Task<IEnumerable<AppointmentSlot>> CheckAvailability(string sorCode, string locationId, DateTime earliestDate)
        {
            await EnsureSessionOpened();

            var checkAvailability = new xmbCheckAvailability
            {
                sessionId = this.sessionId,
                periodBegin = earliestDate,
                periodBeginSpecified = true,
                periodEnd = earliestDate.AddDays(drsOptions.Value.SearchTimeSpanInDays - 1),
                periodEndSpecified = true,
                theOrder = new order
                {
                    userId = DummyUserId,
                    contract = DrsContract,
                    locationID = locationId,
                    primaryOrderNumber = DummyPrimaryOrderNumber,
                    priority = Priority,
                    theBookingCodes = new[]{
                        new bookingCode {
                            bookingCodeSORCode = sorCode,
                            itemNumberWithinBooking = "1",
                            primaryOrderNumber = DummyPrimaryOrderNumber,
                            quantity = "1",
                        }
                    }
                }
            };

            var checkAvailabilityResponse = await this.drsSoapClient.checkAvailabilityAsync(new checkAvailability(checkAvailability));

            var appointmentSlots = checkAvailabilityResponse.@return.theSlots
                .Where(x => x.slotsForDay != null)
                .SelectMany(x =>
                    x.slotsForDay.Where(y => y.available == availableValue.YES).Select(y =>
                        new AppointmentSlot
                        {
                            StartTime = y.beginDate,
                            EndTime = y.endDate,
                        }
                    )
            );

            return appointmentSlots;
        }

        public async Task<int> CreateOrder(string bookingReference, string sorCode, string locationId)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));

            await EnsureSessionOpened();

            var createOrder = new xmbCreateOrder
            {
                sessionId = this.sessionId,
                theOrder = new order
                {
                    contract = DrsContract,
                    locationID = locationId,
                    orderComments = " ",
                    primaryOrderNumber = bookingReference,
                    priority = Priority,
                    targetDate = DateTime.Today.AddDays(20),
                    userId = DummyUserId,
                    theBookingCodes = new[]
                    {
                        new bookingCode
                        {
                            bookingCodeSORCode = sorCode,
                            itemNumberWithinBooking = "1",
                            primaryOrderNumber = bookingReference,
                            quantity = "1",
                        }
                    }
                }
            };

            var createOrderResponse = await drsSoapClient.createOrderAsync(new createOrder(createOrder));
            var result = createOrderResponse.@return.theOrder.theBookings[0].bookingId;

            return result;
        }

        public async Task ScheduleBooking(string bookingReference, int bookingId, DateTime startDateTime, DateTime endDateTime)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.OutOfRange(endDateTime, nameof(endDateTime), startDateTime, DateTime.MaxValue);

            await EnsureSessionOpened();

            var scheduleBooking = new xmbScheduleBooking
            {
                sessionId = sessionId,
                theBooking = new booking
                {
                    bookingId = bookingId,
                    contract = DrsContract,
                    primaryOrderNumber = bookingReference,
                    planningWindowStart = startDateTime,
                    planningWindowEnd = endDateTime,
                }
            };

            _ = await this.drsSoapClient.scheduleBookingAsync(new scheduleBooking(scheduleBooking));
        }

        private async Task OpenSession()
        {
            var xmbOpenSession = new xmbOpenSession
            {
                login = drsOptions.Value.Login,
                password = drsOptions.Value.Password
            };
            var response = await this.drsSoapClient.openSessionAsync(new openSession(xmbOpenSession));

            sessionId = response.@return.sessionId;
        }

        private async Task EnsureSessionOpened()
        {
            if (this.sessionId == null)
            {
                await OpenSession();
            }
        }
    }
}
