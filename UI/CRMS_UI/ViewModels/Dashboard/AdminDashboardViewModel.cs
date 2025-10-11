namespace CRMS_UI.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        public int ActiveRentals { get; set; }
        public int TotalVehicles { get; set; }
        public int PendingApprovals { get; set; }
        public int TrackingEnabledPercent { get; set; } // e.g., 100
        public string UserName { get; set; } = string.Empty;
    }
}
