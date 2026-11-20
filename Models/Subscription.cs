namespace KWMS.Models
{
    public class Subscription
    {
        public int SubscriptionID { get; set; }
        public int ResidentID { get; set; }
        public int ServiceID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        public DateTime CreatedAt { get; set; }


        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public string? ResidentPhone { get; set; }
        public string ResidentLocation { get; set; } = string.Empty;

        public bool IsActive => Status == SubscriptionStatus.Active;

        public string DisplayText => $"{ResidentName} - {ServiceName}";

        public string PaymentDisplayText => $"{ResidentName} - {ServiceName} ({Status}, since {StartDate:dd MMM yyyy})";
    }
}