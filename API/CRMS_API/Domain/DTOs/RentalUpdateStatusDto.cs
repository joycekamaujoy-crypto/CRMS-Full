using CRMS_API.Domain.Entities;

namespace CRMS_API.Domain.DTOs
{
    public class RentalUpdateStatusDto
    {
        public int RentalId { get; set; }
        public bookingStatus NewStatus { get; set; }
    }
}
