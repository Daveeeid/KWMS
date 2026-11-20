namespace KWMS.Models
{
    public class CollectionSchedule
    {
        public int ScheduleID { get; set; }
        public int SubscriptionID { get; set; }
        public DateTime CollectionDate { get; set; }
        public TimeSpan CollectionTime { get; set; }
        public string? Notes { get; set; }
        public ScheduleStatus Status { get; set; } = ScheduleStatus.Scheduled;
        public DateTime CreatedAt { get; set; }


        public string ResidentName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}