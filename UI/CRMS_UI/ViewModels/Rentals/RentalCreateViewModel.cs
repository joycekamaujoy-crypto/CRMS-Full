using System.ComponentModel.DataAnnotations;

namespace CRMS_UI.ViewModels.Rentals
{
    public class RentalCreateViewModel
    {
        public int VehicleId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(15)]
        [Display(Name = "M-Pesa Reference")]
        public string MpesaReference { get; set; } = string.Empty;
    }
}
