using CRMS_API.Domain.DTOs;
using CRMS_API.Domain.Entities;

namespace CRMS_API.Services.Interfaces
{
    public interface IBookingService
    {
        Task<(BookingResponseDto? Booking, string Error)> RequestBookingAsync(BookingRequestDto request, int renterId);

        Task<IEnumerable<BookingResponseDto>> GetRenterBookingsAsync(int renterId);
        Task<IEnumerable<BookingResponseDto>> GetOwnerBookingsAsync(int ownerId);
        Task<int> GetTotalPendingApprovalsCountAsync();

        Task<int> GetActiveRentalsByOwnerIdCountAsync(int ownerId);

        Task<int> GetMyActiveBookingsCountAsync(int renterId);

        Task<int> GetMyPendingBookingsCountAsync(int renterId);
        Task<bool> UpdateBookingStatusAsync(int bookingId, bookingStatus newStatus, int ownerId);
    }
}
