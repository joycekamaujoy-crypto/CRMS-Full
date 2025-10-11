using System.ComponentModel.DataAnnotations;

namespace CRMS_API.Domain.DTOs
{
    public class MpesaSimulationDto
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        [RegularExpression(@"^254\d{9}$", ErrorMessage = "Phone number must be in the format 2547XXXXXXXX.")]
        public string PhoneNumber { get; set; }

        public int TransactionRefId { get; set; }
    }
}
