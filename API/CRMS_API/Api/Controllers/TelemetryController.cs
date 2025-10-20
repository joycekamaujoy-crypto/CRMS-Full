using Microsoft.AspNetCore.Mvc;
using CRMS_API.Services.Interfaces;
using CRMS_API.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
        [HttpGet("count/active")]
        public async Task<ActionResult<int>> GetActiveTelemetryCount()
        {
            var ownerId = GetAuthenticatedUserId(); // You'll need to add the GetAuthenticatedUserId helper here too
            if (!ownerId.HasValue) return Unauthorized();

            var count = await _telemetryService.GetActiveTelemetryCountAsync(ownerId.Value);
            return Ok(count);
        }

        // Don't forget to add the helper method from your other controllers if it's not here
        private int? GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
