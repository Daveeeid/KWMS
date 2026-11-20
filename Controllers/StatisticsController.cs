using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class StatisticsController
    {
        private readonly StatisticsRepository _statistics = new StatisticsRepository();

        public DataResult<SystemStatistics> GetAdminStatistics()
        {
            if (!SessionManager.IsInRole(UserRole.Admin))
                return DataResult<SystemStatistics>.Fail("You do not have permission to view system statistics.");

            try
            {
                return DataResult<SystemStatistics>.Ok(_statistics.GetSystemStatistics());
            }
            catch (MySqlException)
            {
                return DataResult<SystemStatistics>.Fail("Cannot reach the database. Is MySQL running in XAMPP?");
            }
        }

        public DataResult<CompanyStatistics> GetManagerStatistics()
        {
            int? companyId = SessionManager.IsInRole(UserRole.CompanyManager) ? SessionManager.CurrentCompanyId : null;
            if (companyId == null)
                return DataResult<CompanyStatistics>.Fail("You do not have permission to view company statistics.");

            try
            {
                return DataResult<CompanyStatistics>.Ok(_statistics.GetCompanyStatistics(companyId.Value));
            }
            catch (MySqlException)
            {
                return DataResult<CompanyStatistics>.Fail("Cannot reach the database. Is MySQL running in XAMPP?");
            }
        }
    }
}