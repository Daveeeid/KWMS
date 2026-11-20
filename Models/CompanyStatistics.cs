namespace KWMS.Models
{
    /// <summary>The numbers on the company manager's dashboard cards.</summary>
    public class CompanyStatistics
    {
        public int ActiveServices { get; set; }
        public int ActiveWorkers { get; set; }
        public int ActiveSubscribers { get; set; }
        public int TasksToAssign { get; set; }     // Pending
        public int OpenTasks { get; set; }         // Assigned + InProgress
        public int CompletedTasks { get; set; }
        public int OpenComplaints { get; set; }
    }
}