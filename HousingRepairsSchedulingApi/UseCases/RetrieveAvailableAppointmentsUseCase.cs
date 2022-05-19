namespace HousingRepairsSchedulingApi.UseCases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Ardalis.GuardClauses;
    using Domain;
    using Gateways;
    using HACT.Dtos;

    public class RetrieveAvailableAppointmentsUseCase : IRetrieveAvailableAppointmentsUseCase
    {
        private readonly IAppointmentsGateway appointmentsGateway;

        public RetrieveAvailableAppointmentsUseCase(IAppointmentsGateway appointmentsGateway) => this.appointmentsGateway = appointmentsGateway;

        public async Task<IEnumerable<Appointment>> Execute(string sorCode, string locationId, DateTime? fromDate = null)
        {
            Guard.Against.NullOrWhiteSpace(sorCode, nameof(sorCode));
            Guard.Against.NullOrWhiteSpace(locationId, nameof(locationId));

            var availableAppointments = await this.appointmentsGateway.GetAvailableAppointments(sorCode, locationId, fromDate);

            var result = availableAppointments.Select(x => x.ToHactAppointment());

            return result;
        }
    }
}
