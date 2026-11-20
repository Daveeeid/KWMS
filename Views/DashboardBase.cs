using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{





    public class DashboardBase : Form
    {
        private readonly FlowLayoutPanel _menuPanel = new FlowLayoutPanel();
        private Button? _activeButton;

        protected Panel ContentPanel { get; } = new Panel();

        public DashboardBase(string roleTitle)
        {
            User user = SessionManager.CurrentUser
                        ?? throw new InvalidOperationException("No user is logged in.");

            Text = "KWMS - " + roleTitle;
            Size = new Size(1150, 720);
            MinimumSize = new Size(950, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = Theme.BaseFont;


            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = Theme.Sidebar };

            Panel brandPanel = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Theme.Sidebar };
            brandPanel.Controls.Add(new Label
            {
                Text = "KWMS",
                Font = Theme.TitleFont,
                ForeColor = Color.White,
                Left = 20,
                Top = 12,
                AutoSize = true
            });

            string company = user.CompanyName != null ? "\n" + user.CompanyName : "";
            brandPanel.Controls.Add(new Label
            {
                Text = user.UserName + "\n" + roleTitle + company,
                ForeColor = Color.FromArgb(203, 213, 225),
                Left = 20,
                Top = 58,
                Width = 200,
                Height = 60
            });

            Button changePasswordButton = new Button { Text = "Change password", Dock = DockStyle.Bottom, Height = 50 };
            Theme.StyleSidebarButton(changePasswordButton);
            changePasswordButton.Click += ChangePasswordButton_Click;

            Button logoutButton = new Button { Text = "Log out", Dock = DockStyle.Bottom, Height = 50 };
            Theme.StyleSidebarButton(logoutButton);
            logoutButton.Click += LogoutButton_Click;

            _menuPanel.Dock = DockStyle.Fill;
            _menuPanel.FlowDirection = FlowDirection.TopDown;
            _menuPanel.WrapContents = false;


            sidebar.Controls.Add(_menuPanel);
            sidebar.Controls.Add(brandPanel);
            sidebar.Controls.Add(changePasswordButton);
            sidebar.Controls.Add(logoutButton);


            ContentPanel.Dock = DockStyle.Fill;
            ContentPanel.BackColor = Theme.Background;

            Controls.Add(ContentPanel);
            Controls.Add(sidebar);
        }






        protected void AddMenuItem(string text, Func<Control>? createScreen = null, bool showNow = false)
        {
            Button button = new Button { Text = text, Width = 230, Height = 46 };
            Theme.StyleSidebarButton(button);

            void Open()
            {
                SetActiveButton(button);
                ShowScreen(createScreen!());
            }

            if (createScreen == null)
            {
                button.Click += (s, e) => MessageBox.Show("This screen is built in a later stage.", text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                button.Click += (s, e) => Open();
            }

            _menuPanel.Controls.Add(button);

            if (showNow && createScreen != null) Open();
        }


        protected void ShowScreen(Control screen)
        {
            ContentPanel.SuspendLayout();

            foreach (Control oldScreen in ContentPanel.Controls.Cast<Control>().ToList())
            {
                ContentPanel.Controls.Remove(oldScreen);
                oldScreen.Dispose();
            }

            screen.Dock = DockStyle.Fill;
            ContentPanel.Controls.Add(screen);
            ContentPanel.ResumeLayout();
        }


        protected void ShowDescription(string text)
        {
            Panel panel = new Panel { BackColor = Theme.Background };

            panel.Controls.Add(new Label
            {
                Text = "Welcome, " + SessionManager.CurrentUser?.UserName,
                Font = Theme.TitleFont,
                ForeColor = Theme.TextDark,
                Left = 28,
                Top = 24,
                AutoSize = true
            });
            panel.Controls.Add(new Label
            {
                Text = text,
                ForeColor = Theme.TextMuted,
                Left = 30,
                Top = 78,
                Width = 700,
                Height = 80
            });

            ShowScreen(panel);
        }

        private void SetActiveButton(Button button)
        {
            if (_activeButton != null) _activeButton.BackColor = Theme.Sidebar;
            _activeButton = button;
            button.BackColor = Theme.Primary;
        }

        private void ChangePasswordButton_Click(object? sender, EventArgs e)
        {
            using (ChangePasswordForm form = new ChangePasswordForm())
            {
                form.ShowDialog(this);
            }
        }

        private void LogoutButton_Click(object? sender, EventArgs e)
        {
            if (UiHelper.Confirm("Do you want to log out?", "Confirm log out"))
            {
                DialogResult = DialogResult.Retry;
            }
        }
    }
}