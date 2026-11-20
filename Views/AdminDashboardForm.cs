namespace KWMS.Views
{
    public class AdminDashboardForm : DashboardBase
    {
        public AdminDashboardForm() : base("System Administrator")
        {
            // Each lambda (() => new ...) creates a fresh screen every time the button is clicked,
            // so the data on it is always reloaded from the database.
            AddMenuItem("Dashboard", () => new AdminHomeControl(), showNow: true);
            AddMenuItem("Companies", () => new CompaniesControl());
            AddMenuItem("Users", () => new UsersControl());
        }
    }
}