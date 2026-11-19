namespace KWMS.Views
{
    public static class Theme
    {
        public static readonly Color Primary = Color.FromArgb(46, 125, 50);        // green
        public static readonly Color PrimaryDark = Color.FromArgb(27, 94, 32);
        public static readonly Color Sidebar = Color.FromArgb(30, 41, 59);         // dark slate
        public static readonly Color SidebarHover = Color.FromArgb(51, 65, 85);
        public static readonly Color Background = Color.FromArgb(243, 244, 246);
        public static readonly Color TextDark = Color.FromArgb(31, 41, 55);
        public static readonly Color TextMuted = Color.FromArgb(107, 114, 128);
        public static readonly Color Danger = Color.FromArgb(198, 40, 40);

        public static readonly Font BaseFont = new Font("Segoe UI", 10F);
        public static readonly Font BoldFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font TitleFont = new Font("Segoe UI", 20F, FontStyle.Bold);

        public static void StylePrimaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = PrimaryDark;
            button.BackColor = Primary;
            button.ForeColor = Color.White;
            button.Font = BoldFont;
            button.Cursor = Cursors.Hand;
        }

        public static void StyleSidebarButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = SidebarHover;
            button.BackColor = Sidebar;
            button.ForeColor = Color.White;
            button.Font = BaseFont;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(20, 0, 0, 0);
            button.Margin = new Padding(0);
            button.Cursor = Cursors.Hand;
        }
    }
}