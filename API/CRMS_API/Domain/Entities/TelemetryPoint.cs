namespace CRMS_API.Domain.Entities
{
    public class TelemetryPoint
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime TimeStamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}
