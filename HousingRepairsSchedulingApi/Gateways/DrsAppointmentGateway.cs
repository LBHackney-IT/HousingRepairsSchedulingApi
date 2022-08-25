namespace HousingRepairsSchedulingApi.Gateways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;
    using Ardalis.GuardClauses;
    using Domain;
    using Helpers;
    using HousingRepairsSchedulingApi.Boundary.Requests;
    using HousingRepairsSchedulingApi.Gateways.Interfaces;
    using Services.Drs;

    public class DrsAppointmentGateway : IAppointmentsGateway
    {
        private readonly int _requiredNumberOfAppointmentDays;
        private readonly int _appointmentSearchTimeSpanInDays;
        private readonly int _appointmentLeadTimeInDays;
        private readonly int _maximumNumberOfRequests;
        private readonly IDrsService _drsService;

        public DrsAppointmentGateway(
            IDrsService drsService,
            int requiredNumberOfAppointmentDays,
            int appointmentSearchTimeSpanInDays,
            int appointmentLeadTimeInDays,
            int maximumNumberOfRequests)
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
        }

        public async Task<IEnumerable<AppointmentSlot>> GetAvailableAppointments(GetAvailableAppointmentsRequest request)
        {
            Guard.Against.NullOrWhiteSpace(request.SorCode, nameof(request.SorCode));
            Guard.Against.NullOrWhiteSpace(request.LocationId, nameof(request.LocationId));

            LambdaLogger.Log($"About to GetAvailableAppointments for location: {request.LocationId}");

            var appointmentSlots = await GetAppointmentSlots(request);

            appointmentSlots = appointmentSlots
                .GroupBy(x => x.StartTime.Date)
                .Take(_requiredNumberOfAppointmentDays)
                .SelectMany(x => x.Select(y => y));


            LambdaLogger.Log($"GetAvailableAppointments returned {appointmentSlots.Count()} appointment slots for location: {request.LocationId}");

            return appointmentSlots;
        }

        private async Task<IEnumerable<AppointmentSlot>> GetAppointmentSlots(GetAvailableAppointmentsRequest request)
        {
            var earliestDate = request.FromDate ?? DateTime.Today.AddDays(_appointmentLeadTimeInDays);
            var appointmentSlots = Enumerable.Empty<AppointmentSlot>();

            var numberOfRequests = 0;

            while (numberOfRequests < _maximumNumberOfRequests && appointmentSlots.Select(x => x.StartTime.Date).Distinct().Count() < _requiredNumberOfAppointmentDays)
            {
                numberOfRequests++;
                var appointments = await GetValidAppointments(request.SorCode, request.LocationId, earliestDate);

                appointmentSlots = appointmentSlots.Concat(appointments);
                earliestDate = earliestDate.AddDays(_appointmentSearchTimeSpanInDays);
            }

            return appointmentSlots;
        }

        private async Task<IEnumerable<AppointmentSlot>> GetValidAppointments(string sorCode, string locationId, DateTime earliestDate)
        {
            var appointments = await _drsService.CheckAvailability(sorCode, locationId, earliestDate);

            appointments = appointments.Where(x =>
                !(x.StartTime.Hour == 9 && x.EndTime.Minute == 30 && x.EndTime.Hour == 14 && x.EndTime.Minute == 30) &&
                !(x.StartTime.Hour == 8 && x.EndTime.Minute == 0 && x.EndTime.Hour == 16 && x.EndTime.Minute == 0) &&
                !(x.StartTime.Hour == 8 && x.EndTime.Minute == 30 && x.EndTime.Hour == 13 && x.EndTime.Minute == 30) &&
                !(x.StartTime.Hour == 7 && x.EndTime.Minute == 0 && x.EndTime.Hour == 15 && x.EndTime.Minute == 0)
            );

            return appointments;
        }

        public async Task<string> BookAppointment(BookAppointmentRequest request)
        {
            Guard.Against.NullOrWhiteSpace(request.BookingReference, nameof(request.BookingReference));
            Guard.Against.NullOrWhiteSpace(request.SorCode, nameof(request.SorCode));
            Guard.Against.NullOrWhiteSpace(request.LocationId, nameof(request.LocationId));
            Guard.Against.OutOfRange(request.EndDateTime, nameof(request.EndDateTime), request.StartDateTime, DateTime.MaxValue);

            LambdaLogger.Log($"About to BookAppointment for location {request.LocationId}");

            var bookingId = await _drsService.CreateOrder(request.BookingReference, request.SorCode, request.LocationId);

            var convertedStartTime = DrsHelpers.ConvertToDrsTimeZone(request.StartDateTime);
            var convertedEndTime = DrsHelpers.ConvertToDrsTimeZone(request.EndDateTime);

            await _drsService.ScheduleBooking(request.BookingReference, bookingId, convertedStartTime, convertedEndTime);

            LambdaLogger.Log($"BookAppointment was successful for locaton {request.LocationId} with reference {request.BookingReference}");


            return request.BookingReference;
        }
    }
}
