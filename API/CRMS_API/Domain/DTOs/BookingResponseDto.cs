using CRMS_API.Domain.Entities; // For the enum

namespace CRMS_API.Domain.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bookingStatus Status { get; set; }

        // Vehicle Details
        public string Plate { get; set; }
        public string MakeModel { get; set; }

        // Renter Details
        public string RenterName { get; set; }

        // You might also want to include IDs for linking
        public int VehicleId { get; set; }
        public int RenterId { get; set; }
    }
}