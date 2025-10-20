using CRMS_API.Services.Interfaces;
using CRMS_API.Domain.DTOs;


namespace CRMS_API.Services.Background
{
    public class GpsSimulatorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly List<(double Lat, double Lng)> _routeWaypoints = new List<(double Lat, double Lng)>
        {
                        (-1.286389, 36.817223),
                        (-1.166600, 36.950000),
                        (-1.033333, 37.066666),
                        (-1.166600, 36.950000),
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

        private const int STEPS_PER_SEGMENT = 30;
        private readonly List<int> _trackedVehicleIds = new List<int> { 1 };
        private readonly Dictionary<int, VehicleState> _vehicleStates = new Dictionary<int, VehicleState>();

        public GpsSimulatorService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            foreach (var id in _trackedVehicleIds)
            {
                _vehicleStates.Add(id, new VehicleState
                {
                    Lat = _routeWaypoints[0].Lat,
                    Lng = _routeWaypoints[0].Lng,
                    Speed = 60.0
                });
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await SimulateAndIngestData();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task SimulateAndIngestData()
        {
            using var scope = _scopeFactory.CreateScope();

            var telemetryService = scope.ServiceProvider.GetRequiredService<ITelemetryService>();

            foreach (var vehicleId in _trackedVehicleIds)
            {
                var state = _vehicleStates[vehicleId];

                int startIndex = (state.TargetWaypointIndex - 1 + _routeWaypoints.Count) % _routeWaypoints.Count;
                var startPoint = _routeWaypoints[startIndex];
                var endPoint = _routeWaypoints[state.TargetWaypointIndex % _routeWaypoints.Count];

                double interpolationFactor = (double)state.CurrentStep / STEPS_PER_SEGMENT;

                double newLat = startPoint.Lat + (endPoint.Lat - startPoint.Lat) * interpolationFactor;
                double newLng = startPoint.Lng + (endPoint.Lng - startPoint.Lng) * interpolationFactor;

                state.Lat = newLat;
                state.Lng = newLng;
                state.CurrentStep++;

                if (state.CurrentStep > STEPS_PER_SEGMENT)
                {
                    state.Lat = endPoint.Lat;
                    state.Lng = endPoint.Lng;

                    state.TargetWaypointIndex = (state.TargetWaypointIndex + 1) % _routeWaypoints.Count;
                    if (state.TargetWaypointIndex == 0) state.TargetWaypointIndex = 1;
                    state.CurrentStep = 1;
                }

                var telemetryData = new TelemetryPointDto
                {
                    VehicleId = vehicleId,
                    Latitude = state.Lat,
                    Longitude = state.Lng,
                    Speed = state.Speed + (new Random().NextDouble() * 10) - 5,
                    TimeStamp = DateTime.UtcNow
                };

                await telemetryService.IngestTelemetryDataAsync(telemetryData);
            }
        }
    }
}
