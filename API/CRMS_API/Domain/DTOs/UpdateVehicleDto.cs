using System.ComponentModel.DataAnnotations;

namespace CRMS_API.Domain.DTOs
{
    // In CRMS_API/Domain/DTOs/UpdateVehicleDto.cs
    public class UpdateVehicleDto
    {
        // Only include fields that are allowed to be changed.
        [Required]
        public string Make { get; set; }

        [Required]
        public string Model { get; set; }

        [Range(1900, 2100)]
        public int Year { get; set; }
    }
}
