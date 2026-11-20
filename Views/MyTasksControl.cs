using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    /// <summary>The worker's own tasks, with Start and Complete actions.</summary>
    public class MyTasksControl : GridScreen
    {
        private readonly WorkerTaskController _controller = new WorkerTaskController();

        private readonly Button _startButton = new Button();
        private readonly Button _completeButton = new Button();
        private readonly ComboBox _filter;

        public MyTasksControl() : base("My tasks")
        {
            UiHelper.AddColumn(Grid, "Date", "CollectionDate", 80, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Type", "TaskType", 110);
            UiHelper.AddColumn(Grid, "Resident", "ResidentName", 100);
            UiHelper.AddColumn(Grid, "Location", "Location", 160);
            UiHelper.AddColumn(Grid, "Service", "ServiceName", 80);
            UiHelper.AddColumn(Grid, "Status", "StatusDisplay", 80);

            // The order of these items matches the MyTaskFilter enum order used in LoadData
            _filter = AddFilter(230, "To do (assigned / in progress)", "Completed", "Cancelled", "All my tasks");

            AddButton(_startButton, "Start collection", 160, StartSelected);
            AddButton(_completeButton, "Mark completed", 160, CompleteSelected);

            Button refreshButton = new Button { Text = "Refresh", Size = new Size(100, 36) };
            UiHelper.StyleSecondaryButton(refreshButton);
            refreshButton.Click += (s, e) => LoadData();
            Toolbar.Controls.Add(refreshButton);
        }

        protected override void LoadData()
        {
            MyTaskFilter filter = (MyTaskFilter)_filter.SelectedIndex;

            DataResult<List<CollectionTask>> result = _controller.GetMyTasks(filter);
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            Grid.DataSource = result.Data;
            OnSelectionChanged();
        }

        // The buttons are enabled only when the rule allows the action (the controller checks again)
        protected override void OnSelectionChanged()
        {
            CollectionTask? task = Selected<CollectionTask>();
            _startButton.Enabled = task != null && task.CanStart;         // rule 7
            _completeButton.Enabled = task != null && task.CanComplete;   // rule 8
        }

        private void StartSelected()
        {
            CollectionTask? task = Selected<CollectionTask>();
            if (task == null) return;

            string question = $"Start the collection for {task.ResidentName}?\n\n{task.Location}\n{task.CollectionDate:dd MMM yyyy}";
            if (!UiHelper.Confirm(question, "Start collection")) return;

            OperationResult result = _controller.StartTask(task.TaskID);
            if (!result.Success) { UiHelper.ShowError(result.Message); LoadData(); return; }

            UiHelper.ShowSuccess(result.Message);
            LoadData();
        }

        private void CompleteSelected()
        {
            CollectionTask? task = Selected<CollectionTask>();
            if (task == null) return;

            string question = $"Mark the collection for {task.ResidentName} as completed?\n\nThis cannot be undone.";
            if (!UiHelper.Confirm(question, "Complete collection")) return;

            OperationResult result = _controller.CompleteTask(task.TaskID);
            if (!result.Success) { UiHelper.ShowError(result.Message); LoadData(); return; }

            UiHelper.ShowSuccess(result.Message);
            LoadData();
        }
    }
}