using CRMS_API.Domain.Entities;

namespace CRMS_API.Domain.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehicleMakeModel { get; set; }
        public string VehiclePlate { get; set; }
        public int RenterId { get; set; }
        public string RenterName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public bookingStatus Status { get; set; }
    }
}
