namespace HousingRepairsSchedulingApi.Tests
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;

    public class ControllerTests
    {

        protected static T GetResultData<T>(IActionResult result) => (T)(result as ObjectResult)?.Value;

        protected static int? GetStatusCode(IActionResult result) => (result as IStatusCodeActionResult).StatusCode;
    }
}
