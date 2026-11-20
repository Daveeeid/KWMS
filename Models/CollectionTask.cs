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


        public string TaskType { get; set; } = string.Empty;
        public DateTime CollectionDate { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? WorkerName { get; set; }


        public bool CanBeAssigned => Status == CollectionTaskStatus.Pending || Status == CollectionTaskStatus.Assigned;
        public bool CanStart => Status == CollectionTaskStatus.Assigned && WorkerID != null;
        public bool CanComplete => Status == CollectionTaskStatus.InProgress;
        public string StatusDisplay =>
            Status == CollectionTaskStatus.InProgress ? "In progress" : Status.ToString();
    }
}