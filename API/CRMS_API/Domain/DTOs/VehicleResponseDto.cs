namespace CRMS_API.Domain.DTOs
{
    public class VehicleResponseDto
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Plate { get; set; }
        public int Year { get; set; }
        public int OwnerId { get; set; }
        // Why: Including the Owner name makes the response more useful for frontend display.
        public string OwnerName { get; set; }
    }
}
