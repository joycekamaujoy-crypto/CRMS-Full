using System.ComponentModel.DataAnnotations;

namespace CRMS_API.Domain.DTOs
{
    public class TelemetryPointDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        [Range(-90.0, 90.0)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180.0, 180.0)]
        public double Longitude { get; set; }

        [Required]
        [Range(0, 500)]
        public double Speed { get; set; }

        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
