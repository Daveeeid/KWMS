namespace KWMS.Views
{
    public class ResidentDashboardForm : DashboardBase
    {
        public ResidentDashboardForm() : base("Resident")
        {
            AddMenuItem("Dashboard", () => new ResidentHomeControl(), showNow: true);
            AddMenuItem("My Profile", () => new ProfileControl());
            AddMenuItem("Companies", () => new BrowseCompaniesControl());
            AddMenuItem("My Subscription", () => new MySubscriptionsControl());
            AddMenuItem("Schedules", () => new MySchedulesControl());
            AddMenuItem("Pickup Requests", () => new MyPickupRequestsControl());
            AddMenuItem("Payments", () => new MyPaymentsControl());
            AddMenuItem("Complaints", () => new MyComplaintsControl());
        }
    }
}