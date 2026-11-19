using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    /// <summary>Resident self-registration. Validation lives in AuthController, not here.</summary>
    public class RegisterForm : Form
    {
        private readonly AuthController _controller = new AuthController();

        private readonly TextBox _fullNameBox = new TextBox();
        private readonly TextBox _userNameBox = new TextBox();
        private readonly TextBox _emailBox = new TextBox();
        private readonly TextBox _phoneBox = new TextBox();
        private readonly ComboBox _districtBox = new ComboBox();
        private readonly TextBox _sectorBox = new TextBox();
        private readonly TextBox _addressBox = new TextBox();
        private readonly NumericUpDown _householdBox = new NumericUpDown();
        private readonly TextBox _passwordBox = new TextBox();
        private readonly TextBox _confirmBox = new TextBox();

        private readonly Panel _fieldsPanel = new Panel();
        private int _nextY = 15;

        public string RegisteredUserName { get; private set; } = string.Empty;

        public RegisterForm()
        {
            Text = "KWMS - Resident Registration";
            ClientSize = new Size(430, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.Background;
            Font = Theme.BaseFont;

            // Scrollable area for the fields
            _fieldsPanel.Dock = DockStyle.Fill;
            _fieldsPanel.AutoScroll = true;

            _districtBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _districtBox.Items.AddRange(AuthController.KigaliDistricts);   // Gasabo, Kicukiro, Nyarugenge
            _householdBox.Minimum = 1;
            _householdBox.Maximum = 50;
            _householdBox.Value = 1;
            _passwordBox.UseSystemPasswordChar = true;
            _confirmBox.UseSystemPasswordChar = true;

            _fullNameBox.MaxLength = 100;
            _userNameBox.MaxLength = 30;
            _emailBox.MaxLength = 100;
            _phoneBox.MaxLength = 20;
            _sectorBox.MaxLength = 50;
            _addressBox.MaxLength = 150;

            AddRow("Full name *", _fullNameBox);
            AddRow("Username *", _userNameBox);
            AddRow("E-mail *", _emailBox);
            AddRow("Phone (optional)", _phoneBox);
            AddRow("District *", _districtBox);
            AddRow("Sector *", _sectorBox);
            AddRow("House address (optional)", _addressBox);
            AddRow("Household size *", _householdBox);
            AddRow("Password *  (8+ characters, a letter and a digit)", _passwordBox);
            AddRow("Confirm password *", _confirmBox);

            // Buttons stay visible at the bottom
            Panel buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = Theme.Background };

            Button registerButton = new Button { Text = "Create account" };
            registerButton.SetBounds(30, 12, 200, 42);
            Theme.StylePrimaryButton(registerButton);
            registerButton.Click += RegisterButton_Click;

            Button cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            cancelButton.SetBounds(245, 12, 150, 42);

            buttonPanel.Controls.Add(registerButton);
            buttonPanel.Controls.Add(cancelButton);

            // Fill control first, then the Bottom control (so docking lays out correctly)
            Controls.Add(_fieldsPanel);
            Controls.Add(buttonPanel);
            CancelButton = cancelButton;
        }

        // Adds "label above input" at the next free vertical position
        private void AddRow(string labelText, Control input)
        {
            _fieldsPanel.Controls.Add(new Label
            {
                Text = labelText,
                Left = 30,
                Top = _nextY,
                Width = 370,
                Height = 20,
                ForeColor = Theme.TextMuted
            });

            input.Left = 30;
            input.Top = _nextY + 22;
            input.Width = 370;
            _fieldsPanel.Controls.Add(input);

            _nextY += 58;
        }

        private void RegisterButton_Click(object? sender, EventArgs e)
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

            OperationResult result = _controller.RegisterResident(
                _userNameBox.Text, _emailBox.Text, _passwordBox.Text, _confirmBox.Text, resident);

            if (!result.Success)
            {
                MessageBox.Show(result.Message, "Please fix the following",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RegisteredUserName = _userNameBox.Text.Trim();
            MessageBox.Show(result.Message, "Registration successful",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }
    }
}