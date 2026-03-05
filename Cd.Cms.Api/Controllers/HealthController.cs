using Cd.Cms.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cd.Cms.Api.Controllers
{
    [ApiController]
    [Route("api/v1/health")]
    public sealed class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Check()
        {
            var data = new
            {
                status = "Healthy",
                timestampUtc = DateTime.UtcNow
            };

            return Ok(ApiResponse<object>.Success("Service is healthy.", data));
        }
    }
}
