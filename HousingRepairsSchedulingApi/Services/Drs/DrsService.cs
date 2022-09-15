namespace HousingRepairsSchedulingApi.Services.Drs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;
    using Ardalis.GuardClauses;
    using Domain;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class DrsService : IDrsService
    {
        private const string DrsContract = "H01";
        private const string DummyPrimaryOrderNumber = "HousingRepairsOnlineDummyPrimaryOrderNumber";
        private const string DummyUserId = "HousingRepairsOnline";
        private const string Priority = "N";

        private readonly SOAP _drsSoapClient;
        private readonly IOptions<DrsOptions> _drsOptions;

        private string _sessionId;
        private readonly ILogger<DrsService> _logger;


        public DrsService(SOAP drsSoapClient, IOptions<DrsOptions> drsOptions, ILogger<DrsService> logger)
        {
            Guard.Against.Null(drsSoapClient, nameof(drsSoapClient));
            Guard.Against.Null(drsOptions, nameof(drsOptions));

            _drsSoapClient = drsSoapClient;
            _drsOptions = drsOptions;
            _logger = logger;
        }

        public async Task<IEnumerable<AppointmentSlot>> CheckAvailability(string sorCode, string locationId, DateTime earliestDate)
        {
            await EnsureSessionOpened();

            var checkAvailability = new xmbCheckAvailability
            {
                sessionId = _sessionId,
                periodBegin = earliestDate,
                periodBeginSpecified = true,
                periodEnd = earliestDate.AddDays(_drsOptions.Value.SearchTimeSpanInDays - 1),
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

            var checkAvailabilityResponse = await _drsSoapClient.checkAvailabilityAsync(new checkAvailability(checkAvailability));

            _logger.LogInformation("Called checkAvailabilityAsync for {LocationId}", locationId);

            if (checkAvailabilityResponse?.@return?.theSlots == null)
            {
                _logger.LogInformation("checkAvailabilityAsync returned an invalid response for {LocationId}", locationId);
            }

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

            _logger.LogInformation("Called checkAvailabilityAsync for {LocationId} returning {Count} appointment slots", locationId, appointmentSlots?.Count() ?? 0);

            return appointmentSlots;
        }

        public async Task<int> CreateOrder(string bookingReference, string sorCode, string locationId)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));

            await EnsureSessionOpened();

            LambdaLogger.Log($"About to call CreateOrder with following parameters booking reference: {bookingReference}, sorCode {sorCode}, locationId {locationId};");

            var createOrder = new xmbCreateOrder
            {
                sessionId = _sessionId,
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

            LambdaLogger.Log($"Built create order object {bookingReference}, sorCode {sorCode}, locationId {locationId};");

            var createOrderResponse = await _drsSoapClient.createOrderAsync(new createOrder(createOrder));

            if (createOrderResponse?.@return?.theOrder?.theBookings[0]?.bookingId == null)
            {
                _logger.LogInformation("createOrderAsync returned an invalid response for {LocationId} ", locationId);
            }

            LambdaLogger.Log($"Successfully called createOrderAsync with {bookingReference}. createOrderResponse: {createOrderResponse}");

            LambdaLogger.Log($"'createOrderResponse' for booking reference {bookingReference}" + ((createOrderResponse == null) ? "is null" : "is not null"));

            LambdaLogger.Log($"'createOrderResponse.@return' for booking reference {bookingReference}" + ((createOrderResponse?.@return == null) ? "is null" : "is not null"));

            //LambdaLogger.Log($"'createOrderResponse.@return.theOrder.theBookings[0].bookingId' for booking reference {bookingReference}: {createOrderResponse?.@return?.theOrder?.theBookings[0]?.bookingId}");

            //LambdaLogger.Log($"Output from'createOrderResponse.@return.theOrder.theBookings' arrayfor booking reference {bookingReference}. Result: {createOrderResponse?.@return?.theOrder?.theBookings}");

            //LambdaLogger.Log($"Attempting to JSON Serialize 'createOrderResponse.@return.theOrder.theBookings[0]' for booking reference {bookingReference}. Result: {JsonSerializer.Serialize(createOrderResponse?.@return?.theOrder?.theBookings[0])}");

            //LambdaLogger.Log($"Primary Order Number from 'createOrderResponse.@return.theOrder.primaryOrderNumber' for booking reference {bookingReference}. Result: {createOrderResponse?.@return?.theOrder?.primaryOrderNumber}");

            //LambdaLogger.Log($"Contract from 'createOrderResponse.@return.theOrder.theBookings[0].contract' for booking reference {bookingReference}. Result: {createOrderResponse?.@return?.theOrder?.theBookings[0]?.contract}");

            var result = createOrderResponse.@return.theOrder.theBookings[0].bookingId;

            LambdaLogger.Log($"Returning result after sending work order {bookingReference} to DRS. Response booking ID: {result};");

            return result;
        }

        public async Task ScheduleBooking(string bookingReference, int bookingId, DateTime startDateTime, DateTime endDateTime)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.OutOfRange(endDateTime, nameof(endDateTime), startDateTime, DateTime.MaxValue);

            _logger.LogInformation($"Scheduling booking in DRS. Booking reference {bookingReference}. Start time is {startDateTime} and end time is {endDateTime}.");

            await EnsureSessionOpened();

            var scheduleBooking = new xmbScheduleBooking
            {
                sessionId = _sessionId,
                theBooking = new booking
                {
                    bookingId = bookingId,
                    contract = DrsContract,
                    primaryOrderNumber = bookingReference,
                    planningWindowStart = startDateTime,
                    planningWindowEnd = endDateTime,
                    assignedStart = startDateTime,
                    assignedEnd = endDateTime,
                    assignedStartSpecified = true,
                    assignedEndSpecified = true
                }
            };

            _logger.LogInformation($"scheduleBooking object for booking reference {bookingReference}: {JsonSerializer.Serialize(scheduleBooking)}");

            _ = await _drsSoapClient.scheduleBookingAsync(new scheduleBooking(scheduleBooking));
        }

        private async Task OpenSession()
        {
            var xmbOpenSession = new xmbOpenSession
            {
                login = _drsOptions.Value.Login,
                password = _drsOptions.Value.Password
            };

            var response = await _drsSoapClient.openSessionAsync(new openSession(xmbOpenSession));

            _sessionId = response.@return.sessionId;
        }

        private async Task EnsureSessionOpened()
        {
            if (_sessionId == null)
            {
                await OpenSession();
            }
        }
    }
}
