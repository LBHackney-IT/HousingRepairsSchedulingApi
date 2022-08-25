namespace HousingRepairsSchedulingApi.UseCases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Domain;
    using HACT.Dtos;
    using HousingRepairsSchedulingApi.Boundary.Requests;
    using HousingRepairsSchedulingApi.Gateways.Interfaces;
    using HousingRepairsSchedulingApi.UseCases.Interfaces;

    public class RetrieveAvailableAppointmentsUseCase : IRetrieveAvailableAppointmentsUseCase
    {
        private readonly IAppointmentsGateway _appointmentsGateway;

        public RetrieveAvailableAppointmentsUseCase(IAppointmentsGateway appointmentsGateway)
        {
            _appointmentsGateway = appointmentsGateway;
        }

        public async Task<IEnumerable<Appointment>> Execute(GetAvailableAppointmentsRequest request)
        {
            Guard.Against.NullOrWhiteSpace(request.SorCode, nameof(request.SorCode));
            Guard.Against.NullOrWhiteSpace(request.LocationId, nameof(request.LocationId));

            var availableAppointments = await _appointmentsGateway.GetAvailableAppointments(request);

            return availableAppointments.Select(x => x.ToHactAppointment());
        }
    }
}
