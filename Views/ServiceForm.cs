using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    /// <summary>Add (existing == null) or edit a service.</summary>
    public class ServiceForm : FieldsDialog
    {
        private readonly ServiceController _controller = new ServiceController();
        private readonly Service? _existing;

        private readonly TextBox _nameBox = new TextBox { MaxLength = 100 };
        private readonly TextBox _descriptionBox = new TextBox { MaxLength = 255, Multiline = true };
        private readonly NumericUpDown _priceBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000000,
            Increment = 500,
            DecimalPlaces = 0,
            ThousandsSeparator = true
        };
        private readonly ComboBox _frequencyBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

        public ServiceForm(Service? existing = null)
            : base(existing == null ? "Add service" : "Edit service", 370,
                   existing == null ? "Add service" : "Save changes")
        {
            _existing = existing;

            _frequencyBox.Items.AddRange(Enum.GetNames<CollectionFrequency>());   // Weekly, Biweekly, Monthly
            _frequencyBox.SelectedIndex = 0;

            AddRow("Service name *", _nameBox);
            AddRow("Description (optional)", _descriptionBox, 60);
            AddRow("Monthly price (RWF) *", _priceBox);
            AddRow("Collection frequency *", _frequencyBox);

            if (existing != null)
            {
                _nameBox.Text = existing.ServiceName;
                _descriptionBox.Text = existing.Description;
                _priceBox.Value = Math.Min(existing.MonthlyPrice, _priceBox.Maximum);
                _frequencyBox.SelectedItem = existing.CollectionFrequency.ToString();
            }
        }

        protected override void OnSave()
        {
            Service service = new Service
            {
                ServiceID = _existing?.ServiceID ?? 0,
                ServiceName = _nameBox.Text,
                Description = _descriptionBox.Text,
                MonthlyPrice = _priceBox.Value,
                CollectionFrequency = Enum.Parse<CollectionFrequency>(_frequencyBox.SelectedItem!.ToString()!)
            };

            ShowResult(_existing == null
                ? _controller.AddService(service)
                : _controller.UpdateService(service));
        }
    }
}