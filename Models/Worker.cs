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

        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;


        public bool CanReceiveTasks => Status == RecordStatus.Active;
    }
}