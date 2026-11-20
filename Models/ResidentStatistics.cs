namespace KWMS.Models
{
    public class ResidentStatistics
    {
        public string SubscriptionText { get; set; } = "No active subscription";
        public int UpcomingCollections { get; set; }
        public int PendingRequests { get; set; }
        public int OpenComplaints { get; set; }
        public int PaymentsMade { get; set; }
    }
}