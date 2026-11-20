using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class AdminHomeControl : UserControl
    {
        private readonly StatisticsController _controller = new StatisticsController();

        private readonly StatCard _totalCompanies = new StatCard("Registered companies", Theme.Primary);
        private readonly StatCard _activeCompanies = new StatCard("Active companies", Color.FromArgb(0, 150, 136));
        private readonly StatCard _totalUsers = new StatCard("User accounts", Color.FromArgb(2, 136, 209));
        private readonly StatCard _residents = new StatCard("Residents", Color.FromArgb(123, 31, 162));
        private readonly StatCard _workers = new StatCard("Collection workers", Color.FromArgb(245, 124, 0));
        private readonly StatCard _subscriptions = new StatCard("Active subscriptions", Color.FromArgb(194, 24, 91));
        private readonly StatCard _completed = new StatCard("Completed collections", Color.FromArgb(69, 90, 100));

        public AdminHomeControl()
        {
            BackColor = Theme.Background;
            Padding = new Padding(25);

            FlowLayoutPanel cards = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
            cards.Controls.AddRange(new Control[]
            {
                _totalCompanies, _activeCompanies, _totalUsers, _residents,
                _workers, _subscriptions, _completed
            });

            Button refreshButton = new Button { Text = "Refresh", Width = 120, Height = 36 };
            UiHelper.StyleSecondaryButton(refreshButton);
            refreshButton.Click += (s, e) => LoadStatistics();

            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52 };
            toolbar.Controls.Add(refreshButton);

            Label title = new Label
            {
                Text = "System overview",
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
            DataResult<SystemStatistics> result = _controller.GetAdminStatistics();

            if (!result.Success)
            {
                UiHelper.ShowError(result.Message);
                return;
            }

            SystemStatistics stats = result.Data;
            _totalCompanies.SetValue(stats.TotalCompanies);
            _activeCompanies.SetValue(stats.ActiveCompanies);
            _totalUsers.SetValue(stats.TotalUsers);
            _residents.SetValue(stats.TotalResidents);
            _workers.SetValue(stats.TotalWorkers);
            _subscriptions.SetValue(stats.ActiveSubscriptions);
            _completed.SetValue(stats.CompletedCollections);
        }
    }
}