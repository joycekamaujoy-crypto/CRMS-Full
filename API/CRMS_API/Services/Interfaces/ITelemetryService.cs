using CRMS_API.Domain.DTOs;

namespace CRMS_API.Services.Interfaces
{
    public interface ITelemetryService
    {
        Task IngestTelemetryDataAsync(TelemetryPointDto data);
        Task<int> GetActiveTelemetryCountAsync(int ownerId);
    }
}
