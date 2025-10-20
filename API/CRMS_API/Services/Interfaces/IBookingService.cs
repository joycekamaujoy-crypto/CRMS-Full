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

        // For Admin: Get count of all currently active rentals for their vehicles
        Task<int> GetActiveRentalsByOwnerIdCountAsync(int ownerId);

        // For User: Get count of their own active bookings
        Task<int> GetMyActiveBookingsCountAsync(int renterId);

        // For User: Get count of their own bookings that are pending approval
        Task<int> GetMyPendingBookingsCountAsync(int renterId);
        Task<bool> UpdateBookingStatusAsync(int bookingId, bookingStatus newStatus, int ownerId);
    }
}
