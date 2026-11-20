namespace KWMS.Models
{
    /// <summary>The numbers shown on the administrator's dashboard cards.</summary>
    public class SystemStatistics
    {
        public int TotalCompanies { get; set; }
        public int ActiveCompanies { get; set; }
        public int TotalUsers { get; set; }
        public int TotalResidents { get; set; }
        public int TotalWorkers { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int CompletedCollections { get; set; }
    }
}