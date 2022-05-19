namespace HousingRepairsSchedulingApi.UseCases
{
    using System;
    using System.Threading.Tasks;
    using HousingRepairsSchedulingApi.Domain;

    public interface IBookAppointmentUseCase
    {
        public Task<SchedulingApiBookingResponse> Execute(string bookingReference, string sorCode, string locationId,
            DateTime startDateTime, DateTime endDateTime);
    }
}
