using System.ComponentModel.DataAnnotations;


namespace CRMS_API.Domain.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int OwnerId { get; set;}

        [Required]
        [MaxLength(50)]
        public string Make { get; set; }

        [Required]
        [MaxLength(50)]
        public string Model { get; set; }

        [Required]
        [MaxLength(20)]
        public string Plate { get; set; }

        public int Year { get; set; }

        public User Owner { get; set; }

        public ICollection<Booking> Bookings { get; set; }
        public ICollection<TelemetryPoint> TelemetryPoints { get; set; }

    }
}
