namespace KWMS.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int SubscriptionID { get; set; }
        public int RecordedByUserID { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        // Display only
        public string ResidentName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string RecordedByName { get; set; } = string.Empty;

        public string MethodDisplay => PaymentMethod switch
        {
            PaymentMethod.MobileMoney => "Mobile Money",
            PaymentMethod.BankTransfer => "Bank Transfer",
            _ => "Cash"
        };
    }
}