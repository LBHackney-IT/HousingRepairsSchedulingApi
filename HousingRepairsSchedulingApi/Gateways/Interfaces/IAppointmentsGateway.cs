using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HousingRepairsSchedulingApi.Gateways.Interfaces
{
    using Domain;
    using HousingRepairsSchedulingApi.Boundary.Requests;

    public interface IAppointmentsGateway
    {
        Task<IEnumerable<AppointmentSlot>> GetAvailableAppointments(GetAvailableAppointmentsRequest request);

        Task<string> BookAppointment(string bookingReference, string sorCode, string locationId, DateTime startDateTime, DateTime endDateTime);
    }
}
