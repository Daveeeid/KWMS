namespace KWMS.Views
{
    public class ManagerDashboardForm : DashboardBase
    {
        public ManagerDashboardForm() : base("Company Manager")
        {
            AddMenuItem("Dashboard", () => new ManagerHomeControl(), showNow: true);
            AddMenuItem("Services", () => new ServicesControl());
            AddMenuItem("Workers", () => new WorkersControl());
            AddMenuItem("Residents", () => new SubscribersControl());
            AddMenuItem("Schedules", () => new SchedulesControl());
            AddMenuItem("Tasks", () => new TasksControl());
            AddMenuItem("Pickup Requests");   // Stage 8
            AddMenuItem("Payments");          // Stage 9
            AddMenuItem("Complaints");        // Stage 9
        }
    }
}