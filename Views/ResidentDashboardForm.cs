namespace KWMS.Views
{
    public class ResidentDashboardForm : DashboardBase
    {
        public ResidentDashboardForm() : base("Resident")
        {
            AddMenuItem("Dashboard");
            AddMenuItem("My Profile");
            AddMenuItem("Companies");
            AddMenuItem("My Subscription");
            AddMenuItem("Schedules");
            AddMenuItem("Pickup Requests");
            AddMenuItem("Payments");
            AddMenuItem("Complaints");
            ShowDescription("Stages 8 and 9 add: your profile, subscriptions, pickup requests, payment history and complaints.");
        }
    }
}