using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class ManagerHomeControl : UserControl
    {
        private readonly StatisticsController _controller = new StatisticsController();

        private readonly StatCard _services = new StatCard("Active services", Theme.Primary);
        private readonly StatCard _workers = new StatCard("Active workers", Color.FromArgb(245, 124, 0));
        private readonly StatCard _subscribers = new StatCard("Subscribed residents", Color.FromArgb(123, 31, 162));
        private readonly StatCard _toAssign = new StatCard("Tasks to assign", Color.FromArgb(198, 40, 40));
        private readonly StatCard _openTasks = new StatCard("Tasks assigned / in progress", Color.FromArgb(2, 136, 209));
        private readonly StatCard _completed = new StatCard("Completed collections", Color.FromArgb(0, 150, 136));
        private readonly StatCard _complaints = new StatCard("Open complaints", Color.FromArgb(69, 90, 100));

        public ManagerHomeControl()
        {
            BackColor = Theme.Background;
            Padding = new Padding(25);

            FlowLayoutPanel cards = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
            cards.Controls.AddRange(new Control[]
            {
                _services, _workers, _subscribers, _toAssign, _openTasks, _completed, _complaints
            });

            Button refreshButton = new Button { Text = "Refresh", Width = 120, Height = 36 };
            UiHelper.StyleSecondaryButton(refreshButton);
            refreshButton.Click += (s, e) => LoadStatistics();

            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52 };
            toolbar.Controls.Add(refreshButton);

            Label title = new Label
            {
                Text = "Overview - " + SessionManager.CurrentUser?.CompanyName,
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
            DataResult<CompanyStatistics> result = _controller.GetManagerStatistics();

            if (!result.Success)
            {
                UiHelper.ShowError(result.Message);
                return;
            }

            CompanyStatistics stats = result.Data;
            _services.SetValue(stats.ActiveServices);
            _workers.SetValue(stats.ActiveWorkers);
            _subscribers.SetValue(stats.ActiveSubscribers);
            _toAssign.SetValue(stats.TasksToAssign);
            _openTasks.SetValue(stats.OpenTasks);
            _completed.SetValue(stats.CompletedTasks);
            _complaints.SetValue(stats.OpenComplaints);
        }
    }
}