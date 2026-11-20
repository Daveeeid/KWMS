using KWMS.Data.Repositories;
using KWMS.Models;

namespace KWMS.Controllers
{
    public abstract class ResidentControllerBase
    {
        protected const string NoPermission = "You do not have permission to perform this action.";
        protected const string DatabaseDown = "Cannot reach the database. Is MySQL running in XAMPP?";

        private readonly ResidentRepository _residentRepository = new ResidentRepository();

        protected Resident? GetCurrentResident(out string error)
        {
            error = string.Empty;
            User? user = SessionManager.CurrentUser;

            if (user == null || user.Role != UserRole.Resident)
            {
                error = NoPermission;
                return null;
            }

            Resident? resident = _residentRepository.GetByUserId(user.UserID);
            if (resident == null)
                error = "Your account has no resident profile. Please contact the administrator.";
            return resident;
        }
    }
}