namespace HousingRepairsSchedulingApi.Gateways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Domain;
    using HousingRepairsSchedulingApi.Exceptions;
    using HousingRepairsSchedulingApi.Gateways.Interfaces;
    using HousingRepairsSchedulingApi.Helpers;
    using Microsoft.Extensions.Logging;
    using Services.Drs;

    public class DrsAppointmentGateway : IAppointmentsGateway
    {
        private readonly int _requiredNumberOfAppointmentDays;
        private readonly int _appointmentSearchTimeSpanInDays;
        private readonly int _appointmentLeadTimeInDays;
        private readonly int _maximumNumberOfRequests;
        private readonly IDrsService _drsService;
        private readonly ILogger<DrsAppointmentGateway> _logger;

        public DrsAppointmentGateway(
            IDrsService drsService,
            int requiredNumberOfAppointmentDays,
            int appointmentSearchTimeSpanInDays,
            int appointmentLeadTimeInDays,
            int maximumNumberOfRequests,
            ILogger<DrsAppointmentGateway> logger)
        {
            Guard.Against.Null(drsService, nameof(drsService));
            Guard.Against.NegativeOrZero(requiredNumberOfAppointmentDays, nameof(requiredNumberOfAppointmentDays));
            Guard.Against.NegativeOrZero(appointmentSearchTimeSpanInDays, nameof(appointmentSearchTimeSpanInDays));
            Guard.Against.Negative(appointmentLeadTimeInDays, nameof(appointmentLeadTimeInDays));
            Guard.Against.NegativeOrZero(maximumNumberOfRequests, nameof(maximumNumberOfRequests));

            _drsService = drsService;
            _requiredNumberOfAppointmentDays = requiredNumberOfAppointmentDays;
            _appointmentSearchTimeSpanInDays = appointmentSearchTimeSpanInDays;
            _appointmentLeadTimeInDays = appointmentLeadTimeInDays;
            _maximumNumberOfRequests = maximumNumberOfRequests;
            _logger = logger;
        }

        public async Task<IEnumerable<AppointmentSlot>> GetAvailableAppointments(
            string sorCode,
            string locationId,
            DateTime? fromDate = null)
        {
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));

            var earliestDate = fromDate ?? DateTime.Today.AddDays(_appointmentLeadTimeInDays);
            var appointmentSlots = Enumerable.Empty<AppointmentSlot>();

            var numberOfRequests = 0;
            var numberOfAppointments = 0;

            while (numberOfRequests < _maximumNumberOfRequests && appointmentSlots.Select(x => x.StartTime.Date).Distinct().Count() < _requiredNumberOfAppointmentDays)
            {
                numberOfRequests++;

                var appointments = await GetValidAppointments(sorCode, locationId, earliestDate);

                numberOfAppointments += appointments.Count();

                appointmentSlots = appointmentSlots.Concat(appointments);
                earliestDate = earliestDate.AddDays(_appointmentSearchTimeSpanInDays);
            }

            appointmentSlots = appointmentSlots
                .GroupBy(x => x.StartTime.Date)
                .Take(_requiredNumberOfAppointmentDays)
                .SelectMany(x => x.Select(y => y));

            _logger.LogInformation("GetAvailableAppointments returned {NumberOfAppointmentSlots} from {NumberOfAppointments} for {LocationId}", appointmentSlots.Count(), numberOfAppointments, locationId);

            return appointmentSlots;
        }

        private async Task<IEnumerable<AppointmentSlot>> GetValidAppointments(string sorCode, string locationId, DateTime earliestDate)
        {
            _logger.LogInformation("About to call CheckAvailability for {LocationId}", locationId);

            var appointments = await _drsService.CheckAvailability(sorCode, locationId, earliestDate);

            var initialNumberOfAppointments = appointments.Count();

            appointments = appointments.Where(x =>
                !(x.StartTime.Hour == 9 && x.EndTime.Minute == 30
                  && x.EndTime.Hour == 14 && x.EndTime.Minute == 30) &&
                !(x.StartTime.Hour == 8 && x.EndTime.Minute == 0
                                        && x.EndTime.Hour == 16 && x.EndTime.Minute == 0) &&
                !(x.StartTime.Hour == 8 && x.EndTime.Minute == 30
                                        && x.EndTime.Hour == 13 && x.EndTime.Minute == 30) &&
                !(x.StartTime.Hour == 7 && x.EndTime.Minute == 0
                                        && x.EndTime.Hour == 15 && x.EndTime.Minute == 0)
            );

            _logger.LogInformation("GetValidAppointments returned {NumberOfValidAppointments} from {InitialNumberOfAppointments} for {LocationId}", appointments.Count(), initialNumberOfAppointments, locationId);

            return appointments;
        }

        public async Task<string> BookAppointment(
            string bookingReference,
            string sorCode,
            string locationId,
            DateTime startDateTime,
            DateTime endDateTime)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));
            Guard.Against.OutOfRange(endDateTime, nameof(endDateTime), startDateTime, DateTime.MaxValue);

            _logger.LogInformation($"Appointment times for booking reference {bookingReference} - start time is {startDateTime} and end time is {endDateTime}");

            var order = await _drsService.SelectOrder(int.Parse(bookingReference), DateTime.Now.AddDays(-30));

            if (order == null)
            {
                throw new DrsException($"The DRS order for booking reference {bookingReference} is null");
            }

            if (order.theBookings == null || order.theBookings.Count() < 1)
            {
                throw new DrsException($"No valid booking found in DRS order with booking reference {bookingReference}");
            }

            if (order.theBookings[0].bookingId == null || order.theBookings[0].bookingId == 0)
            {
                throw new DrsException($"Booking ID for DRS order with booking reference {bookingReference} is invalid");
            }

            _logger.LogInformation($"Order created successfully for {bookingReference}");

            var convertedStartTime = DrsHelpers.ConvertToDrsTimeZone(startDateTime);
            var convertedEndTime = DrsHelpers.ConvertToDrsTimeZone(endDateTime);

            _logger.LogInformation($"Converted times for booking reference {bookingReference} - start time is {convertedStartTime} and end time is {convertedEndTime} prior to sending to DRS");

            await _drsService.ScheduleBooking(bookingReference, order.theBookings[0].bookingId, convertedStartTime, convertedEndTime);

            return bookingReference;
        }
    }
}
