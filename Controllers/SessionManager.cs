using KWMS.Models;

namespace KWMS.Controllers
{





    public static class SessionManager
    {
        public static User? CurrentUser { get; private set; }

        public static bool IsLoggedIn => CurrentUser != null;
        public static int? CurrentCompanyId => CurrentUser?.CompanyID;

        public static void Start(User user)
        {
            user.PasswordHash = string.Empty;
            CurrentUser = user;
        }

        public static void End() => CurrentUser = null;

        public static bool IsInRole(UserRole role) => CurrentUser?.Role == role;
    }
}