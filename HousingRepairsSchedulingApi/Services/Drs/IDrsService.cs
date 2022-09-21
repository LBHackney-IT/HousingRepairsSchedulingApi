namespace HousingRepairsSchedulingApi.Services.Drs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;

    public interface IDrsService
    {
        Task<IEnumerable<AppointmentSlot>> CheckAvailability(string sorCode, string locationId, DateTime earliestDate);

        Task<int> CreateOrder(string bookingReference, string sorCode, string locationId);

        Task ScheduleBooking(string bookingReference, int bookingId, DateTime startDateTime, DateTime endDateTime);

        Task<order> SelectOrder(int workOrderId, DateTime? validationDate);
    }
}
