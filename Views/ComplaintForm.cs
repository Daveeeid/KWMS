using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class ComplaintForm : FieldsDialog
    {
        private readonly ResidentComplaintController _controller = new ResidentComplaintController();

        private readonly ComboBox _companyBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _subjectBox = new TextBox { MaxLength = 150 };
        private readonly TextBox _descriptionBox = new TextBox { MaxLength = 2000, Multiline = true };

        public ComplaintForm() : base("New complaint", 360, "Submit complaint")
        {
            AddRow("Company *", _companyBox);
            AddRow("Subject *", _subjectBox);
            AddRow("Describe the problem *", _descriptionBox, 110);

            DataResult<List<Company>> result = _controller.GetComplaintCompanies();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            _companyBox.DisplayMember = "CompanyName";
            _companyBox.ValueMember = "CompanyID";
            _companyBox.DataSource = result.Data;
        }

        protected override void OnSave()
        {
            if (_companyBox.SelectedValue == null)
            {
                UiHelper.ShowWarning("You can only complain about a company you have subscribed to. Subscribe to a service first.");
                return;
            }

            ShowResult(_controller.CreateComplaint(
                Convert.ToInt32(_companyBox.SelectedValue), _subjectBox.Text, _descriptionBox.Text));
        }
    }
}