using KWMS.Controllers;

namespace KWMS.Views
{
    /// <summary>Lets the logged-in user (any role) change their own password.</summary>
    public class ChangePasswordForm : Form
    {
        private readonly AccountController _controller = new AccountController();

        private readonly TextBox _currentBox = new TextBox();
        private readonly TextBox _newBox = new TextBox();
        private readonly TextBox _confirmBox = new TextBox();

        public ChangePasswordForm()
        {
            Text = "Change password";
            ClientSize = new Size(400, 295);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.Background;
            Font = Theme.BaseFont;

            AddField("Current password", _currentBox, 20);
            AddField("New password  (8+ characters, a letter and a digit)", _newBox, 88);
            AddField("Confirm new password", _confirmBox, 156);

            Button saveButton = new Button { Text = "Change password" };
            saveButton.SetBounds(30, 232, 200, 42);
            Theme.StylePrimaryButton(saveButton);
            saveButton.Click += SaveButton_Click;

            Button cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            cancelButton.SetBounds(245, 232, 125, 42);

            Controls.Add(saveButton);
            Controls.Add(cancelButton);
            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private void AddField(string labelText, TextBox box, int top)
        {
            Controls.Add(new Label
            {
                Text = labelText,
                Left = 30,
                Top = top,
                Width = 340,
                ForeColor = Theme.TextMuted
            });

            box.SetBounds(30, top + 22, 340, 28);
            box.UseSystemPasswordChar = true;
            box.MaxLength = 100;
            Controls.Add(box);
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            OperationResult result = _controller.ChangePassword(_currentBox.Text, _newBox.Text, _confirmBox.Text);

            if (!result.Success)
            {
                UiHelper.ShowWarning(result.Message);
                _currentBox.Clear();
                _currentBox.Focus();
                return;
            }

            UiHelper.ShowSuccess(result.Message);
            DialogResult = DialogResult.OK;
        }
    }
}