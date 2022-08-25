namespace HousingRepairsSchedulingApi.UseCases.Interfaces
{
    using System;
    using System.Threading.Tasks;

    public interface IBookAppointmentUseCase
    {
        public Task<string> Execute(string bookingReference, string sorCode, string locationId,
            DateTime startDateTime, DateTime endDateTime);
    }
}
