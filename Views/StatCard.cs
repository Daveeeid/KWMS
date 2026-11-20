namespace KWMS.Views
{

    public class StatCard : Panel
    {
        private readonly Label _valueLabel = new Label();

        public StatCard(string caption, Color accent)
        {
            Size = new Size(210, 100);
            BackColor = Color.White;
            Margin = new Padding(0, 0, 18, 18);

            Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6, BackColor = accent });

            _valueLabel.Text = "-";
            _valueLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            _valueLabel.ForeColor = Theme.TextDark;
            _valueLabel.SetBounds(20, 12, 180, 46);
            Controls.Add(_valueLabel);

            Controls.Add(new Label
            {
                Text = caption,
                ForeColor = Theme.TextMuted,
                Left = 22,
                Top = 64,
                Width = 180,
                Height = 24
            });
        }

        public void SetValue(int value) => _valueLabel.Text = value.ToString();
    }
}