using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class TasksControl : GridScreen
    {
        private readonly TaskController _controller = new TaskController();
        private readonly Button _assignButton = new Button();
        private readonly ComboBox _filter;

        public TasksControl() : base("Collection tasks")
        {
            UiHelper.AddColumn(Grid, "Date", "CollectionDate", 80, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Type", "TaskType", 110);
            UiHelper.AddColumn(Grid, "Resident", "ResidentName", 100);
            UiHelper.AddColumn(Grid, "Location", "Location", 150);
            UiHelper.AddColumn(Grid, "Service", "ServiceName", 80);
            UiHelper.AddColumn(Grid, "Worker", "WorkerName", 100);
            UiHelper.AddColumn(Grid, "Status", "Status", 70);

            _filter = AddFilter(160, "All statuses", "Pending", "Assigned", "InProgress", "Completed", "Cancelled");
            AddButton(_assignButton, "Assign worker", 150, AssignSelected);
        }

        protected override void LoadData()
        {
            CollectionTaskStatus? status = _filter.SelectedIndex == 0
                ? null
                : Enum.Parse<CollectionTaskStatus>(_filter.SelectedItem!.ToString()!);

            DataResult<List<CollectionTask>> result = _controller.GetTasks(status);
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            Grid.DataSource = result.Data;
            OnSelectionChanged();
        }

        protected override void OnSelectionChanged()
        {
            CollectionTask? task = Selected<CollectionTask>();
            _assignButton.Enabled = task != null && task.CanBeAssigned;   // Pending or Assigned only
            _assignButton.Text = task != null && task.Status == CollectionTaskStatus.Assigned
                ? "Change worker" : "Assign worker";
        }

        private void AssignSelected()
        {
            CollectionTask? task = Selected<CollectionTask>();
            if (task == null) return;

            using (AssignWorkerForm form = new AssignWorkerForm(task))
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }
    }
}