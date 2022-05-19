namespace HousingRepairsSchedulingApi.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Sentry;
    using UseCases;
    using Constants = Constants;

    [ApiController]
    [Route($"{Constants.ApiV1RoutePrefix}[controller]")]
    [ApiVersion("1.0")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IRetrieveAvailableAppointmentsUseCase retrieveAvailableAppointmentsUseCase;
        private readonly IBookAppointmentUseCase bookAppointmentUseCase;

        public AppointmentsController(IRetrieveAvailableAppointmentsUseCase retrieveAvailableAppointmentsUseCase,
            IBookAppointmentUseCase bookAppointmentUseCase)
        {
            this.retrieveAvailableAppointmentsUseCase = retrieveAvailableAppointmentsUseCase;
            this.bookAppointmentUseCase = bookAppointmentUseCase;
        }

        [HttpGet]
        [Route("AvailableAppointments")]
        public async Task<IActionResult> AvailableAppointments([FromQuery] string sorCode, [FromQuery] string locationId, [FromQuery] DateTime? fromDate = null)
        {
            try
            {
                var result = await this.retrieveAvailableAppointmentsUseCase.Execute(sorCode, locationId, fromDate);
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return this.StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("BookAppointment")]
        public async Task<IActionResult> BookAppointment([FromQuery] string bookingReference,
            [FromQuery] string sorCode,
            [FromQuery] string locationId,
            [FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime)
        {
            try
            {
                var result = await this.bookAppointmentUseCase.Execute(bookingReference, sorCode, locationId, startDateTime, endDateTime);

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return this.StatusCode(500, ex.Message);
            }
        }
    }
}
