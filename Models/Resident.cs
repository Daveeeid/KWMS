namespace KWMS.Models
{
    public class Resident
    {
        public int ResidentID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string District { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string? HouseAddress { get; set; }
        public int HouseholdSize { get; set; } = 1;
        public DateTime CreatedAt { get; set; }
    }
}