namespace KWMS.Views
{
    public static class UiHelper
    {
        // ----- Messages -----
        public static void ShowError(string message, string title = "Error") =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static void ShowWarning(string message, string title = "Please fix the following") =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public static void ShowSuccess(string message, string title = "Success") =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static bool Confirm(string message, string title = "Please confirm") =>
            MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        // ----- Grids -----
        public static void StyleGrid(DataGridView grid)
        {
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoGenerateColumns = false;           // we define the columns ourselves
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = Color.FromArgb(229, 231, 235);
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = 38;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Sidebar;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = Theme.BoldFont;

            grid.RowTemplate.Height = 32;
            grid.DefaultCellStyle.SelectionBackColor = Theme.Primary;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
        }

        /// <summary>
        /// Adds a text column. 'property' is the NAME of the model property to display.
        /// 'fillWeight' controls relative width. 'format' is optional, e.g. "dd MMM yyyy".
        /// </summary>
        public static void AddColumn(DataGridView grid, string header, string property,
                                     int fillWeight = 100, string? format = null)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                DataPropertyName = property,
                FillWeight = fillWeight
            };
            if (format != null) column.DefaultCellStyle.Format = format;
            grid.Columns.Add(column);
        }

        // ----- Buttons -----
        public static void StyleSecondaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Theme.Primary;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 245, 233);
            button.BackColor = Color.White;
            button.ForeColor = Theme.Primary;
            button.Font = Theme.BoldFont;
            button.Cursor = Cursors.Hand;
        }

        public static void StyleDangerButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(160, 30, 30);
            button.BackColor = Theme.Danger;
            button.ForeColor = Color.White;
            button.Font = Theme.BoldFont;
            button.Cursor = Cursors.Hand;
        }
    }
}