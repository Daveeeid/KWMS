using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{

    public class SubscribersControl : GridScreen
    {
        private readonly SubscriptionController _controller = new SubscriptionController();
        private readonly TextBox _searchBox;
        private readonly ComboBox _filter;

        public SubscribersControl() : base("Residents")
        {
            UiHelper.AddColumn(Grid, "Resident", "ResidentName", 110);
            UiHelper.AddColumn(Grid, "Phone", "ResidentPhone", 80);
            UiHelper.AddColumn(Grid, "Location", "ResidentLocation", 150);
            UiHelper.AddColumn(Grid, "Service", "ServiceName", 100);
            UiHelper.AddColumn(Grid, "Price (RWF)", "MonthlyPrice", 70, "N0");
            UiHelper.AddColumn(Grid, "Since", "StartDate", 80, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Status", "Status", 60);

            _searchBox = AddSearchBox("Search resident, service or location");
            _filter = AddFilter(190, "Active subscriptions", "All (including past)");
        }

        protected override void LoadData()
        {
            DataResult<List<Subscription>> result =
                _controller.GetSubscribers(_searchBox.Text, activeOnly: _filter.SelectedIndex == 0);

            if (!result.Success) { UiHelper.ShowError(result.Message); return; }
            Grid.DataSource = result.Data;
        }
    }
}