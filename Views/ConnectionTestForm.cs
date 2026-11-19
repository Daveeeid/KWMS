using KWMS.Controller;
using System;
using System.Collections.Generic;
using System.Text;

namespace KWMS.Views
{
    /// <summary>
    /// Temporary form that lets us confirm the database connection works.
    /// Notice: no SQL and no MySqlConnection here, only a call to the controller.
    /// </summary>
    public class ConnectionTestForm : Form
    {
        private readonly DatabaseTestController _controller = new DatabaseTestController();
        private readonly Button _testButton = new Button();
        private readonly TextBox _resultTextBox = new TextBox();

        public ConnectionTestForm()
        {
            // Form settings
            Text = "KWMS - Stage 1: Connection Test";
            Size = new Size(700, 450);
            StartPosition = FormStartPosition.CenterScreen;

            // Button
            _testButton.Text = "Test Database Connection";
            _testButton.Dock = DockStyle.Top;
            _testButton.Height = 45;
            _testButton.Click += TestButton_Click;

            // Result box
            _resultTextBox.Multiline = true;
            _resultTextBox.ReadOnly = true;
            _resultTextBox.ScrollBars = ScrollBars.Vertical;
            _resultTextBox.Dock = DockStyle.Fill;
            _resultTextBox.Font = new Font("Consolas", 10);

            // Add Fill control first, then Top, so docking lays out correctly
            Controls.Add(_resultTextBox);
            Controls.Add(_testButton);
        }

        private void TestButton_Click(object? sender, EventArgs e)
        {
            _resultTextBox.Text = "Testing connection...";
            _resultTextBox.Text = _controller.RunConnectionTest();
        }

        private void InitializeComponent()
        {

        }
    }
}
