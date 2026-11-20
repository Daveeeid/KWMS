using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class MySchedulesControl : GridScreen
    {
        private readonly ResidentRecordsController _controller = new ResidentRecordsController();

        public MySchedulesControl() : base("My collection schedule")
        {
            UiHelper.AddColumn(Grid, "Date", "CollectionDate", 90, "dd MMM yyyy");
            UiHelper.AddColumn(Grid, "Time", "CollectionTime", 60, "hh\\:mm");
            UiHelper.AddColumn(Grid, "Service", "ServiceName", 120);
            UiHelper.AddColumn(Grid, "Status", "Status", 80);
            UiHelper.AddColumn(Grid, "Notes", "Notes", 160);
        }

        protected override void LoadData()
        {
            DataResult<List<CollectionSchedule>> result = _controller.GetMySchedules();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }
            Grid.DataSource = result.Data;
        }
    }
}