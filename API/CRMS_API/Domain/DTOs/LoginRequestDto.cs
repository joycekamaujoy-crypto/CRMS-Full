using System.ComponentModel.DataAnnotations;

namespace CRMS_API.Domain.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

    }
}
