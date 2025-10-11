using CRMS_API.Domain.DTOs;

namespace CRMS_API.Services.Interfaces
{
    public interface IBookingService
    {
        Task<(BookingResponseDto? Booking, string Error)> RequestBookingAsync(BookingRequestDto request, int renterId);
        Task<BookingResponseDto?> ApproveBookingAsync(int bookingId, int ownerId);
        Task<BookingResponseDto?> RejectBookingAsync(int bookingId, int ownerId);
        Task<IEnumerable<BookingResponseDto>> GetRenterBookingsAsync(int renterId);
        Task<IEnumerable<BookingResponseDto>> GetOwnerBookingsAsync(int ownerId);
    }
}
