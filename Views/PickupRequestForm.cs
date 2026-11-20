using KWMS.Controllers;

namespace KWMS.Views
{
    public class PickupRequestForm : FieldsDialog
    {
        private readonly ResidentPickupController _controller = new ResidentPickupController();

        private readonly DateTimePicker _dateBox = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            MinDate = DateTime.Today,
            Value = DateTime.Today.AddDays(1)
        };
        private readonly TextBox _descriptionBox = new TextBox { MaxLength = 255, Multiline = true };

        public PickupRequestForm() : base("Request additional pickup", 265, "Send request")
        {
            AddRow("Preferred date *", _dateBox);
            AddRow("What should be collected? *", _descriptionBox, 80);
        }

        protected override void OnSave() =>
            ShowResult(_controller.CreateRequest(_dateBox.Value, _descriptionBox.Text));
    }
}