namespace HousingRepairsSchedulingApi.UseCases
{
    using System;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using HousingRepairsSchedulingApi.Gateways.Interfaces;
    using HousingRepairsSchedulingApi.UseCases.Interfaces;

    public class BookAppointmentUseCase : IBookAppointmentUseCase
    {
        private readonly IAppointmentsGateway _appointmentsGateway;

        public BookAppointmentUseCase(IAppointmentsGateway appointmentsGateway)
        {
            _appointmentsGateway = appointmentsGateway;
        }

        public Task<string> Execute(
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

            return _appointmentsGateway.BookAppointment(bookingReference, sorCode, locationId, startDateTime, endDateTime);
        }
    }
}
