using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class ResidentHomeControl : UserControl
    {
        private readonly ResidentController _controller = new ResidentController();

        private readonly StatCard _upcoming = new StatCard("Upcoming collections", Theme.Primary);
        private readonly StatCard _pending = new StatCard("Pending pickup requests", Color.FromArgb(245, 124, 0));
        private readonly StatCard _complaints = new StatCard("Open complaints", Color.FromArgb(198, 40, 40));
        private readonly StatCard _payments = new StatCard("Payments recorded", Color.FromArgb(2, 136, 209));
        private readonly Label _subscriptionLabel = new Label();

        public ResidentHomeControl()
        {
            BackColor = Theme.Background;
            Padding = new Padding(25);

            FlowLayoutPanel cards = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
            cards.Controls.AddRange(new Control[] { _upcoming, _pending, _complaints, _payments });

            _subscriptionLabel.Dock = DockStyle.Top;
            _subscriptionLabel.Height = 40;
            _subscriptionLabel.Font = Theme.BoldFont;
            _subscriptionLabel.ForeColor = Theme.Primary;

            Button refreshButton = new Button { Text = "Refresh", Width = 120, Height = 36 };
            UiHelper.StyleSecondaryButton(refreshButton);
            refreshButton.Click += (s, e) => LoadStatistics();

            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52 };
            toolbar.Controls.Add(refreshButton);

            Label title = new Label
            {
                Text = "My household",
                Font = Theme.TitleFont,
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 50
            };

            Controls.Add(cards);
            Controls.Add(_subscriptionLabel);
            Controls.Add(toolbar);
            Controls.Add(title);

            Load += (s, e) => LoadStatistics();
        }

        private void LoadStatistics()
        {
            DataResult<ResidentStatistics> result = _controller.GetMyStatistics();
            if (!result.Success) { UiHelper.ShowError(result.Message); return; }

            _subscriptionLabel.Text = "Current subscription: " + result.Data.SubscriptionText;
            _upcoming.SetValue(result.Data.UpcomingCollections);
            _pending.SetValue(result.Data.PendingRequests);
            _complaints.SetValue(result.Data.OpenComplaints);
            _payments.SetValue(result.Data.PaymentsMade);
        }
    }
}