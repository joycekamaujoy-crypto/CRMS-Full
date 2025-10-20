using CRMS_API.Services.Interfaces;
using CRMS_API.Domain.DTOs;
using CRMS_API.Domain.Entities;
using CRMS_API.Domain.Data;
using Microsoft.AspNetCore.SignalR;
using CRMS_API.Api.Hubs;
using Microsoft.EntityFrameworkCore;

namespace CRMS_API.Services.Implementations
{
    public class TelemetryService : ITelemetryService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<TelemetryHub> _hubContext;

        public TelemetryService(AppDbContext context, IHubContext<TelemetryHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<TelemetryPointDto?> IngestTelemetryDataAsync(TelemetryPointDto data)
        {
            var newPoint = new TelemetryPoint
            {
                VehicleId = data.VehicleId,
                Latitude = data.Latitude,
                Longitude = data.Longitude,
                Speed = data.Speed,
                TimeStamp = data.TimeStamp
            };

            _context.TelemetryPoints.Add(newPoint);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("RecieveTelemetryUpdate", data);

            return data;
        }
        public async Task<int> GetActiveTelemetryCountAsync(int ownerId)
        {
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);

            // This query counts the unique vehicles that belong to the owner
            // and have sent a telemetry point recently.
            return await _context.TelemetryPoints
                .Where(tp => tp.Vehicle.OwnerId == ownerId && tp.TimeStamp >= fiveMinutesAgo)
                .Select(tp => tp.VehicleId) // Select only the vehicle IDs
                .Distinct()                // Get only the unique ones
                .CountAsync();             // Count them
        }
    }
}
