namespace HousingRepairsSchedulingApi.UseCases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Domain;
    using HACT.Dtos;
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

        public async Task<IEnumerable<Appointment>> Execute(string sorCode, string locationId, DateTime? fromDate = null)
        {
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));

            var availableAppointments = await _appointmentsGateway.GetAvailableAppointments(sorCode, locationId, fromDate);

            _logger.LogInformation("RetrieveAvailableAppointmentsUseCase received {NumberOfAvailableAppointments} from AppointmentGateway for {LocationId}", availableAppointments.Count(), locationId);

            return availableAppointments.Select(x => x.ToHactAppointment());
        }
    }
}
