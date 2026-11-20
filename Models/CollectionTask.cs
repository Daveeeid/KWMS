namespace KWMS.Models
{
    public class CollectionTask
    {
        public int TaskID { get; set; }
        public int? ScheduleID { get; set; }
        public int? PickupRequestID { get; set; }
        public int? WorkerID { get; set; }
        public CollectionTaskStatus Status { get; set; } = CollectionTaskStatus.Pending;
        public DateTime? AssignedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Display only
        public string TaskType { get; set; } = string.Empty;    // "Scheduled collection" or "Additional pickup"
        public DateTime CollectionDate { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? WorkerName { get; set; }

        // Business rules
        public bool CanBeAssigned => Status == CollectionTaskStatus.Pending || Status == CollectionTaskStatus.Assigned;
        public bool CanStart => Status == CollectionTaskStatus.Assigned && WorkerID != null;   // rule 7
        public bool CanComplete => Status == CollectionTaskStatus.InProgress;                  // rule 8
        public string StatusDisplay =>
            Status == CollectionTaskStatus.InProgress ? "In progress" : Status.ToString();
    }
}