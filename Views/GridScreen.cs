namespace KWMS.Views
{




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

            Controls.Add(Grid);
            Controls.Add(Toolbar);
            Controls.Add(titleLabel);

            Load += (s, e) => LoadData();
        }


        protected abstract void LoadData();


        protected virtual void OnSelectionChanged() { }


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
            box.SelectedIndex = 0;
            box.SelectedIndexChanged += (s, e) => LoadData();
            Toolbar.Controls.Add(box);
            return box;
        }


        protected void AddButton(Button button, string text, int width, Action onClick)
        {
            button.Text = text;
            button.Size = new Size(width, 36);
            Theme.StylePrimaryButton(button);
            button.Click += (s, e) => onClick();
            Toolbar.Controls.Add(button);
        }


        protected static void StyleToggle(Button button, bool isActive)
        {
            if (isActive) { button.Text = "Deactivate"; UiHelper.StyleDangerButton(button); }
            else { button.Text = "Activate"; Theme.StylePrimaryButton(button); }
        }
    }
}