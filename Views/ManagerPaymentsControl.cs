using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class ManagerPaymentsControl : GridScreen
    {
        private readonly PaymentController _controller = new PaymentController();

        public ManagerPaymentsControl() : base("Payments")
        {
            UiHelper.AddColumn(Grid, "Date", "PaymentDate", 80, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Resident", "ResidentName", 110);
            UiHelper.AddColumn(Grid, "Service", "ServiceName", 100);
            UiHelper.AddColumn(Grid, "Amount (RWF)", "Amount", 80, "N0");
            UiHelper.AddColumn(Grid, "Method", "MethodDisplay", 90);
            UiHelper.AddColumn(Grid, "Reference", "ReferenceNumber", 90);
            UiHelper.AddColumn(Grid, "Recorded by", "RecordedByName", 90);

            AddButton(new Button(), "+ Record payment", 180, RecordPayment);
        }

        protected override void LoadData()
        {
            DataResult<List<Payment>> result = _controller.GetPayments();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }
            Grid.DataSource = result.Data;
        }

        private void RecordPayment()
        {
            using (PaymentForm form = new PaymentForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK) LoadData();
            }
        }
    }
}