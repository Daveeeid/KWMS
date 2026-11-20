using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{

    public class ProfileControl : UserControl
    {
        private readonly ResidentController _controller = new ResidentController();

        private readonly TextBox _fullNameBox = new TextBox { MaxLength = 100 };
        private readonly TextBox _phoneBox = new TextBox { MaxLength = 20 };
        private readonly ComboBox _districtBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _sectorBox = new TextBox { MaxLength = 50 };
        private readonly TextBox _addressBox = new TextBox { MaxLength = 150 };
        private readonly NumericUpDown _householdBox = new NumericUpDown { Minimum = 1, Maximum = 50, Value = 1 };
        private readonly Panel _fields = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        private int _nextY = 10;

        public ProfileControl()
        {
            BackColor = Theme.Background;
            Padding = new Padding(25);

            _districtBox.Items.AddRange(AuthController.KigaliDistricts);

            AddRow("Full name *", _fullNameBox);
            AddRow("Phone (optional)", _phoneBox);
            AddRow("District *", _districtBox);
            AddRow("Sector *", _sectorBox);
            AddRow("House address (optional)", _addressBox);
            AddRow("Household size *", _householdBox);

            Button saveButton = new Button { Text = "Save changes" };
            saveButton.SetBounds(0, _nextY + 10, 200, 42);
            Theme.StylePrimaryButton(saveButton);
            saveButton.Click += (s, e) => Save();
            _fields.Controls.Add(saveButton);

            Label title = new Label
            {
                Text = "My profile",
                Font = Theme.TitleFont,
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 50
            };

            Controls.Add(_fields);
            Controls.Add(title);
            Load += (s, e) => LoadProfile();
        }

        private void AddRow(string label, Control input)
        {
            _fields.Controls.Add(new Label { Text = label, Left = 0, Top = _nextY, Width = 400, ForeColor = Theme.TextMuted });
            input.SetBounds(0, _nextY + 22, 400, 28);
            _fields.Controls.Add(input);
            _nextY += 58;
        }

        private void LoadProfile()
        {
            DataResult<Resident> result = _controller.GetMyProfile();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            Resident r = result.Data;
            _fullNameBox.Text = r.FullName;
            _phoneBox.Text = r.Phone;
            _districtBox.SelectedItem = r.District;
            _sectorBox.Text = r.Sector;
            _addressBox.Text = r.HouseAddress;
            _householdBox.Value = Math.Clamp(r.HouseholdSize, 1, 50);
        }

        private void Save()
        {
            Resident resident = new Resident
            {
                FullName = _fullNameBox.Text,
                Phone = _phoneBox.Text,
                District = _districtBox.SelectedItem?.ToString() ?? string.Empty,
                Sector = _sectorBox.Text,
                HouseAddress = _addressBox.Text,
                HouseholdSize = (int)_householdBox.Value
            };

            OperationResult result = _controller.UpdateProfile(resident);
            if (!result.Success) { UiHelper.ShowWarning(result.Message); return; }
            UiHelper.ShowSuccess(result.Message);
        }
    }
}