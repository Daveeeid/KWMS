namespace KWMS.Models
{
    public class Company
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public RecordStatus Status { get; set; } = RecordStatus.Active;
        public DateTime CreatedAt { get; set; }

        public bool IsActive => Status == RecordStatus.Active;

        // Business rule 4: inactive companies cannot accept new subscriptions
        public bool CanAcceptNewSubscriptions() => IsActive;
    }
}