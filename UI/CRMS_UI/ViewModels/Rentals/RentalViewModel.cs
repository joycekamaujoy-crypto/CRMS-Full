namespace CRMS_UI.ViewModels.Rentals
{
    public enum BookingStatus
    {
        Pending = 0,
        Approved = 1,
        Active = 2,
        Completed = 3,
        Cancelled = 4
    }

    // Why: Model for displaying booking details in lists and history
    public class RentalViewModel
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Plate { get; set; } = string.Empty;
        public string MakeModel { get; set; } = string.Empty;
        public string RenterName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BookingStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();

        // Why: Helper for calculating the total price (simulated for PoC)
        public decimal TotalCost => CalculateCost();

        private decimal CalculateCost()
        {
            // Simple calculation: 50 USD per day (PoC simulation)
            var days = (EndDate - StartDate).TotalDays;
            return (decimal)Math.Round(days * 50.0, 2);
        }
    }
}
