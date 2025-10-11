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
                VehicleMakeModel = $"{vehicle.Make} {vehicle.Model}",
                VehiclePlate = vehicle.Plate,
                RenterId = newBooking.RenterId,
                RenterName = renter!.Name,
                StartDate = newBooking.StartDate,
                EndDate = newBooking.EndDate,
                //TotalPrice = newBooking.TotalPrice,
                Status = newBooking.Status
            }, string.Empty);
        }
        public async Task<BookingResponseDto?> ApproveBookingAsync(int bookingId, int ownerId)
        {
            return await ChangeBookingStatus(bookingId, ownerId, bookingStatus.Approved);
        }

        public async Task<BookingResponseDto?> RejectBookingAsync(int bookingId, int ownerId)
        {
            return await ChangeBookingStatus(bookingId, ownerId, bookingStatus.Cancelled);
        }
        private async Task<BookingResponseDto?> ChangeBookingStatus(int bookingId, int ownerId,bookingStatus newStatus)
        {
            var booking = await _context.Bookings
                .Include(b => b.Vehicle)
                    .ThenInclude(v => v.Owner)
                .Include(b => b.Renter)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
                return null;
            /* if (booking.Vehicle.OwnerId != authorizedUserId)
             {
                 return null;
             } */
            if (booking.Status != bookingStatus.Pending)
                return null;

            booking.Status = newStatus;
            await _context.SaveChangesAsync();

            return new BookingResponseDto
            {
                Id = booking.Id,
                VehicleId = booking.VehicleId,
                VehicleMakeModel = $"{booking.Vehicle.Make} {booking.Vehicle.Model}",
                VehiclePlate = booking.Vehicle.Plate,
                RenterId = booking.RenterId,
                RenterName = booking.Renter.Name,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                //TotalPrice = booking.TotalPrice,
                Status = booking.Status
            };    
        }

        public async Task<IEnumerable<BookingResponseDto>> GetRenterBookingsAsync(int renterId)
        {
            return await _context.Bookings
                .Where(b => b.RenterId == renterId)
                .Include(b => b.Vehicle)
                .Include(b => b.Renter)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    VehicleId = b.VehicleId,
                    VehicleMakeModel = $"{b.Vehicle.Make} {b.Vehicle.Model}",
                    VehiclePlate = b.Vehicle.Plate,
                    RenterId = b.RenterId,
                    RenterName = b.Renter.Name,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                   // TotalPrice = b.TotalPrice,
                    Status = b.Status
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<BookingResponseDto>> GetOwnerBookingsAsync(int ownerId)
        {
            return await _context.Bookings
                .Include(b => b.Vehicle)
                .Where(b => b.Vehicle.OwnerId == ownerId) // Filter by the owner of the vehicle
                .Include(b => b.Renter)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    VehicleId = b.VehicleId,
                    VehicleMakeModel = $"{b.Vehicle.Make} {b.Vehicle.Model}",
                    VehiclePlate = b.Vehicle.Plate,
                    RenterId = b.RenterId,
                    RenterName = b.Renter.Name,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                   // TotalPrice = b.TotalPrice,
                    Status = b.Status
                })
                .ToListAsync();
        }
    }
}
