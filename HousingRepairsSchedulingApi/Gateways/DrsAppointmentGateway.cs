namespace HousingRepairsSchedulingApi.Gateways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Domain;
    using Services.Drs;

    public class DrsAppointmentGateway : IAppointmentsGateway
    {
        private readonly int requiredNumberOfAppointmentDays;
        private readonly int appointmentSearchTimeSpanInDays;
        private readonly int appointmentLeadTimeInDays;
        private readonly int maximumNumberOfRequests;
        private readonly IDrsService drsService;

        public DrsAppointmentGateway(IDrsService drsService, int requiredNumberOfAppointmentDays,
            int appointmentSearchTimeSpanInDays, int appointmentLeadTimeInDays, int maximumNumberOfRequests)
        {
            Guard.Against.Null(drsService, nameof(drsService));
            Guard.Against.NegativeOrZero(requiredNumberOfAppointmentDays, nameof(requiredNumberOfAppointmentDays));
            Guard.Against.NegativeOrZero(appointmentSearchTimeSpanInDays, nameof(appointmentSearchTimeSpanInDays));
            Guard.Against.Negative(appointmentLeadTimeInDays, nameof(appointmentLeadTimeInDays));
            Guard.Against.NegativeOrZero(maximumNumberOfRequests, nameof(maximumNumberOfRequests));

            this.drsService = drsService;
            this.requiredNumberOfAppointmentDays = requiredNumberOfAppointmentDays;
            this.appointmentSearchTimeSpanInDays = appointmentSearchTimeSpanInDays;
            this.appointmentLeadTimeInDays = appointmentLeadTimeInDays;
            this.maximumNumberOfRequests = maximumNumberOfRequests;
        }

        public async Task<IEnumerable<AppointmentSlot>> GetAvailableAppointments(string sorCode, string locationId,
            DateTime? fromDate = null)
        {
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));

            var earliestDate = fromDate ?? DateTime.Today.AddDays(this.appointmentLeadTimeInDays);
            var appointmentSlots = Enumerable.Empty<AppointmentSlot>();

            var numberOfRequests = 0;
            while (numberOfRequests < this.maximumNumberOfRequests && appointmentSlots.Select(x => x.StartTime.Date).Distinct().Count() < this.requiredNumberOfAppointmentDays)
            {
                numberOfRequests++;
                var appointments = await this.drsService.CheckAvailability(sorCode, locationId, earliestDate);
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
                appointmentSlots = appointmentSlots.Concat(appointments);
                earliestDate = earliestDate.AddDays(this.appointmentSearchTimeSpanInDays);
            }

            appointmentSlots = appointmentSlots.GroupBy(x => x.StartTime.Date).Take(this.requiredNumberOfAppointmentDays)
                .SelectMany(x => x.Select(y => y));

            return appointmentSlots;
        }

        public async Task<SchedulingApiBookingResponse> BookAppointment(string bookingReference, string sorCode, string locationId,
            DateTime startDateTime, DateTime endDateTime)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));
            Guard.Against.OutOfRange(endDateTime, nameof(endDateTime), startDateTime, DateTime.MaxValue);

            var booking = await this.drsService.CreateOrder(bookingReference, sorCode, locationId);

            await this.drsService.ScheduleBooking(bookingReference, booking.bookingId, startDateTime, endDateTime);

            return new SchedulingApiBookingResponse
            {
                BookingReference = bookingReference,
                TokenId = booking.tokenId
            };
        }
    }
}
