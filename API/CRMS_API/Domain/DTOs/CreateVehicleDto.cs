using System.ComponentModel.DataAnnotations;

namespace CRMS_API.Domain.DTOs
{
    public class CreateVehicleDto
    {
        [Required]
        [MaxLength(50)]
        public string Make { get; set; }

        [Required]
        [MaxLength(50)]
        public string Model { get; set; }

        [Required]
        [MaxLength(20)]
        public string Plate { get; set; }

        [Range(1900, 2100)]
        public int Year { get; set; }
    }
}
