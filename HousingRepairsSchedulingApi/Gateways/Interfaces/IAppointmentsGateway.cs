namespace HousingRepairsSchedulingApi.Gateways.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HousingRepairsSchedulingApi.Domain;
    using HousingRepairsSchedulingApi.Boundary.Requests;

    public interface IAppointmentsGateway
    {
        Task<IEnumerable<AppointmentSlot>> GetAvailableAppointments(GetAvailableAppointmentsRequest request);

        Task<string> BookAppointment(BookAppointmentRequest request);
    }
}
