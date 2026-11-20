namespace KWMS.Models
{
    /// <summary>The numbers on the collection worker's dashboard cards.</summary>
    public class WorkerStatistics
    {
        public int ToStart { get; set; }          // Assigned, not started yet
        public int InProgress { get; set; }
        public int CompletedToday { get; set; }
        public int CompletedTotal { get; set; }
    }
}