namespace CRMS_UI.ViewModels.Tracking
{
    public class TrackingViewModel
    {
        public int VehicleId { get; set; }
        public string Plate { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
