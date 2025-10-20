using System.ComponentModel.DataAnnotations;

namespace CRMS_API.Domain.DTOs
{
    public class UpdateVehicleDto
    {
        [Required]
        public string Make { get; set; }

        [Required]
        public string Model { get; set; }

        [Range(1900, 2100)]
        public int Year { get; set; }
    }
}
