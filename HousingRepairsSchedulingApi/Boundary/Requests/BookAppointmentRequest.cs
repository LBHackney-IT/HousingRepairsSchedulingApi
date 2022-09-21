namespace HousingRepairsSchedulingApi.Boundary.Requests
{
    using System;
    using Microsoft.AspNetCore.Mvc;

    public class BookAppointmentRequest
    {
        [FromQuery(Name = "bookingReference")]
        public string BookingReference { get; set; }

        [FromQuery(Name = "sorCode")]
        public string SorCode { get; set; }

        [FromQuery(Name = "locationId")]
        public string LocationId { get; set; }

        [FromQuery(Name = "startDateTime")]
        public DateTime StartDateTime { get; set; }

        [FromQuery(Name = "endDateTime")]
        public DateTime EndDateTime { get; set; }
    }
}
