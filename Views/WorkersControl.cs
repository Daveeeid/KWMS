using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class WorkersControl : GridScreen
    {
        private readonly WorkerController _controller = new WorkerController();

        private readonly Button _editButton = new Button();
        private readonly Button _toggleButton = new Button();
        private readonly TextBox _searchBox;

        public WorkersControl() : base("Workers")
        {
            UiHelper.AddColumn(Grid, "Full name", "FullName", 130);
            UiHelper.AddColumn(Grid, "Username", "UserName", 100);
            UiHelper.AddColumn(Grid, "E-mail", "Email", 140);
            UiHelper.AddColumn(Grid, "Phone", "Phone", 90);
            UiHelper.AddColumn(Grid, "Status", "Status", 60);

            _searchBox = AddSearchBox("Search name or username");
            AddButton(new Button(), "+ Add worker", 150, AddWorker);
            AddButton(_editButton, "Edit", 100, EditSelected);
            UiHelper.StyleSecondaryButton(_editButton);
            AddButton(_toggleButton, "Deactivate", 130, ToggleStatus);

            Grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditSelected(); };
        }

        protected override void LoadData()
        {
            DataResult<List<Worker>> result = _controller.GetWorkers(_searchBox.Text);
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            Grid.DataSource = result.Data;
            OnSelectionChanged();
        }

        protected override void OnSelectionChanged()
        {
            Worker? worker = Selected<Worker>();
            _editButton.Enabled = worker != null;
            _toggleButton.Enabled = worker != null;
            StyleToggle(_toggleButton, worker == null || worker.CanReceiveTasks);
        }

        private void AddWorker()
        {
            using (WorkerForm form = new WorkerForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }

        private void EditSelected()
        {
            Worker? worker = Selected<Worker>();
            if (worker == null) { UiHelper.ShowWarning("Please select a worker first.", "No worker selected"); return; }

            using (WorkerForm form = new WorkerForm(worker))
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }

        private void ToggleStatus()
        {
            Worker? worker = Selected<Worker>();
            if (worker == null) return;

            bool deactivate = worker.CanReceiveTasks;
            string question = deactivate
                ? $"Deactivate '{worker.FullName}'?\n\nThey will no longer be able to log in or receive tasks."
                : $"Activate '{worker.FullName}' again?\n\nThey will be able to log in and receive tasks.";

            if (!UiHelper.Confirm(question)) return;

            OperationResult result = _controller.SetWorkerStatus(
                worker.WorkerID, deactivate ? RecordStatus.Inactive : RecordStatus.Active);

            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            UiHelper.ShowSuccess(result.Message);
            LoadData();
        }
    }
}