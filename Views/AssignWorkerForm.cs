using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class AssignWorkerForm : FieldsDialog
    {
        private readonly TaskController _controller = new TaskController();
        private readonly CollectionTask _task;

        private readonly TextBox _summaryBox = new TextBox { ReadOnly = true };
        private readonly ComboBox _workerBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

        public AssignWorkerForm(CollectionTask task) : base("Assign worker", 215, "Assign")
        {
            _task = task;

            _summaryBox.Text = $"{task.CollectionDate:dd MMM yyyy} - {task.ResidentName} ({task.TaskType})";

            AddRow("Task", _summaryBox);
            AddRow("Active worker *", _workerBox);

            DataResult<List<Worker>> result = _controller.GetAssignableWorkers();

            if (!result.Success)
            {
                UiHelper.ShowError(result.Message);
                return;
            }

            _workerBox.DisplayMember = "FullName";
            _workerBox.ValueMember = "WorkerID";
            _workerBox.DataSource = result.Data;
        }

        protected override void OnSave()
        {
            if (_workerBox.SelectedValue == null)
            {
                UiHelper.ShowWarning("There is no active worker. Add or activate a worker first.");
                return;
            }

            int workerId = Convert.ToInt32(_workerBox.SelectedValue);
            ShowResult(_controller.AssignWorker(_task.TaskID, workerId));
        }
    }
}