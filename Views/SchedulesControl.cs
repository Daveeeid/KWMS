using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class SchedulesControl : GridScreen
    {
        private readonly ScheduleController _controller = new ScheduleController();
        private readonly Button _cancelButton = new Button();

        public SchedulesControl() : base("Collection schedules")
        {
            UiHelper.AddColumn(Grid, "Date", "CollectionDate", 80, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Time", "CollectionTime", 50, "hh\\:mm");
            UiHelper.AddColumn(Grid, "Resident", "ResidentName", 110);
            UiHelper.AddColumn(Grid, "Location", "Location", 150);
            UiHelper.AddColumn(Grid, "Service", "ServiceName", 90);
            UiHelper.AddColumn(Grid, "Status", "Status", 70);
            UiHelper.AddColumn(Grid, "Notes", "Notes", 110);

            AddButton(new Button(), "+ New schedule", 160, AddSchedule);
            AddButton(_cancelButton, "Cancel schedule", 150, CancelSelected);
            UiHelper.StyleDangerButton(_cancelButton);
        }

        protected override void LoadData()
        {
            DataResult<List<CollectionSchedule>> result = _controller.GetSchedules();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            Grid.DataSource = result.Data;
            OnSelectionChanged();
        }

        protected override void OnSelectionChanged()
        {
            CollectionSchedule? schedule = Selected<CollectionSchedule>();
            _cancelButton.Enabled = schedule != null && schedule.Status == ScheduleStatus.Scheduled;
        }

        private void AddSchedule()
        {
            using (ScheduleForm form = new ScheduleForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }

        private void CancelSelected()
        {
            CollectionSchedule? schedule = Selected<CollectionSchedule>();
            if (schedule == null) return;

            string question = $"Cancel the collection for {schedule.ResidentName} on {schedule.CollectionDate:dd MMM yyyy}?\n\n" +
                              "Its task will be cancelled too.";
            if (!UiHelper.Confirm(question)) return;

            OperationResult result = _controller.CancelSchedule(schedule.ScheduleID);
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            UiHelper.ShowSuccess(result.Message);
            LoadData();
        }
    }
}