using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class ScheduleForm : FieldsDialog
    {
        private readonly ScheduleController _controller = new ScheduleController();

        private readonly ComboBox _subscriptionBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _dateBox = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            MinDate = DateTime.Today,
            Value = DateTime.Today.AddDays(1)
        };
        private readonly DateTimePicker _timeBox = new DateTimePicker
        {
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Value = DateTime.Today.AddHours(8)
        };
        private readonly TextBox _notesBox = new TextBox { MaxLength = 255, Multiline = true };

        public ScheduleForm() : base("New collection schedule", 360, "Create schedule")
        {
            AddRow("Resident (active subscribers) *", _subscriptionBox);
            AddRow("Collection date *", _dateBox);
            AddRow("Collection time *", _timeBox);
            AddRow("Notes (optional)", _notesBox, 60);

            // Fill the drop-down. Set the Member names BEFORE the DataSource.
            DataResult<List<Subscription>> result = new SubscriptionController().GetSubscribers(null, activeOnly: true);

            if (!result.Success)
            {
                UiHelper.ShowError(result.Message);
                return;
            }

            _subscriptionBox.DisplayMember = "DisplayText";
            _subscriptionBox.ValueMember = "SubscriptionID";
            _subscriptionBox.DataSource = result.Data;
        }

        protected override void OnSave()
        {
            if (_subscriptionBox.SelectedValue == null)
            {
                UiHelper.ShowWarning("There is no resident to schedule. A resident must first subscribe to one of your services.");
                return;
            }

            int subscriptionId = Convert.ToInt32(_subscriptionBox.SelectedValue);
            TimeSpan time = new TimeSpan(_timeBox.Value.Hour, _timeBox.Value.Minute, 0);

            ShowResult(_controller.CreateSchedule(subscriptionId, _dateBox.Value, time, _notesBox.Text));
        }
    }
}