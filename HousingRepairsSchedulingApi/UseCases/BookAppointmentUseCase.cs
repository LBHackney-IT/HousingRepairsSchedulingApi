namespace HousingRepairsSchedulingApi.UseCases
{
    using System;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Gateways;
    using HousingRepairsSchedulingApi.Domain;

    public class BookAppointmentUseCase : IBookAppointmentUseCase
    {
        private readonly IAppointmentsGateway appointmentsGateway;

        public BookAppointmentUseCase(IAppointmentsGateway appointmentsGateway) => this.appointmentsGateway = appointmentsGateway;

        public Task<SchedulingApiBookingResponse> Execute(string bookingReference, string sorCode, string locationId,
            DateTime startDateTime, DateTime endDateTime)
        {
            Guard.Against.NullOrWhiteSpace(bookingReference, nameof(bookingReference));
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));
            Guard.Against.OutOfRange(endDateTime, nameof(endDateTime), startDateTime, DateTime.MaxValue);

            var result = this.appointmentsGateway.BookAppointment(bookingReference, sorCode, locationId,
                startDateTime, endDateTime);

            return result;
        }
    }
}
