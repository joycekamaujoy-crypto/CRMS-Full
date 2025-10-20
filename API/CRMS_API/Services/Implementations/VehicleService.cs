using CRMS_API.Services.Interfaces;
using CRMS_API.Domain.DTOs;
using CRMS_API.Domain.Data;
using Microsoft.EntityFrameworkCore;
using CRMS_API.Domain.Entities;

namespace CRMS_API.Services.Implementations
{
    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;

        public VehicleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<VehicleResponseDto?> AddVehicleAsync(CreateVehicleDto vehicleDto, int ownerId)
        {
            var owner = await _context.Users.FindAsync(ownerId);
            if(owner == null)
            {
                return null;
            }

            var newVehicle = new Vehicle
            {
                OwnerId = ownerId,
                Make = vehicleDto.Make,
                Model = vehicleDto.Model,
                Plate = vehicleDto.Plate,
                Year = vehicleDto.Year
            };

            _context.Vehicles.Add(newVehicle);
            await _context.SaveChangesAsync();

            return new VehicleResponseDto
            {
                Id = newVehicle.Id,
                Make = newVehicle.Make,
                Model = newVehicle.Model,
                Plate = newVehicle.Plate,
                Year = newVehicle.Year,
                OwnerId = newVehicle.OwnerId,
                OwnerName = owner.Name
            };
        }
        public async Task<VehicleResponseDto?> GetVehicleByIdAsync(int vehicleId)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
            if(vehicle == null)
            {
                return null;
            }

            return new VehicleResponseDto
            {
                Id = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Plate = vehicle.Plate,
                Year = vehicle.Year,
                OwnerId = vehicle.OwnerId,
                OwnerName = vehicle.Owner.Name
            };
        }
        public async Task<IEnumerable<VehicleResponseDto>> GetVehiclesByOwnerIdAsync(int ownerId)
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.OwnerId == ownerId)
                .Select(v => new VehicleResponseDto
                {
                    Id = v.Id,
                    Make = v.Make,
                    Model = v.Model,
                    Plate = v.Plate,
                    Year = v.Year,
                    OwnerId = v.OwnerId,
                    OwnerName = v.Owner.Name
                })
                .ToListAsync();
            return vehicles;
        }
        public async Task<IEnumerable<VehicleResponseDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Owner)
                .Select(v => new VehicleResponseDto
                {
                    Id = v.Id,
                    Make = v.Make,
                    Model = v.Model,
                    Plate = v.Plate,
                    Year = v.Year,
                    OwnerId = v.OwnerId,
                    OwnerName = v.Owner.Name
                }).ToListAsync();
            return vehicles;
        }
        // In VehicleService.cs
        public async Task<int> GetTotalVehicleCountAsync()
        {
            // Assuming you are using Entity Framework Core
            return await _context.Vehicles.CountAsync();
        }

        public async Task<int> GetAvailableVehicleCountAsync()
        {
            // 1. Get the list of all Vehicle IDs that are currently in an "Active" booking.
            var unavailableVehicleIds = await _context.Bookings
                .Where(b => b.Status == bookingStatus.Active)
                .Select(b => b.VehicleId)
                .Distinct()
                .ToListAsync();

            // 2. Count all vehicles whose ID is NOT in the list of unavailable vehicles.
            return await _context.Vehicles
                .CountAsync(v => !unavailableVehicleIds.Contains(v.Id));
        }
        public async Task<VehicleResponseDto?> UpdateVehicleAsync(int vehicleId, UpdateVehicleDto vehicleDto, int ownerId)
        {
            // Use .Include() to also load the related Owner data
            var vehicle = await _context.Vehicles
                .Include(v => v.Owner)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            // CRITICAL: Ensure the vehicle exists AND belongs to the user trying to edit it.
            if (vehicle == null || vehicle.OwnerId != ownerId)
            {
                return null; // Not found or unauthorized
            }

            // Map the changes
            vehicle.Make = vehicleDto.Make;
            vehicle.Model = vehicleDto.Model;
            vehicle.Year = vehicleDto.Year;

            await _context.SaveChangesAsync();

            // Now this will work correctly because the Owner is loaded
            return MapVehicleToResponseDto(vehicle);
        }
        public async Task<bool> DeleteVehicleAsync(int vehicleId, int ownerId)
        {
            // Find the vehicle, including its related bookings
            var vehicle = await _context.Vehicles
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            // 1. Security Check: Ensure the vehicle exists and belongs to this owner.
            if (vehicle == null || vehicle.OwnerId != ownerId)
            {
                // Return false if not found or if the user is not the owner.
                return false;
            }

            // 2. Business Logic Check: Prevent deletion if the car has active or upcoming bookings.
            bool hasActiveOrApprovedBookings = vehicle.Bookings.Any(b =>
                b.Status == bookingStatus.Active ||
                b.Status == bookingStatus.Approved);

            if (hasActiveOrApprovedBookings)
            {
                // The vehicle is in use or scheduled for use, so we cannot delete it.
                return false;
            }

            // 3. Proceed with deletion
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return true;
        }
        // Place this private helper method at the bottom of your VehicleService class
        private VehicleResponseDto MapVehicleToResponseDto(Vehicle vehicle)
        {
            return new VehicleResponseDto
            {
                Id = vehicle.Id,
                Plate = vehicle.Plate,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                // If the Owner was loaded, use their name. Otherwise, provide a default.
                OwnerName = vehicle.Owner?.Name ?? "N/A"
            };
        }
    }
}
