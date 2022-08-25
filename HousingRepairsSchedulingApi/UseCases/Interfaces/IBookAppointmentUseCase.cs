namespace HousingRepairsSchedulingApi.UseCases.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using HousingRepairsSchedulingApi.Boundary.Requests;

    public interface IBookAppointmentUseCase
    {
        public Task<string> Execute(BookAppointmentRequest request);
    }
}
