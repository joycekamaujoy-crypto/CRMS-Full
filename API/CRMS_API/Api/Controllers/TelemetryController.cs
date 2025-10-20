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
        [Authorize] // It's good practice to secure this endpoint
        public async Task<IActionResult> IngestData([FromBody] TelemetryPointDto data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // The service method now handles everything (saving and broadcasting).
            // It doesn't return a value, so we just 'await' its completion.
            await _telemetryService.IngestTelemetryDataAsync(data);

            // Return a 202 Accepted response. This is the correct HTTP status for
            // a "fire-and-forget" operation where the request is accepted
            // for processing but is not yet complete.
            return Accepted();
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
