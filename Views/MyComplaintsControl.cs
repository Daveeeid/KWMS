using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class MyComplaintsControl : GridScreen
    {
        private readonly ResidentComplaintController _controller = new ResidentComplaintController();
        private readonly Button _viewButton = new Button();

        public MyComplaintsControl() : base("My complaints")
        {
            UiHelper.AddColumn(Grid, "Date", "CreatedAt", 80, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Company", "CompanyName", 110);
            UiHelper.AddColumn(Grid, "Subject", "Subject", 180);
            UiHelper.AddColumn(Grid, "Status", "StatusDisplay", 80);

            AddButton(new Button(), "+ New complaint", 170, NewComplaint);
            AddButton(_viewButton, "View responses", 160, ViewSelected);
            UiHelper.StyleSecondaryButton(_viewButton);
            Grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) ViewSelected(); };
        }

        protected override void LoadData()
        {
            DataResult<List<Complaint>> result = _controller.GetMyComplaints();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }
            Grid.DataSource = result.Data;
            OnSelectionChanged();
        }

        protected override void OnSelectionChanged() => _viewButton.Enabled = Selected<Complaint>() != null;

        private void NewComplaint()
        {
            using (ComplaintForm form = new ComplaintForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }

        private void ViewSelected()
        {
            Complaint? complaint = Selected<Complaint>();
            if (complaint == null) return;

            using (ComplaintDetailsForm form = new ComplaintDetailsForm(complaint, _controller.GetResponses))
            {
                form.ShowDialog(this);
            }
        }
    }
}