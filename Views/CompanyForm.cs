using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{




    public class CompanyForm : Form
    {
        private readonly CompanyController _controller = new CompanyController();
        private readonly Company? _existing;

        private readonly TextBox _nameBox = new TextBox();
        private readonly TextBox _registrationBox = new TextBox();
        private readonly TextBox _phoneBox = new TextBox();
        private readonly TextBox _addressBox = new TextBox();
        private readonly TextBox _managerUserNameBox = new TextBox();
        private readonly TextBox _managerEmailBox = new TextBox();
        private readonly TextBox _managerPasswordBox = new TextBox();
        private readonly TextBox _managerConfirmBox = new TextBox();

        private readonly Panel _fieldsPanel = new Panel();
        private int _nextY = 15;

        public CompanyForm(Company? existing = null)
        {
            _existing = existing;
            bool isEdit = existing != null;

            Text = isEdit ? "Edit company" : "Register new company";
            ClientSize = new Size(430, isEdit ? 340 : 620);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.Background;
            Font = Theme.BaseFont;

            _fieldsPanel.Dock = DockStyle.Fill;
            _fieldsPanel.AutoScroll = true;

            _nameBox.MaxLength = 100;
            _registrationBox.MaxLength = 50;
            _phoneBox.MaxLength = 20;
            _addressBox.MaxLength = 150;
            _managerUserNameBox.MaxLength = 30;
            _managerEmailBox.MaxLength = 100;
            _managerPasswordBox.UseSystemPasswordChar = true;
            _managerConfirmBox.UseSystemPasswordChar = true;

            AddRow("Company name *", _nameBox);
            AddRow("Registration number *", _registrationBox);
            AddRow("Phone (optional)", _phoneBox);
            AddRow("Address (optional)", _addressBox);

            if (existing != null)
            {
                _nameBox.Text = existing.CompanyName;
                _registrationBox.Text = existing.RegistrationNumber;
                _phoneBox.Text = existing.Phone;
                _addressBox.Text = existing.Address;
            }
            else
            {
                AddHeading("Company manager account (first login)");
                AddRow("Manager username *", _managerUserNameBox);
                AddRow("Manager e-mail *", _managerEmailBox);
                AddRow("Password *  (8+ characters, a letter and a digit)", _managerPasswordBox);
                AddRow("Confirm password *", _managerConfirmBox);
            }


            Panel buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = Theme.Background };

            Button saveButton = new Button { Text = isEdit ? "Save changes" : "Register company" };
            saveButton.SetBounds(30, 12, 200, 42);
            Theme.StylePrimaryButton(saveButton);
            saveButton.Click += SaveButton_Click;

            Button cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            cancelButton.SetBounds(245, 12, 150, 42);

            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(cancelButton);

            Controls.Add(_fieldsPanel);
            Controls.Add(buttonPanel);
            CancelButton = cancelButton;
        }

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

            input.SetBounds(30, _nextY + 22, 370, 28);
            _fieldsPanel.Controls.Add(input);
            _nextY += 58;
        }

        private void AddHeading(string text)
        {
            _fieldsPanel.Controls.Add(new Label
            {
                Text = text,
                Font = Theme.BoldFont,
                ForeColor = Theme.TextDark,
                Left = 30,
                Top = _nextY + 6,
                Width = 370,
                Height = 24
            });
            _nextY += 40;
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            Company company = new Company
            {
                CompanyID = _existing?.CompanyID ?? 0,
                CompanyName = _nameBox.Text,
                RegistrationNumber = _registrationBox.Text,
                Phone = _phoneBox.Text,
                Address = _addressBox.Text
            };


            OperationResult result = _existing == null
                ? _controller.RegisterCompany(company, _managerUserNameBox.Text, _managerEmailBox.Text,
                                              _managerPasswordBox.Text, _managerConfirmBox.Text)
                : _controller.UpdateCompany(company);

            if (!result.Success)
            {
                UiHelper.ShowWarning(result.Message);
                return;
            }

            UiHelper.ShowSuccess(result.Message);
            DialogResult = DialogResult.OK;
        }
    }
}