using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class BrowseCompaniesControl : GridScreen
    {
        private readonly ResidentSubscriptionController _controller = new ResidentSubscriptionController();
        private readonly Button _viewButton = new Button();
        private readonly TextBox _searchBox;

        public BrowseCompaniesControl() : base("Waste collection companies")
        {
            UiHelper.AddColumn(Grid, "Company", "CompanyName", 150);
            UiHelper.AddColumn(Grid, "Phone", "Phone", 80);
            UiHelper.AddColumn(Grid, "Address", "Address", 150);

            _searchBox = AddSearchBox("Search company name");
            AddButton(_viewButton, "View services & subscribe", 230, OpenServices);
            Grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OpenServices(); };
        }

        protected override void LoadData()
        {
            DataResult<List<Company>> result = _controller.GetActiveCompanies(_searchBox.Text);
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }
            Grid.DataSource = result.Data;
            OnSelectionChanged();
        }

        protected override void OnSelectionChanged() => _viewButton.Enabled = Selected<Company>() != null;

        private void OpenServices()
        {
            Company? company = Selected<Company>();
            if (company == null) return;

            using (CompanyServicesForm form = new CompanyServicesForm(company))
            {
                form.ShowDialog(this);
            }
        }
    }
}