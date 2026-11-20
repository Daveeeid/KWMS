using KWMS.Models;

namespace KWMS.Controllers
{




    public abstract class ManagerControllerBase
    {
        protected const string NoPermission = "You do not have permission to perform this action.";
        protected const string DatabaseDown = "Cannot reach the database. Is MySQL running in XAMPP?";


        protected static int? CompanyId =>
            SessionManager.IsInRole(UserRole.CompanyManager) ? SessionManager.CurrentCompanyId : null;
    }
}