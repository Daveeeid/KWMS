using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{

    public class PaymentForm : FieldsDialog
    {
        private readonly PaymentController _controller = new PaymentController();

        private readonly ComboBox _subscriptionBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly NumericUpDown _amountBox = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 10000000,
            Increment = 500,
            DecimalPlaces = 0,
            ThousandsSeparator = true,
            Value = 5000
        };
        private readonly ComboBox _methodBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DateTimePicker _dateBox = new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            MaxDate = DateTime.Today,
            Value = DateTime.Today
        };
        private readonly TextBox _referenceBox = new TextBox { MaxLength = 50 };
        private readonly TextBox _notesBox = new TextBox { MaxLength = 255, Multiline = true };

        public PaymentForm() : base("Record payment", 480, "Record payment")
        {

            _methodBox.Items.AddRange(new object[] { "Cash", "Mobile Money", "Bank Transfer" });
            _methodBox.SelectedIndex = 0;

            AddRow("Resident and service *", _subscriptionBox);
            AddRow("Amount received (RWF) *", _amountBox);
            AddRow("Payment method *", _methodBox);
            AddRow("Payment date *", _dateBox);
            AddRow("Reference number (required unless cash)", _referenceBox);
            AddRow("Notes (optional)", _notesBox, 60);

            DataResult<List<Subscription>> result = _controller.GetSubscriptions();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            _subscriptionBox.DisplayMember = "PaymentDisplayText";
            _subscriptionBox.ValueMember = "SubscriptionID";
            _subscriptionBox.DataSource = result.Data;


            _subscriptionBox.SelectedIndexChanged += (s, e) => SuggestAmount();
            SuggestAmount();
        }

        private void SuggestAmount()
        {
            if (_subscriptionBox.SelectedItem is Subscription sub)
                _amountBox.Value = Math.Clamp(sub.MonthlyPrice, _amountBox.Minimum, _amountBox.Maximum);
        }

        protected override void OnSave()
        {
            if (_subscriptionBox.SelectedValue == null)
            {
                UiHelper.ShowWarning("There is no subscription to record a payment for.");
                return;
            }

            ShowResult(_controller.RecordPayment(
                Convert.ToInt32(_subscriptionBox.SelectedValue), _amountBox.Value,
                (PaymentMethod)_methodBox.SelectedIndex, _dateBox.Value,
                _referenceBox.Text, _notesBox.Text));
        }
    }
}