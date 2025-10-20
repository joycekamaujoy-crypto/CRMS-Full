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
        [Authorize] 
        public async Task<IActionResult> IngestData([FromBody] TelemetryPointDto data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _telemetryService.IngestTelemetryDataAsync(data);

            return Accepted();
        }
        [HttpGet("count/active")]
        public async Task<ActionResult<int>> GetActiveTelemetryCount()
        {
            var ownerId = GetAuthenticatedUserId();
            if (!ownerId.HasValue) return Unauthorized();

            var count = await _telemetryService.GetActiveTelemetryCountAsync(ownerId.Value);
            return Ok(count);
        }

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
