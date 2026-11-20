using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class CompaniesControl : UserControl
    {
        private readonly CompanyController _controller = new CompanyController();

        private readonly DataGridView _grid = new DataGridView();
        private readonly TextBox _searchBox = new TextBox();
        private readonly ComboBox _statusFilter = new ComboBox();
        private readonly Button _addButton = new Button();
        private readonly Button _editButton = new Button();
        private readonly Button _toggleButton = new Button();


        private Company? SelectedCompany => _grid.CurrentRow?.DataBoundItem as Company;

        public CompaniesControl()
        {
            BackColor = Theme.Background;
            Padding = new Padding(25);


            UiHelper.StyleGrid(_grid);
            UiHelper.AddColumn(_grid, "Company name", "CompanyName", 160);
            UiHelper.AddColumn(_grid, "Registration no.", "RegistrationNumber", 100);
            UiHelper.AddColumn(_grid, "Phone", "Phone", 80);
            UiHelper.AddColumn(_grid, "Address", "Address", 130);
            UiHelper.AddColumn(_grid, "Status", "Status", 60);
            UiHelper.AddColumn(_grid, "Registered", "CreatedAt", 80, "dd MMM yyyy");
            _grid.Dock = DockStyle.Fill;
            _grid.SelectionChanged += (s, e) => UpdateButtons();
            _grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditSelected(); };


            _searchBox.Width = 240;
            _searchBox.PlaceholderText = "Search name or registration no.";
            _searchBox.Margin = new Padding(0, 4, 10, 0);
            _searchBox.TextChanged += (s, e) => LoadCompanies();

            _statusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            _statusFilter.Width = 140;
            _statusFilter.Margin = new Padding(0, 4, 20, 0);
            _statusFilter.Items.AddRange(new object[] { "All statuses", "Active", "Inactive" });
            _statusFilter.SelectedIndex = 0;
            _statusFilter.SelectedIndexChanged += (s, e) => LoadCompanies();

            _addButton.Text = "+ Register company";
            _addButton.Size = new Size(160, 36);
            Theme.StylePrimaryButton(_addButton);
            _addButton.Click += (s, e) => AddCompany();

            _editButton.Text = "Edit";
            _editButton.Size = new Size(100, 36);
            UiHelper.StyleSecondaryButton(_editButton);
            _editButton.Click += (s, e) => EditSelected();

            _toggleButton.Text = "Deactivate";
            _toggleButton.Size = new Size(130, 36);
            _toggleButton.Click += (s, e) => ToggleStatus();

            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, WrapContents = false };
            toolbar.Controls.AddRange(new Control[] { _searchBox, _statusFilter, _addButton, _editButton, _toggleButton });

            Label title = new Label
            {
                Text = "Companies",
                Font = Theme.TitleFont,
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 50
            };

            Controls.Add(_grid);
            Controls.Add(toolbar);
            Controls.Add(title);

            Load += (s, e) => LoadCompanies();
        }

        private void LoadCompanies()
        {
            RecordStatus? status = _statusFilter.SelectedIndex switch
            {
                1 => RecordStatus.Active,
                2 => RecordStatus.Inactive,
                _ => null
            };

            DataResult<List<Company>> result = _controller.GetCompanies(_searchBox.Text, status);

            if (!result.Success)
            {
                UiHelper.ShowError(result.Message);
                return;
            }

            _grid.DataSource = result.Data;
            UpdateButtons();
        }


        private void UpdateButtons()
        {
            Company? company = SelectedCompany;

            _editButton.Enabled = company != null;
            _toggleButton.Enabled = company != null;

            if (company != null && !company.IsActive)
            {
                _toggleButton.Text = "Activate";
                Theme.StylePrimaryButton(_toggleButton);
            }
            else
            {
                _toggleButton.Text = "Deactivate";
                UiHelper.StyleDangerButton(_toggleButton);
            }
        }

        private void AddCompany()
        {
            using (CompanyForm form = new CompanyForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadCompanies();
            }
        }

        private void EditSelected()
        {
            Company? company = SelectedCompany;
            if (company == null)
            {
                UiHelper.ShowWarning("Please select a company first.", "No company selected");
                return;
            }

            using (CompanyForm form = new CompanyForm(company))
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadCompanies();
            }
        }

        private void ToggleStatus()
        {
            Company? company = SelectedCompany;
            if (company == null) return;

            RecordStatus newStatus = company.IsActive ? RecordStatus.Inactive : RecordStatus.Active;

            string question = company.IsActive
                ? $"Deactivate '{company.CompanyName}'?\n\nIt will no longer accept new subscriptions and its services will be hidden from residents. Existing data is kept."
                : $"Activate '{company.CompanyName}' again?";

            if (!UiHelper.Confirm(question)) return;

            OperationResult result = _controller.SetCompanyStatus(company.CompanyID, newStatus);

            if (!result.Success)
            {
                UiHelper.ShowError(result.Message);
                return;
            }

            UiHelper.ShowSuccess(result.Message);
            LoadCompanies();
        }
    }
}