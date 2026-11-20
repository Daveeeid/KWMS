using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class ServicesControl : GridScreen
    {
        private readonly ServiceController _controller = new ServiceController();

        // Field initializers run before the base constructor, so these are ready
        // even if OnSelectionChanged fires early.
        private readonly Button _editButton = new Button();
        private readonly Button _toggleButton = new Button();

        public ServicesControl() : base("Services")
        {
            UiHelper.AddColumn(Grid, "Service", "ServiceName", 130);
            UiHelper.AddColumn(Grid, "Description", "Description", 170);
            UiHelper.AddColumn(Grid, "Monthly price (RWF)", "MonthlyPrice", 90, "N0");
            UiHelper.AddColumn(Grid, "Frequency", "CollectionFrequency", 80);
            UiHelper.AddColumn(Grid, "Status", "Status", 60);

            AddButton(new Button(), "+ Add service", 150, AddService);
            AddButton(_editButton, "Edit", 100, EditSelected);
            UiHelper.StyleSecondaryButton(_editButton);
            AddButton(_toggleButton, "Deactivate", 130, ToggleStatus);

            Grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) EditSelected(); };
        }

        protected override void LoadData()
        {
            DataResult<List<Service>> result = _controller.GetServices();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            Grid.DataSource = result.Data;
            OnSelectionChanged();
        }

        protected override void OnSelectionChanged()
        {
            Service? service = Selected<Service>();
            _editButton.Enabled = service != null;
            _toggleButton.Enabled = service != null;
            StyleToggle(_toggleButton, service == null || service.Status == RecordStatus.Active);
        }

        private void AddService()
        {
            using (ServiceForm form = new ServiceForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }

        private void EditSelected()
        {
            Service? service = Selected<Service>();
            if (service == null) { UiHelper.ShowWarning("Please select a service first.", "No service selected"); return; }

            using (ServiceForm form = new ServiceForm(service))
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }

        private void ToggleStatus()
        {
            Service? service = Selected<Service>();
            if (service == null) return;

            bool deactivate = service.Status == RecordStatus.Active;
            string question = deactivate
                ? $"Deactivate '{service.ServiceName}'?\n\nResidents will no longer be able to subscribe to it. Current subscribers keep their subscription."
                : $"Activate '{service.ServiceName}' again?";

            if (!UiHelper.Confirm(question)) return;

            OperationResult result = _controller.SetServiceStatus(
                service.ServiceID, deactivate ? RecordStatus.Inactive : RecordStatus.Active);

            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            UiHelper.ShowSuccess(result.Message);
            LoadData();
        }
    }
}