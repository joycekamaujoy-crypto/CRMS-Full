namespace CRMS_UI.ViewModels.Dashboard
{
    public class UserDashboardViewModel
    {
        public string UserName { get; set; } = "Renter";

        public int AvailableCarsCount { get; set; }

        public int ActiveBookingsCount { get; set; }

        public int PendingApprovalsCount { get; set; }
    }
}