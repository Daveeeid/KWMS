namespace KWMS.Models
{
    public class Worker
    {
        public int WorkerID { get; set; }
        public int UserID { get; set; }
        public int CompanyID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public RecordStatus Status { get; set; } = RecordStatus.Active;
        public DateTime CreatedAt { get; set; }

        public string UserName { get; set; } = string.Empty;   // display only
        public string Email { get; set; } = string.Empty;      // display only

        // Business rule 13: inactive workers cannot receive new tasks
        public bool CanReceiveTasks => Status == RecordStatus.Active;
    }
}