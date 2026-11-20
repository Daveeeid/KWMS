using KWMS.Models;

namespace KWMS.Controllers
{
    /// <summary>
    /// Shared by every controller the Company Manager uses. CompanyId comes from the login
    /// session (rule 5), never from a text box or a parameter the View could change.
    /// </summary>
    public abstract class ManagerControllerBase
    {
        protected const string NoPermission = "You do not have permission to perform this action.";
        protected const string DatabaseDown = "Cannot reach the database. Is MySQL running in XAMPP?";

        // null means "not a company manager"
        protected static int? CompanyId =>
            SessionManager.IsInRole(UserRole.CompanyManager) ? SessionManager.CurrentCompanyId : null;
    }
}