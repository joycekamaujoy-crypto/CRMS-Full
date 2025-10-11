using CRMS_API.Services.Interfaces;
using CRMS_API.Domain.DTOs;


namespace CRMS_API.Services.Background
{
    public class GpsSimulatorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly List<(double Lat, double Lng)> _routeWaypoints = new List<(double Lat, double Lng)>
        {
            // 1. Nairobi CBD (Start/End)
            (-1.286389, 36.817223),
            // 2. Ruiru (Mid-point North)
            (-1.166600, 36.950000),
            // 3. Thika Town (Turnaround Point)
            (-1.033333, 37.066666),
            // 4. Ruiru (Mid-point South, on return trip)
            (-1.166600, 36.950000),
            // 5. Nairobi CBD (Complete Loop)
            (-1.286389, 36.817223)
        };

        private class VehicleState
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
            public double Speed { get; set; } = 60.0; 
            public int TargetWaypointIndex { get; set; } = 1;
            public int CurrentStep { get; set; } = 0;
        }

        // --- SIMULATION PARAMETERS ---
        private const int STEPS_PER_SEGMENT = 30; // Number of 5-second intervals to travel between two major waypoints.
                                                  // (30 steps * 5 seconds = 150 seconds or 2.5 minutes per segment)
                                                  // Total route (4 segments) will take about 10 minutes.

        private readonly List<int> _trackedVehicleIds = new List<int> { 1 };
        private readonly Dictionary<int, VehicleState> _vehicleStates = new Dictionary<int, VehicleState>();

        public GpsSimulatorService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            // Initialize vehicle states based on the first waypoint (Nairobi CBD)
            foreach (var id in _trackedVehicleIds)
            {
                _vehicleStates.Add(id, new VehicleState
                {
                    Lat = _routeWaypoints[0].Lat,
                    Lng = _routeWaypoints[0].Lng,
                    Speed = 60.0 // Start at cruising speed
                });
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            // The core loop that repeats every 5 seconds (interval from previous design)
            while (!stoppingToken.IsCancellationRequested)
            {
                await SimulateAndIngestData();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task SimulateAndIngestData()
        {
            // Create a new scope for the duration of this operation.
            using var scope = _scopeFactory.CreateScope();

            // Resolve the ITelemetryService from the created scope
            var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();

            foreach (var vehicleId in _trackedVehicleIds)
            {
                var state = _vehicleStates[vehicleId];

                // Determine the start and end points for the current segment
                int startIndex = (state.TargetWaypointIndex - 1 + _routeWaypoints.Count) % _routeWaypoints.Count;
                var startPoint = _routeWaypoints[startIndex];
                var endPoint = _routeWaypoints[state.TargetWaypointIndex % _routeWaypoints.Count];

                // 1. Calculate step factors
                // The fraction of the segment distance covered in this step (e.g., 1/30)
                double interpolationFactor = (double)state.CurrentStep / STEPS_PER_SEGMENT;

                // 2. Linear Interpolation (L.I.R.P.) to find the new position
                double newLat = startPoint.Lat + (endPoint.Lat - startPoint.Lat) * interpolationFactor;
                double newLng = startPoint.Lng + (endPoint.Lng - startPoint.Lng) * interpolationFactor;

                // 3. Update State
                state.Lat = newLat;
                state.Lng = newLng;
                state.CurrentStep++;

                // 4. Check if we reached the end of the segment
                if (state.CurrentStep > STEPS_PER_SEGMENT)
                {
                    // Snap to the final waypoint position to ensure accuracy
                    state.Lat = endPoint.Lat;
                    state.Lng = endPoint.Lng;

                    // Move to the next target waypoint (looping back to index 1 after index 4)
                    state.TargetWaypointIndex = (state.TargetWaypointIndex + 1) % _routeWaypoints.Count;
                    if (state.TargetWaypointIndex == 0) state.TargetWaypointIndex = 1; // Skip index 0 as target until the loop closes

                    state.CurrentStep = 1; // Start the next segment (CurrentStep is 1-based index)
                }

                // 5. Ingest and Broadcast Data
                var telemetryData = new TelemetryPointDto
                {
                    VehicleId = vehicleId,
                    Latitude = state.Lat,
                    Longitude = state.Lng,
                    // Simulate a slight speed variation around 60 km/h
                    Speed = state.Speed + (new Random().NextDouble() * 10) - 5,
                    TimeStamp = DateTime.UtcNow
                };

                // Call the ITelemetryService to save and broadcast
                await telemetryService.IngestTelemetryDataAsync(telemetryData);
            }
        }
    }
}
