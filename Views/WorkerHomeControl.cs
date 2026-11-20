using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class WorkerHomeControl : UserControl
    {
        private readonly WorkerTaskController _controller = new WorkerTaskController();

        private readonly StatCard _toStart = new StatCard("Assigned, not started", Color.FromArgb(2, 136, 209));
        private readonly StatCard _inProgress = new StatCard("In progress", Color.FromArgb(245, 124, 0));
        private readonly StatCard _today = new StatCard("Completed today", Theme.Primary);
        private readonly StatCard _total = new StatCard("Completed in total", Color.FromArgb(69, 90, 100));

        public WorkerHomeControl()
        {
            BackColor = Theme.Background;
            Padding = new Padding(25);

            FlowLayoutPanel cards = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
            cards.Controls.AddRange(new Control[] { _toStart, _inProgress, _today, _total });

            Button refreshButton = new Button { Text = "Refresh", Width = 120, Height = 36 };
            UiHelper.StyleSecondaryButton(refreshButton);
            refreshButton.Click += (s, e) => LoadStatistics();

            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52 };
            toolbar.Controls.Add(refreshButton);

            Label title = new Label
            {
                Text = "My work - " + SessionManager.CurrentUser?.CompanyName,
                Font = Theme.TitleFont,
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 50
            };

            Controls.Add(cards);
            Controls.Add(toolbar);
            Controls.Add(title);

            Load += (s, e) => LoadStatistics();
        }

        private void LoadStatistics()
        {
            DataResult<WorkerStatistics> result = _controller.GetMyStatistics();

            if (!result.Success)
            {
                UiHelper.ShowError(result.Message);
                return;
            }

            _toStart.SetValue(result.Data.ToStart);
            _inProgress.SetValue(result.Data.InProgress);
            _today.SetValue(result.Data.CompletedToday);
            _total.SetValue(result.Data.CompletedTotal);
        }
    }
}