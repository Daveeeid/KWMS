namespace KWMS.Models
{
    public class PickupRequest
    {
        public int RequestID { get; set; }
        public int SubscriptionID { get; set; }
        public DateTime RequestedDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public PickupRequestStatus Status { get; set; } = PickupRequestStatus.Pending;
        public DateTime CreatedAt { get; set; }

        // Display only
        public string ResidentName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}