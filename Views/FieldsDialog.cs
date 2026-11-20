using KWMS.Controllers;

namespace KWMS.Views
{
    /// <summary>
    /// Base class for add/edit dialogs. A subclass adds its fields with AddRow() and
    /// implements OnSave(), which calls a controller and passes the result to ShowResult().
    /// </summary>
    public abstract class FieldsDialog : Form
    {
        private readonly Panel _fieldsPanel = new Panel();
        private int _nextY = 15;

        protected FieldsDialog(string title, int clientHeight, string saveText)
        {
            Text = title;
            ClientSize = new Size(430, clientHeight);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Theme.Background;
            Font = Theme.BaseFont;

            _fieldsPanel.Dock = DockStyle.Fill;
            _fieldsPanel.AutoScroll = true;

            Panel buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = Theme.Background };

            Button saveButton = new Button { Text = saveText };
            saveButton.SetBounds(30, 12, 200, 42);
            Theme.StylePrimaryButton(saveButton);
            saveButton.Click += (s, e) => OnSave();

            Button cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            cancelButton.SetBounds(245, 12, 150, 42);

            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(cancelButton);

            Controls.Add(_fieldsPanel);   // Fill first, then Bottom
            Controls.Add(buttonPanel);
            CancelButton = cancelButton;
        }

        protected abstract void OnSave();

        // Adds "label above input" at the next free vertical position
        protected void AddRow(string labelText, Control input, int height = 28)
        {
            _fieldsPanel.Controls.Add(new Label
            {
                Text = labelText,
                Left = 30,
                Top = _nextY,
                Width = 370,
                Height = 20,
                ForeColor = Theme.TextMuted
            });

            input.SetBounds(30, _nextY + 22, 370, height);
            _fieldsPanel.Controls.Add(input);
            _nextY += 30 + height;
        }

        protected void AddHeading(string text)
        {
            _fieldsPanel.Controls.Add(new Label
            {
                Text = text,
                Font = Theme.BoldFont,
                ForeColor = Theme.TextDark,
                Left = 30,
                Top = _nextY + 6,
                Width = 370,
                Height = 24
            });
            _nextY += 40;
        }

        // Shows the controller's message. On success the dialog closes with DialogResult.OK.
        protected void ShowResult(OperationResult result)
        {
            if (!result.Success)
            {
                UiHelper.ShowWarning(result.Message);
                return;
            }

            UiHelper.ShowSuccess(result.Message);
            DialogResult = DialogResult.OK;
        }
    }
}