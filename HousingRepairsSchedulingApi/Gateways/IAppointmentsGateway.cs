namespace HousingRepairsSchedulingApi.Gateways
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HousingRepairsSchedulingApi.Domain;

    public interface IAppointmentsGateway
    {
        Task<IEnumerable<AppointmentSlot>> GetAvailableAppointments(string sorCode, string locationId, DateTime? fromDate = null);

        Task<SchedulingApiBookingResponse> BookAppointment(string bookingReference, string sorCode, string locationId, DateTime startDateTime, DateTime endDateTime);
    }
}
