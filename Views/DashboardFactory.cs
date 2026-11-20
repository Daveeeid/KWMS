using KWMS.Models;

namespace KWMS.Views
{

    public static class DashboardFactory
    {
        public static Form Create(User user) => user.Role switch
        {
            UserRole.Admin => new AdminDashboardForm(),
            UserRole.CompanyManager => new ManagerDashboardForm(),
            UserRole.CollectionWorker => new WorkerDashboardForm(),
            UserRole.Resident => new ResidentDashboardForm(),
            _ => throw new ArgumentOutOfRangeException(nameof(user), "Unknown role.")
        };
    }
}