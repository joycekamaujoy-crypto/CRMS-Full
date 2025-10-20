using CRMS_API.Services.Interfaces;
using CRMS_API.Domain.DTOs;
using CRMS_API.Domain.Entities;
using CRMS_API.Domain.Data;
using Microsoft.EntityFrameworkCore;


namespace CRMS_API.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        private async Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime start, DateTime end)
        {
            var isOverlapping = await _context.Bookings.AnyAsync(b =>
                b.VehicleId == vehicleId &&
                (b.Status == bookingStatus.Approved || b.Status == bookingStatus.Pending || b.Status == bookingStatus.Active) &&
                b.StartDate < end &&
                b.EndDate > start
            );
            return !isOverlapping;
        }

        //add a way for owners to change renting status to completed!!

        public async Task<(BookingResponseDto? Booking, string Error)> RequestBookingAsync(BookingRequestDto request, int renterId)
        {
            if (request.StartDate >= request.EndDate || request.StartDate < DateTime.Today)
            {
                return (null, "Invalid dates. Start date must be before end date and not in the past");
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(v => v.Id == request.VehicleId);
            if(vehicle == null)
                return (null, "Vehicle not found");
            if (vehicle.OwnerId == renterId)
                return (null, "You cannot rent your own vehicle");
            if(!await IsVehicleAvailableAsync(request.VehicleId, request.StartDate, request.EndDate))
            {
                return (null, "Vehicle is not available for the requested dates");
            }

            var newBooking = new Booking
            {
                VehicleId = request.VehicleId,
                RenterId = renterId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = bookingStatus.Pending
               // TotalPrice = request.TotalPrice
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();

            var renter = await _context.Users.FindAsync(renterId);
            return (new BookingResponseDto
            {
                Id = newBooking.Id,
                VehicleId = newBooking.VehicleId,
                MakeModel = $"{vehicle.Make} {vehicle.Model}",
                Plate = vehicle.Plate,
                RenterId = newBooking.RenterId,
                RenterName = renter!.Name,
                StartDate = newBooking.StartDate,
                EndDate = newBooking.EndDate,
                //TotalPrice = newBooking.TotalPrice,
                Status = newBooking.Status
            }, string.Empty);
        }
        public async Task<bool> UpdateBookingStatusAsync(int bookingId, bookingStatus newStatus, int ownerId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            // Security check: Booking must exist AND belong to the owner making the request.
            if (booking == null || booking.Vehicle.OwnerId != ownerId)
            {
                return false; // Not found or unauthorized
            }

            // Business Logic: Add any rules here, e.g., can't cancel an active booking.
            if (booking.Status == bookingStatus.Active && newStatus == bookingStatus.Cancelled)
            {
                // Example rule: Prevent cancelling a rental that's already in progress.
                return false;
            }

            booking.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BookingResponseDto>> GetRenterBookingsAsync(int renterId)
        {
            var bookingsFromDb = await _context.Bookings
                    .Include(b => b.Vehicle)
                    .Include(b => b.Renter)
                    .Where(b => b.RenterId == renterId)
                    .OrderByDescending(b => b.StartDate)
                    .ToListAsync(); // This runs the SQL query

            // 2. Map the results in memory using standard LINQ to Objects.
            //    This is now safe because all the data is loaded.
            return bookingsFromDb.Select(b => MapBookingToResponseDto(b));
        }
        public async Task<IEnumerable<BookingResponseDto>> GetOwnerBookingsAsync(int ownerId)
        {
            var bookingsFromDb = await _context.Bookings
                    .Include(b => b.Vehicle)
                    .Include(b => b.Renter)
                    .Where(b => b.Vehicle.OwnerId == ownerId)
                    .OrderByDescending(b => b.StartDate)
                    .ToListAsync(); // This runs the SQL query

            // 2. Map the results in memory.
            return bookingsFromDb.Select(b => MapBookingToResponseDto(b));
        }
        public async Task<int> GetTotalPendingApprovalsCountAsync()
        {
            // Counts all bookings in the system with the status "Pending"
            return await _context.Bookings
                .CountAsync(b => b.Status == bookingStatus.Pending);
        }

        public async Task<int> GetActiveRentalsByOwnerIdCountAsync(int ownerId)
        {
            var now = DateTime.UtcNow;

            // Counts bookings that are approved, ongoing, AND belong to a vehicle owned by the specified ownerId
            return await _context.Bookings
                .CountAsync(b => b.Vehicle.OwnerId == ownerId &&
                                 b.Status == bookingStatus.Approved &&
                                 b.StartDate <= now &&
                                 b.EndDate >= now);
        }

        public async Task<int> GetMyActiveBookingsCountAsync(int renterId)
        {
            var now = DateTime.UtcNow; // Get the current time once

            // An active booking is one that has been approved and is currently ongoing.
            return await _context.Bookings
                .CountAsync(b => b.RenterId == renterId &&
                                 b.Status == bookingStatus.Approved && // It must have been approved
                                 b.StartDate <= now &&                  // The rental period has started
                                 b.EndDate >= now);                     // The rental period has not ended
        }

        public async Task<int> GetMyPendingBookingsCountAsync(int renterId)
        {
            // Counts bookings for the specific renterId that are "Pending" approval
            return await _context.Bookings
                .CountAsync(b => b.RenterId == renterId && b.Status == bookingStatus.Pending);
        }
        private BookingResponseDto MapBookingToResponseDto(Booking booking)
        {
            return new BookingResponseDto
            {
                Id = booking.Id,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                Status = booking.Status,
                Plate = booking.Vehicle?.Plate ?? "N/A",
                MakeModel = $"{booking.Vehicle?.Make} {booking.Vehicle?.Model}",
                RenterName = booking.Renter?.Name ?? "N/A",
                VehicleId = booking.VehicleId,
                RenterId = booking.RenterId
            };
        }
    }
}
