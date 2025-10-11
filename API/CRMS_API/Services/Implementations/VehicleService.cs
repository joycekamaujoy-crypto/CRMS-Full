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
    }
}
