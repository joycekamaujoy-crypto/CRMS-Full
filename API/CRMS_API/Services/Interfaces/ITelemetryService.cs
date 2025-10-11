using CRMS_API.Domain.DTOs;

namespace CRMS_API.Services.Interfaces
{
    public interface ITelemetryService
    {
        Task<TelemetryPointDto?> IngestTelemetryDataAsync(TelemetryPointDto data);
    }
}
