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
    }
}
