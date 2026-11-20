using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class ManagerPickupRequestsControl : GridScreen
    {
        private readonly PickupRequestController _controller = new PickupRequestController();
        private readonly Button _approveButton = new Button();
        private readonly Button _rejectButton = new Button();
        private readonly ComboBox _filter;

        public ManagerPickupRequestsControl() : base("Additional pickup requests")
        {
            UiHelper.AddColumn(Grid, "Requested for", "RequestedDate", 80, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Resident", "ResidentName", 100);
            UiHelper.AddColumn(Grid, "Location", "Location", 140);
            UiHelper.AddColumn(Grid, "Description", "Description", 170);
            UiHelper.AddColumn(Grid, "Status", "Status", 70);

            _filter = AddFilter(150, "Pending", "Approved", "Rejected", "Completed", "All statuses");
            AddButton(_approveButton, "Approve", 110, Approve);
            AddButton(_rejectButton, "Reject", 110, Reject);
            UiHelper.StyleDangerButton(_rejectButton);
        }

        protected override void LoadData()
        {
            PickupRequestStatus? status = _filter.SelectedIndex switch
            {
                0 => PickupRequestStatus.Pending,
                1 => PickupRequestStatus.Approved,
                2 => PickupRequestStatus.Rejected,
                3 => PickupRequestStatus.Completed,
                _ => null
            };

            DataResult<List<PickupRequest>> result = _controller.GetRequests(status);
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }
            Grid.DataSource = result.Data;
            OnSelectionChanged();
        }

        protected override void OnSelectionChanged()
        {
            bool pending = Selected<PickupRequest>()?.Status == PickupRequestStatus.Pending;
            _approveButton.Enabled = pending;
            _rejectButton.Enabled = pending;
        }

        private void Approve() => Act(r => _controller.Approve(r.RequestID),
            r => $"Approve the pickup request from {r.ResidentName}?\n\nA collection task will be created for you to assign.");

        private void Reject() => Act(r => _controller.Reject(r.RequestID),
            r => $"Reject the pickup request from {r.ResidentName}?");


        private void Act(Func<PickupRequest, OperationResult> action, Func<PickupRequest, string> question)
        {
            PickupRequest? request = Selected<PickupRequest>();
            if (request == null) return;
            if (!UiHelper.Confirm(question(request))) return;

            OperationResult result = action(request);
            if (!result.Success) UiHelper.ShowError(result.Message);
            else UiHelper.ShowSuccess(result.Message);
            LoadData();
        }
    }
}