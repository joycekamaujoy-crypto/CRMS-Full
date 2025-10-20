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

        public async Task IngestTelemetryDataAsync(TelemetryPointDto data)
        {
            // 2. Look up the vehicle to get its details
            var vehicle = await _context.Vehicles
                .AsNoTracking() // Performance improvement, as we don't need to track changes
                .FirstOrDefaultAsync(v => v.Id == data.VehicleId);

            if (vehicle == null) return; // Don't proceed if the vehicle doesn't exist

            // 3. Create the enriched broadcast object
            var broadcastData = new TelemetryBroadcastDto
            {
                VehicleId = data.VehicleId,
                Latitude = data.Latitude,
                Longitude = data.Longitude,
                Speed = data.Speed,
                TimeStamp = data.TimeStamp,
                Plate = vehicle.Plate,
                MakeModel = $"{vehicle.Make} {vehicle.Model}"
            };

            // 4. Broadcast the enriched data to all connected clients
            // The method name "ReceiveTelemetryUpdate" MUST match your JavaScript client's .on() listener
            await _hubContext.Clients.All.SendAsync("ReceiveTelemetryUpdate", broadcastData);

            // (Optional) Save the raw telemetry point to the database
            var telemetryEntity = new TelemetryPoint
            {
                VehicleId = data.VehicleId,
                Latitude = data.Latitude,
                Longitude = data.Longitude,
                Speed = data.Speed,
                TimeStamp = data.TimeStamp
            };
            _context.TelemetryPoints.Add(telemetryEntity);
            await _context.SaveChangesAsync();
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
