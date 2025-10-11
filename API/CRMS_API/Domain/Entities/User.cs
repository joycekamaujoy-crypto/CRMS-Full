using System.ComponentModel.DataAnnotations;

namespace CRMS_API.Domain.Entities
{
    public enum userRole
    {
        Renter = 1,
        Owner = 2
    }
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public userRole Role { get; set; }

        public ICollection<Booking> RentedBookings { get; set; }
        public ICollection<Vehicle> OwnedVehicles { get; set; }
    }
}
