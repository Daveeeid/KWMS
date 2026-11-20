namespace KWMS.Models
{

    public class CompanyStatistics
    {
        public int ActiveServices { get; set; }
        public int ActiveWorkers { get; set; }
        public int ActiveSubscribers { get; set; }
        public int TasksToAssign { get; set; }
        public int OpenTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OpenComplaints { get; set; }
    }
}