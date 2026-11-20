namespace KWMS.Views
{
    public class WorkerDashboardForm : DashboardBase
    {
        public WorkerDashboardForm() : base("Collection Worker")
        {
            AddMenuItem("Dashboard", () => new WorkerHomeControl(), showNow: true);
            AddMenuItem("My Tasks", () => new MyTasksControl());
        }
    }
}