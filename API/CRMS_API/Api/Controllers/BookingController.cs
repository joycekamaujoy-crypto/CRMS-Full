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
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IMpesaSimulation _paymentSimulation;

        public BookingController(IBookingService bookingService, IMpesaSimulation paymentSimulation)
        {
            _bookingService = bookingService;
            _paymentSimulation = paymentSimulation;
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

        [HttpPost("booking")]
        [Authorize(Roles = "Renter, Owner")]
        public async Task<ActionResult<BookingResponseDto>> RequestBooking([FromBody] BookingRequestDto request)
        {
            var renterId = GetAuthenticatedUserId();
            if (!renterId.HasValue)
            {
                return Unauthorized(new { message = "User not identified from token" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (booking, error) = await _bookingService.RequestBookingAsync(request, renterId.Value);
            if(booking == null)
            {
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(nameof(GetMyBookings), booking);
        }

        [HttpPost("simulate-mpesa")]
        [Authorize(Roles = "Renter, Owner")]
        public async Task<ActionResult<string>> SimulateMpesaPayment([FromBody] MpesaSimulationDto paymentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _paymentSimulation.SimulateMpesaPaymentAsync(paymentDto);
            return Ok(new { message = result });
        }

        [HttpPut("{id}/status")] 
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] RentalUpdateStatusDto model)
        {
            var ownerId = GetAuthenticatedUserId();
            if (!ownerId.HasValue) return Unauthorized();

            var success = await _bookingService.UpdateBookingStatusAsync(id, model.NewStatus, ownerId.Value);

            if (!success)
            {
                return NotFound(new { message = "Booking not found or update is not allowed." });
            }

            return Ok();         }

        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetMyBookings()
        {
            var renterId = GetAuthenticatedUserId();
            if (!renterId.HasValue)
            {
                return Unauthorized(new { message = "User not identified from token" });
            }

            var bookings = await _bookingService.GetRenterBookingsAsync(renterId.Value);

            return Ok(bookings);

        }

        [HttpGet("owner")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetOwnerBookings()
        {
            var ownerId = GetAuthenticatedUserId();
            if (!ownerId.HasValue)
            {
                return Unauthorized(new { message = "User not identified from token" });
            }

            var bookings = await _bookingService.GetOwnerBookingsAsync(ownerId.Value);

            return Ok(bookings);
        }

        [HttpGet("count/pending")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<int>> GetTotalPendingApprovalsCount()
        {
            var count = await _bookingService.GetTotalPendingApprovalsCountAsync();
            return Ok(count);
        }

        [HttpGet("count/owner/active")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<int>> GetActiveRentalsByOwnerCount()
        {
            var ownerId = GetAuthenticatedUserId();
            if (!ownerId.HasValue) return Unauthorized();

            var count = await _bookingService.GetActiveRentalsByOwnerIdCountAsync(ownerId.Value);
            return Ok(count);
        }

        [HttpGet("count/my-active")]
        [Authorize(Roles = "Renter")]
        public async Task<ActionResult<int>> GetMyActiveBookingsCount()
        {
            var renterId = GetAuthenticatedUserId();
            if (!renterId.HasValue) return Unauthorized();

            var count = await _bookingService.GetMyActiveBookingsCountAsync(renterId.Value);
            return Ok(count);
        }

        [HttpGet("count/my-pending")]
        [Authorize(Roles = "Renter")]
        public async Task<ActionResult<int>> GetMyPendingBookingsCount()
        {
            var renterId = GetAuthenticatedUserId();
            if (!renterId.HasValue) return Unauthorized();

            var count = await _bookingService.GetMyPendingBookingsCountAsync(renterId.Value);
            return Ok(count);
        }
    }
}
