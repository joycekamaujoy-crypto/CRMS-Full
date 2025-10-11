using Microsoft.AspNetCore.Mvc;
using CRMS_API.Services.Interfaces;
using CRMS_API.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CRMS_API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryService _telemetryService;

        public TelemetryController(ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
        }

        [HttpPost("ingest")]
        public async Task<ActionResult<TelemetryPointDto>> IngestData([FromBody] TelemetryPointDto data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _telemetryService.IngestTelemetryDataAsync(data);
            if (result == null)
            {
                return BadRequest(new { message = "Telemetry Ingestion Failed" });
            }

            return Accepted(result); //202
        }
    }
}
