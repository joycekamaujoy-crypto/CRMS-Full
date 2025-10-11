using CRMS_API.Domain.DTOs;

namespace CRMS_API.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<VehicleResponseDto?> AddVehicleAsync(CreateVehicleDto vehicleDto, int ownerId);
        Task<VehicleResponseDto?> GetVehicleByIdAsync(int vehicleId);
        Task<IEnumerable<VehicleResponseDto>> GetVehiclesByOwnerIdAsync(int ownerId);
        Task<IEnumerable<VehicleResponseDto>> GetAllVehiclesAsync();

    }
}
