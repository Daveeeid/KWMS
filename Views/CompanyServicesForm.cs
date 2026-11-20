using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    /// <summary>Lists one company's available services and lets the resident subscribe.</summary>
    public class CompanyServicesForm : Form
    {
        private readonly ResidentSubscriptionController _controller = new ResidentSubscriptionController();
        private readonly Company _company;
        private readonly DataGridView _grid = new DataGridView();

        public CompanyServicesForm(Company company)
        {
            _company = company;

            Text = "Services - " + company.CompanyName;
            ClientSize = new Size(740, 420);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.Background;
            Font = Theme.BaseFont;

            UiHelper.StyleGrid(_grid);
            UiHelper.AddColumn(_grid, "Service", "ServiceName", 130);
            UiHelper.AddColumn(_grid, "Description", "Description", 200);
            UiHelper.AddColumn(_grid, "Price per month (RWF)", "MonthlyPrice", 90, "N0");
            UiHelper.AddColumn(_grid, "Frequency", "CollectionFrequency", 80);
            _grid.Dock = DockStyle.Fill;

            Panel bottom = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Theme.Background };

            Button subscribeButton = new Button { Text = "Subscribe to selected service" };
            subscribeButton.SetBounds(20, 12, 260, 40);
            Theme.StylePrimaryButton(subscribeButton);
            subscribeButton.Click += (s, e) => Subscribe();

            Button closeButton = new Button { Text = "Close", DialogResult = DialogResult.Cancel };
            closeButton.SetBounds(295, 12, 120, 40);

            bottom.Controls.Add(subscribeButton);
            bottom.Controls.Add(closeButton);

            Controls.Add(_grid);      // Fill first, then Bottom
            Controls.Add(bottom);
            CancelButton = closeButton;

            Load += (s, e) => LoadServices();
        }

        private void LoadServices()
        {
            DataResult<List<Service>> result = _controller.GetServices(_company.CompanyID);
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }
            _grid.DataSource = result.Data;
        }

        private void Subscribe()
        {
            Service? service = _grid.CurrentRow?.DataBoundItem as Service;
            if (service == null) { UiHelper.ShowWarning("Please select a service first.", "No service selected"); return; }

            string question = $"Subscribe to '{service.ServiceName}' ({service.MonthlyPrice:N0} RWF per month) with {_company.CompanyName}?\n\n" +
                              "You can only have one active subscription at a time.";
            if (!UiHelper.Confirm(question, "Confirm subscription")) return;

            OperationResult result = _controller.Subscribe(service.ServiceID);
            if (!result.Success) { UiHelper.ShowWarning(result.Message, "Cannot subscribe"); return; }

            UiHelper.ShowSuccess(result.Message);
            DialogResult = DialogResult.OK;
        }
    }
}