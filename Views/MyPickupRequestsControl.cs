using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class MyPickupRequestsControl : GridScreen
    {
        private readonly ResidentPickupController _controller = new ResidentPickupController();

        public MyPickupRequestsControl() : base("Additional pickup requests")
        {
            UiHelper.AddColumn(Grid, "Requested for", "RequestedDate", 90, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Service", "ServiceName", 110);
            UiHelper.AddColumn(Grid, "Description", "Description", 200);
            UiHelper.AddColumn(Grid, "Status", "Status", 80);
            UiHelper.AddColumn(Grid, "Sent on", "CreatedAt", 90, "dd MMM yyyy");

            AddButton(new Button(), "+ Request pickup", 180, NewRequest);
        }

        protected override void LoadData()
        {
            DataResult<List<PickupRequest>> result = _controller.GetMyRequests();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }
            Grid.DataSource = result.Data;
        }

        private void NewRequest()
        {
            using (PickupRequestForm form = new PickupRequestForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }
    }
}