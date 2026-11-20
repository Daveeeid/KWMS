namespace KWMS.Views
{
    /// <summary>
    /// Base class for list screens: title + toolbar + DataGridView.
    /// A subclass adds its columns and buttons in its constructor and implements LoadData().
    /// </summary>
    public abstract class GridScreen : UserControl
    {
        protected DataGridView Grid { get; } = new DataGridView();
        protected FlowLayoutPanel Toolbar { get; } = new FlowLayoutPanel();

        protected GridScreen(string title)
        {
            BackColor = Theme.Background;
            Padding = new Padding(25);

            UiHelper.StyleGrid(Grid);
            Grid.Dock = DockStyle.Fill;
            Grid.SelectionChanged += (s, e) => OnSelectionChanged();

            Toolbar.Dock = DockStyle.Top;
            Toolbar.Height = 52;
            Toolbar.WrapContents = false;

            Label titleLabel = new Label
            {
                Text = title,
                Font = Theme.TitleFont,
                ForeColor = Theme.TextDark,
                Dock = DockStyle.Top,
                Height = 50
            };

            Controls.Add(Grid);       // Fill first, then Top controls
            Controls.Add(Toolbar);
            Controls.Add(titleLabel);

            Load += (s, e) => LoadData();
        }

        /// <summary>Asks the controller for data and puts it in Grid.DataSource.</summary>
        protected abstract void LoadData();

        /// <summary>Called when the highlighted row changes. Override to enable/disable buttons.</summary>
        protected virtual void OnSelectionChanged() { }

        // The object behind the highlighted row (null when no row is selected)
        protected T? Selected<T>() where T : class => Grid.CurrentRow?.DataBoundItem as T;

        protected TextBox AddSearchBox(string placeholder)
        {
            TextBox box = new TextBox { Width = 240, PlaceholderText = placeholder, Margin = new Padding(0, 4, 10, 0) };
            box.TextChanged += (s, e) => LoadData();
            Toolbar.Controls.Add(box);
            return box;
        }

        protected ComboBox AddFilter(int width, params string[] items)
        {
            ComboBox box = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = width,
                Margin = new Padding(0, 4, 20, 0)
            };
            box.Items.AddRange(items);
            box.SelectedIndex = 0;                                   // set BEFORE attaching the event
            box.SelectedIndexChanged += (s, e) => LoadData();
            Toolbar.Controls.Add(box);
            return box;
        }

        // Styles a button as the green primary button and adds it to the toolbar
        protected void AddButton(Button button, string text, int width, Action onClick)
        {
            button.Text = text;
            button.Size = new Size(width, 36);
            Theme.StylePrimaryButton(button);
            button.Click += (s, e) => onClick();
            Toolbar.Controls.Add(button);
        }

        // Turns a button into "Deactivate" (red) or "Activate" (green)
        protected static void StyleToggle(Button button, bool isActive)
        {
            if (isActive) { button.Text = "Deactivate"; UiHelper.StyleDangerButton(button); }
            else { button.Text = "Activate"; Theme.StylePrimaryButton(button); }
        }
    }
}