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
    using Microsoft.Extensions.Logging;

    public class RetrieveAvailableAppointmentsUseCase : IRetrieveAvailableAppointmentsUseCase
    {
        private readonly IAppointmentsGateway _appointmentsGateway;
        private readonly ILogger<RetrieveAvailableAppointmentsUseCase> _logger;

        public RetrieveAvailableAppointmentsUseCase(IAppointmentsGateway appointmentsGateway, ILogger<RetrieveAvailableAppointmentsUseCase> logger)
        {
            _appointmentsGateway = appointmentsGateway;
            _logger = logger;
        }

        public async Task<IEnumerable<Appointment>> Execute(GetAvailableAppointmentsRequest request)
        {
            Guard.Against.NullOrWhiteSpace(request.SorCode, nameof(request.SorCode));
            Guard.Against.NullOrWhiteSpace(request.LocationId, nameof(request.LocationId));

            var availableAppointments = await _appointmentsGateway.GetAvailableAppointments(request);

            _logger.LogInformation("RetrieveAvailableAppointmentsUseCase received {NumberOfAvailableAppointments} from AppointmentGateway for {LocationId}", availableAppointments.Count(), request.LocationId);

            return availableAppointments.Select(x => x.ToHactAppointment());
        }
    }
}
