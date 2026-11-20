using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class MySubscriptionsControl : GridScreen
    {
        private readonly ResidentSubscriptionController _controller = new ResidentSubscriptionController();
        private readonly Button _cancelButton = new Button();

        public MySubscriptionsControl() : base("My subscriptions")
        {
            UiHelper.AddColumn(Grid, "Company", "CompanyName", 120);
            UiHelper.AddColumn(Grid, "Service", "ServiceName", 120);
            UiHelper.AddColumn(Grid, "Price (RWF)", "MonthlyPrice", 70, "N0");
            UiHelper.AddColumn(Grid, "Started", "StartDate", 80, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Ended", "EndDate", 80, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Status", "Status", 70);

            AddButton(_cancelButton, "Cancel subscription", 190, CancelSelected);
            UiHelper.StyleDangerButton(_cancelButton);
        }

        protected override void LoadData()
        {
            DataResult<List<Subscription>> result = _controller.GetMySubscriptions();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }
            Grid.DataSource = result.Data;
            OnSelectionChanged();
        }

        protected override void OnSelectionChanged() =>
            _cancelButton.Enabled = Selected<Subscription>()?.IsActive == true;

        private void CancelSelected()
        {
            Subscription? sub = Selected<Subscription>();
            if (sub == null) return;

            string question = $"Cancel your subscription to '{sub.ServiceName}' with {sub.CompanyName}?\n\n" +
                              "Collections that have not started will be cancelled too.";
            if (!UiHelper.Confirm(question, "Cancel subscription")) return;

            OperationResult result = _controller.CancelSubscription(sub.SubscriptionID);
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            UiHelper.ShowSuccess(result.Message);
            LoadData();
        }
    }
}