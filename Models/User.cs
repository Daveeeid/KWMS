namespace KWMS.Models
{
    public class User
    {
        public int UserID { get; set; }
        public int? CompanyID { get; set; }          // NULL for Admin and Resident
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public RecordStatus AccountStatus { get; set; } = RecordStatus.Active;
        public DateTime CreatedAt { get; set; }

        public string? CompanyName { get; set; }     // display only (from JOIN)

        public bool IsActive => AccountStatus == RecordStatus.Active;

        // Managers and workers must belong to a company. Admins and residents do not.
        public bool RequiresCompany =>
            Role == UserRole.CompanyManager || Role == UserRole.CollectionWorker;

        // Friendly role text for grids (the enum name "CompanyManager" looks unpolished on screen)
        public string RoleDisplay => Role switch
        {
            UserRole.Admin => "Administrator",
            UserRole.CompanyManager => "Company Manager",
            UserRole.CollectionWorker => "Collection Worker",
            UserRole.Resident => "Resident",
            _ => Role.ToString()
        };
    }
}