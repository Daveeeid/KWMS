using KWMS.Models;

namespace KWMS.Controllers
{
    /// <summary>
    /// Remembers who is logged in while the app runs. In later stages every controller reads
    /// the role and CompanyID from here, NEVER from a text box. That is what makes
    /// "a manager can only access their own company" (rule 5) trustworthy.
    /// </summary>
    public static class SessionManager
    {
        public static User? CurrentUser { get; private set; }

        public static bool IsLoggedIn => CurrentUser != null;
        public static int? CurrentCompanyId => CurrentUser?.CompanyID;

        public static void Start(User user)
        {
            user.PasswordHash = string.Empty;   // the hash is not needed after login
            CurrentUser = user;
        }

        public static void End() => CurrentUser = null;

        public static bool IsInRole(UserRole role) => CurrentUser?.Role == role;
    }
}