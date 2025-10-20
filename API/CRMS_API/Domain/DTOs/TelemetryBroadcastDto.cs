namespace CRMS_API.Domain.DTOs
{
    public class TelemetryBroadcastDto
    {
        public int VehicleId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public DateTime TimeStamp { get; set; }

        public string Plate { get; set; }
        public string MakeModel { get; set; }
    }
}