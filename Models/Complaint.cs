namespace KWMS.Models
{
    public class Complaint
    {
        public int ComplaintID { get; set; }
        public int ResidentID { get; set; }
        public int CompanyID { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;
        public DateTime CreatedAt { get; set; }


        public string ResidentName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        public string StatusDisplay => Status == ComplaintStatus.InProgress ? "In progress" : Status.ToString();
    }
}