namespace KWMS.Models
{

    public class WorkerStatistics
    {
        public int ToStart { get; set; }
        public int InProgress { get; set; }
        public int CompletedToday { get; set; }
        public int CompletedTotal { get; set; }
    }
}