namespace KWMS.Views
{
    public class AdminDashboardForm : DashboardBase
    {
        public AdminDashboardForm() : base("System Administrator")
        {


            AddMenuItem("Dashboard", () => new AdminHomeControl(), showNow: true);
            AddMenuItem("Companies", () => new CompaniesControl());
            AddMenuItem("Users", () => new UsersControl());
        }
    }
}