using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class ManagerComplaintsControl : GridScreen
    {
        private readonly ComplaintController _controller = new ComplaintController();
        private readonly Button _viewButton = new Button();
        private readonly Button _statusButton = new Button();
        private readonly ComboBox _filter;

        public ManagerComplaintsControl() : base("Complaints")
        {
            UiHelper.AddColumn(Grid, "Date", "CreatedAt", 80, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Resident", "ResidentName", 110);
            UiHelper.AddColumn(Grid, "Subject", "Subject", 200);
            UiHelper.AddColumn(Grid, "Status", "StatusDisplay", 80);

            _filter = AddFilter(160, "All statuses", "Open", "In progress", "Resolved");
            AddButton(_viewButton, "View / respond", 160, ViewSelected);
            AddButton(_statusButton, "Mark resolved", 150, ChangeStatus);
            UiHelper.StyleSecondaryButton(_statusButton);
            Grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) ViewSelected(); };
        }

        protected override void LoadData()
        {
            ComplaintStatus? status = _filter.SelectedIndex switch
            {
                1 => ComplaintStatus.Open,
                2 => ComplaintStatus.InProgress,
                3 => ComplaintStatus.Resolved,
                _ => null
            };

            DataResult<List<Complaint>> result = _controller.GetComplaints(status);
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }
            Grid.DataSource = result.Data;
            OnSelectionChanged();
        }

        protected override void OnSelectionChanged()
        {
            Complaint? complaint = Selected<Complaint>();
            _viewButton.Enabled = complaint != null;
            _statusButton.Enabled = complaint != null;
            _statusButton.Text = complaint?.Status == ComplaintStatus.Resolved ? "Reopen" : "Mark resolved";
        }

        private void ViewSelected()
        {
            Complaint? complaint = Selected<Complaint>();
            if (complaint == null) return;

            using (ComplaintDetailsForm form = new ComplaintDetailsForm(complaint, _controller.GetResponses, _controller.Respond))
            {
                form.ShowDialog(this);
            }
            LoadData();   // the status may have changed to "In progress"
        }

        private void ChangeStatus()
        {
            Complaint? complaint = Selected<Complaint>();
            if (complaint == null) return;

            bool resolve = complaint.Status != ComplaintStatus.Resolved;
            string question = resolve
                ? $"Mark the complaint '{complaint.Subject}' as resolved?"
                : $"Reopen the complaint '{complaint.Subject}'?";
            if (!UiHelper.Confirm(question)) return;

            OperationResult result = _controller.SetStatus(
                complaint.ComplaintID, resolve ? ComplaintStatus.Resolved : ComplaintStatus.InProgress);

            if (!result.Success) UiHelper.ShowError(result.Message);
            LoadData();
        }
    }
}