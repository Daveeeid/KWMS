namespace KWMS.Models
{
    public class Service
    {
        public int ServiceID { get; set; }
        public int CompanyID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public CollectionFrequency CollectionFrequency { get; set; } = CollectionFrequency.Weekly;
        public RecordStatus Status { get; set; } = RecordStatus.Active;
        public DateTime CreatedAt { get; set; }

        public string CompanyName { get; set; } = string.Empty;
    }
}