namespace HousingRepairsSchedulingApi.Controllers
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;
    using HousingRepairsSchedulingApi.UseCases.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Sentry;
    using UseCases;
    using Constants = HousingRepairsSchedulingApi.Constants;

    [ApiController]
    [Route($"{Constants.ApiV1RoutePrefix}[controller]")]
    [ApiVersion("1.0")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IRetrieveAvailableAppointmentsUseCase _retrieveAvailableAppointmentsUseCase;
        private readonly IBookAppointmentUseCase _bookAppointmentUseCase;

        public AppointmentsController(
            IRetrieveAvailableAppointmentsUseCase retrieveAvailableAppointmentsUseCase,
            IBookAppointmentUseCase bookAppointmentUseCase)
        {
            _retrieveAvailableAppointmentsUseCase = retrieveAvailableAppointmentsUseCase;
            _bookAppointmentUseCase = bookAppointmentUseCase;
        }

        [HttpGet]
        [Route("AvailableAppointments")]
        public async Task<IActionResult> AvailableAppointments(
            [FromQuery] string sorCode,
            [FromQuery] string locationId,
            [FromQuery] DateTime? fromDate = null)
        {
            try
            {
                var result = await _retrieveAvailableAppointmentsUseCase.Execute(sorCode, locationId, fromDate);
                return this.Ok(result);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);

                LambdaLogger.Log("An error was thrown when calling retrieveAvailableAppointmentsUseCase: " + JsonSerializer.Serialize(ex.Message));

                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("BookAppointment")]
        public async Task<IActionResult> BookAppointment(
            [FromQuery] string bookingReference,
            [FromQuery] string sorCode,
            [FromQuery] string locationId,
            [FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime)
        {
            try
            {
                var result = await _bookAppointmentUseCase.Execute(bookingReference, sorCode, locationId, startDateTime, endDateTime);

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);

                LambdaLogger.Log("An error was thrown when calling bookAppointmentUseCase: " + JsonSerializer.Serialize(ex.Message));

                return StatusCode(500);
            }
        }
    }
}
