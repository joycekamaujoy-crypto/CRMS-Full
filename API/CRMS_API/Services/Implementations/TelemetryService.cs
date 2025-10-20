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
            var vehicle = await _context.Vehicles
                .AsNoTracking() 
                .FirstOrDefaultAsync(v => v.Id == data.VehicleId);

            if (vehicle == null) return;

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

            await _hubContext.Clients.All.SendAsync("ReceiveTelemetryUpdate", broadcastData);

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

            return await _context.TelemetryPoints
                .Where(tp => tp.Vehicle.OwnerId == ownerId && tp.TimeStamp >= fiveMinutesAgo)
                .Select(tp => tp.VehicleId) 
                .Distinct()                
                .CountAsync();             
        }
    }
}
