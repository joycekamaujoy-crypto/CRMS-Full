using System.ComponentModel.DataAnnotations;
using CRMS_API.Domain.Entities;

namespace CRMS_API.Domain.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        public userRole Role { get; set; }
    }
}
