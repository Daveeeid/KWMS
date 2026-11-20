namespace KWMS.Models
{
    public class ComplaintResponse
    {
        public int ResponseID { get; set; }
        public int ComplaintID { get; set; }
        public int ManagerUserID { get; set; }
        public string ResponseText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string ManagerName { get; set; } = string.Empty;
    }
}