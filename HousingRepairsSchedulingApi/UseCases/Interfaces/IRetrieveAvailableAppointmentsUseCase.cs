namespace HousingRepairsSchedulingApi.UseCases.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HACT.Dtos;
    using HousingRepairsSchedulingApi.Boundary.Requests;

    public interface IRetrieveAvailableAppointmentsUseCase
    {
        public Task<IEnumerable<Appointment>> Execute(GetAvailableAppointmentsRequest request);
    }
}
