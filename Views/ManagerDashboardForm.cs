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
            AddMenuItem("Pickup Requests", () => new ManagerPickupRequestsControl());
            AddMenuItem("Payments", () => new ManagerPaymentsControl());
            AddMenuItem("Complaints", () => new ManagerComplaintsControl());
        }
    }
}