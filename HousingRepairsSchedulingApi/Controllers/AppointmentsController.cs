using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using HousingRepairsSchedulingApi.Boundary.Requests;
using HousingRepairsSchedulingApi.UseCases.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sentry;
using HousingRepairsSchedulingApi.UseCases;
using Constants = HousingRepairsSchedulingApi.Constants;

namespace HousingRepairsSchedulingApi.Controllers
{
    [ApiController]
    [Route($"{Constants.ApiV1RoutePrefix}[controller]")]
    [ApiVersion("1.0")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IRetrieveAvailableAppointmentsUseCase _retrieveAvailableAppointmentsUseCase;
        private readonly IBookAppointmentUseCase _bookAppointmentUseCase;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IRetrieveAvailableAppointmentsUseCase retrieveAvailableAppointmentsUseCase,
            IBookAppointmentUseCase bookAppointmentUseCase,
            ILogger<AppointmentsController> logger)
        {
            _retrieveAvailableAppointmentsUseCase = retrieveAvailableAppointmentsUseCase;
            _bookAppointmentUseCase = bookAppointmentUseCase;
            _logger = logger;
        }

        [HttpGet]
        [Route("AvailableAppointments")]
        public async Task<IActionResult> AvailableAppointments([FromQuery] GetAvailableAppointmentsRequest request)
        {
            try
            {
                var result = await _retrieveAvailableAppointmentsUseCase.Execute(request);
                return Ok(result);
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
        public async Task<IActionResult> BookAppointment([FromQuery] BookAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation($"Appointment times (from HousingRepairsOnlineAPI) for booking reference {request.BookingReference} - start time is {request.StartDateTime} and end time is {request.EndDateTime}.");

                var result = await _bookAppointmentUseCase.Execute(request);

                return Ok(result);
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
