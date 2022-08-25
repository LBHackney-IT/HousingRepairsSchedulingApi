namespace HousingRepairsSchedulingApi.UseCases.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HACT.Dtos;

    public interface IRetrieveAvailableAppointmentsUseCase
    {
        public Task<IEnumerable<Appointment>> Execute(string sorCode, string locationId, DateTime? fromDate);
    }
}
