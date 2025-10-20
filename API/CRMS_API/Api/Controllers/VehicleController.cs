using Microsoft.AspNetCore.Mvc;
using CRMS_API.Services.Interfaces;
using CRMS_API.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CRMS_API.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        private int? GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }

        [HttpPost("add")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<VehicleResponseDto>> AddVehicle([FromBody] CreateVehicleDto vehicleDto)
        {
            var ownerId = GetAuthenticatedUserId();
            if (!ownerId.HasValue)
            {
                return Unauthorized(new { message = "User not identified from token" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _vehicleService.AddVehicleAsync(vehicleDto, ownerId.Value);
            if(result == null)
            {
                return StatusCode(500, new { message = "Vehicle creation failed" });
            }

            return CreatedAtAction(nameof(GetVehicleById), new { id = result.Id }, result); //201
        }

        [HttpGet("owner")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<VehicleResponseDto>>> GetOwnerVehicles()
        {
            var ownerId = GetAuthenticatedUserId();
            if (!ownerId.HasValue)
            {
                return Unauthorized(new { message = "User not identified from token" });
            }

            var vehicles = await _vehicleService.GetVehiclesByOwnerIdAsync(ownerId.Value);

            return Ok(vehicles);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<VehicleResponseDto>>> GetAllVehicles()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleResponseDto>> GetVehicleById(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound(); // 404
            }

            return Ok(vehicle);
        }

        [HttpGet("count/all")] // New endpoint for total vehicle count
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<int>> GetTotalVehicleCount()
        {
            var count = await _vehicleService.GetTotalVehicleCountAsync();
            return Ok(count);
        }

        [HttpGet("count/available")] // New endpoint for available vehicle count
        public async Task<ActionResult<int>> GetAvailableVehicleCount()
        {
            var count = await _vehicleService.GetAvailableVehicleCountAsync();
            return Ok(count);
        }
        // In your API's VehicleController.cs
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleDto vehicleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ownerId = GetAuthenticatedUserId();
            if (!ownerId.HasValue)
            {
                return Unauthorized();
            }

            var updatedVehicle = await _vehicleService.UpdateVehicleAsync(id, vehicleDto, ownerId.Value);

            if (updatedVehicle == null)
            {
                // This means the vehicle was not found OR the user is not the owner.
                // Return 404 Not Found for security to avoid revealing which is the case.
                return NotFound();
            }

            return Ok(updatedVehicle);
        }
        // In your API's VehicleController.cs
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var ownerId = GetAuthenticatedUserId();
            if (!ownerId.HasValue) return Unauthorized();

            // The service method should check for ownership before deleting
            var success = await _vehicleService.DeleteVehicleAsync(id, ownerId.Value);

            if (!success)
            {
                return NotFound(); // Vehicle not found or user is not the owner
            }

            return NoContent(); // Standard successful delete response
        }
    }
}
