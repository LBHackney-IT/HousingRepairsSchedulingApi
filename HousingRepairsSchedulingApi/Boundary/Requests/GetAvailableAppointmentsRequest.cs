namespace HousingRepairsSchedulingApi.Boundary.Requests
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    public class GetAvailableAppointmentsRequest
    {
        [FromQuery(Name = "sorCode")]
        public string SorCode { get; set; }

        [FromQuery(Name = "locationId")]
        public string LocationId { get; set; }

        [FromQuery(Name = "fromDate")]
        public DateTime? FromDate { get; set; } = null;
    }
}
