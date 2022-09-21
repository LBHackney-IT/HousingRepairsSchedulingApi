namespace HousingRepairsSchedulingApi.Tests.GatewaysTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain;
    using HousingRepairsSchedulingApi.Boundary.Requests;
    using HousingRepairsSchedulingApi.Gateways.Interfaces;

    public class DummyAppointmentsGateway : IAppointmentsGateway
    {
        public async Task<IEnumerable<AppointmentSlot>> GetAvailableAppointments(GetAvailableAppointmentsRequest request)
        {
            var dateTime = (request.FromDate ?? DateTime.Today).Date;

            List<AppointmentSlot> unorderedAppointments = new List<AppointmentSlot>
            {
                new ()
                {
                    StartTime = dateTime.AddDays(16).AddHours(8),
                    EndTime= dateTime.AddDays(16).AddHours(12)
                },
                new()
                {
                    StartTime = dateTime.AddDays(20).AddDays(2).AddHours(12),
                    EndTime = dateTime.AddDays(20).AddDays(2).AddHours(16)
                },
                new()
                {
                    StartTime = dateTime.AddDays(7).AddDays(7).AddHours(8),
                    EndTime = dateTime.AddDays(7).AddDays(7).AddHours(12)
                },
                new ()
                {
                    StartTime = dateTime.AddDays(1).AddDays(1).AddHours(8),
                    EndTime = dateTime.AddDays(1).AddDays(1).AddHours(12)
                },
                new ()
                {
                    StartTime = dateTime.AddDays(5).AddDays(5).AddHours(12),
                    EndTime = dateTime.AddDays(5).AddDays(5).AddHours(16)
                }
            };
            var orderedAppointments = unorderedAppointments.OrderBy(x => x.StartTime);
            return orderedAppointments;
        }

        public Task<string> BookAppointment(BookAppointmentRequest request) =>
            throw new NotImplementedException();
    }
}
