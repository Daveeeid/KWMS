using KWMS.Controllers;

namespace KWMS.Views
{




    public class LoginForm : Form
    {
        private readonly AuthController _controller = new AuthController();

        private readonly TextBox _userNameBox = new TextBox();
        private readonly TextBox _passwordBox = new TextBox();
        private readonly Button _loginButton = new Button();
        private readonly LinkLabel _registerLink = new LinkLabel();
        private readonly Label _messageLabel = new Label();

        public LoginForm()
        {
            Text = "KWMS - Login";
            ClientSize = new Size(400, 430);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Theme.Background;
            Font = Theme.BaseFont;


            Panel header = new Panel { Dock = DockStyle.Top, Height = 110, BackColor = Theme.Primary };
            header.Controls.Add(new Label
            {
                Text = "KWMS",
                Font = Theme.TitleFont,
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.BottomCenter,
                Dock = DockStyle.Top,
                Height = 62
            });
            header.Controls.Add(new Label
            {
                Text = "Kigali Waste Management System",
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Fill
            });


            Controls.Add(new Label { Text = "Username", Left = 30, Top = 135, Width = 340, ForeColor = Theme.TextMuted });
            _userNameBox.SetBounds(30, 158, 340, 28);
            _userNameBox.MaxLength = 50;


            Controls.Add(new Label { Text = "Password", Left = 30, Top = 200, Width = 340, ForeColor = Theme.TextMuted });
            _passwordBox.SetBounds(30, 223, 340, 28);
            _passwordBox.UseSystemPasswordChar = true;
            _passwordBox.MaxLength = 100;


            _messageLabel.SetBounds(30, 262, 340, 40);
            _messageLabel.ForeColor = Theme.Danger;


            _loginButton.Text = "Log in";
            _loginButton.SetBounds(30, 310, 340, 42);
            Theme.StylePrimaryButton(_loginButton);
            _loginButton.Click += LoginButton_Click;


            _registerLink.Text = "New resident? Create an account";
            _registerLink.SetBounds(30, 370, 340, 24);
            _registerLink.TextAlign = ContentAlignment.MiddleCenter;
            _registerLink.LinkClicked += RegisterLink_Clicked;

            Controls.Add(_userNameBox);
            Controls.Add(_passwordBox);
            Controls.Add(_messageLabel);
            Controls.Add(_loginButton);
            Controls.Add(_registerLink);
            Controls.Add(header);

            AcceptButton = _loginButton;
        }

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            _messageLabel.Text = string.Empty;

            OperationResult result = _controller.Login(_userNameBox.Text, _passwordBox.Text);

            if (!result.Success)
            {
                _messageLabel.Text = result.Message;
                _passwordBox.Clear();
                _passwordBox.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void RegisterLink_Clicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            using (RegisterForm registerForm = new RegisterForm())
            {
                if (registerForm.ShowDialog(this) == DialogResult.OK)
                {
                    _userNameBox.Text = registerForm.RegisteredUserName;
                    _passwordBox.Clear();
                    _passwordBox.Focus();
                }
            }
        }
    }
}