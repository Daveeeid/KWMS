using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class MyPaymentsControl : GridScreen
    {
        private readonly ResidentRecordsController _controller = new ResidentRecordsController();

        public MyPaymentsControl() : base("My payment history")
        {
            UiHelper.AddColumn(Grid, "Date", "PaymentDate", 80, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Company", "CompanyName", 110);
            UiHelper.AddColumn(Grid, "Service", "ServiceName", 110);
            UiHelper.AddColumn(Grid, "Amount (RWF)", "Amount", 80, "N0");
            UiHelper.AddColumn(Grid, "Method", "MethodDisplay", 90);
            UiHelper.AddColumn(Grid, "Reference", "ReferenceNumber", 90);
        }

        protected override void LoadData()
        {
            DataResult<List<Payment>> result = _controller.GetMyPayments();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }
            Grid.DataSource = result.Data;
        }
    }
}