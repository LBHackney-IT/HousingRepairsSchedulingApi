namespace HousingRepairsSchedulingApi.UseCases
{
    using System;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using HousingRepairsSchedulingApi.Boundary.Requests;
    using HousingRepairsSchedulingApi.Gateways.Interfaces;
    using HousingRepairsSchedulingApi.UseCases.Interfaces;

    public class BookAppointmentUseCase : IBookAppointmentUseCase
    {
        private readonly IAppointmentsGateway _appointmentsGateway;

        public BookAppointmentUseCase(IAppointmentsGateway appointmentsGateway)
        {
            _appointmentsGateway = appointmentsGateway;
        }

        public Task<string> Execute(BookAppointmentRequest request)
        {
            Guard.Against.NullOrWhiteSpace(request.BookingReference, nameof(request.BookingReference));
            Guard.Against.NullOrWhiteSpace(request.SorCode, nameof(request.SorCode));
            Guard.Against.NullOrWhiteSpace(request.LocationId, nameof(request.LocationId));
            Guard.Against.OutOfRange(request.EndDateTime, nameof(request.EndDateTime), request.StartDateTime, DateTime.MaxValue);

            return _appointmentsGateway.BookAppointment(request);
        }
    }
}
