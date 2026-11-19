using KWMS.Controller;
using KWMS.Controllers;

namespace KWMS.Views
{
    /// <summary>Temporary form for testing. Deleted when the Login form is created in Stage 4.</summary>
    public class ConnectionTestForm : Form
    {
        private readonly DatabaseTestController _connectionController = new DatabaseTestController();
        private readonly RepositoryTestController _repositoryController = new RepositoryTestController();

        private readonly FlowLayoutPanel _buttonPanel = new FlowLayoutPanel();
        private readonly Button _connectionButton = new Button();
        private readonly Button _repositoryButton = new Button();
        private readonly TextBox _resultTextBox = new TextBox();

        public ConnectionTestForm()
        {
            Text = "KWMS - Stage 3: Repository Tests";
            Size = new Size(850, 650);
            StartPosition = FormStartPosition.CenterScreen;

            _connectionButton.Text = "Test Connection (Stage 1)";
            _connectionButton.Size = new Size(230, 38);
            _connectionButton.Click += (s, e) => RunAction(_connectionController.RunConnectionTest);

            _repositoryButton.Text = "Run Repository Tests (Stage 3)";
            _repositoryButton.Size = new Size(260, 38);
            _repositoryButton.Click += (s, e) => RunAction(_repositoryController.RunTests);

            _buttonPanel.Dock = DockStyle.Top;
            _buttonPanel.Height = 52;
            _buttonPanel.Padding = new Padding(6);
            _buttonPanel.Controls.Add(_connectionButton);
            _buttonPanel.Controls.Add(_repositoryButton);

            _resultTextBox.Multiline = true;
            _resultTextBox.ReadOnly = true;
            _resultTextBox.ScrollBars = ScrollBars.Vertical;
            _resultTextBox.Dock = DockStyle.Fill;
            _resultTextBox.Font = new Font("Consolas", 10);

            Controls.Add(_resultTextBox);   // Fill first, then Top
            Controls.Add(_buttonPanel);
        }

        // Shows "Running...", calls the controller, then shows the report
        private void RunAction(Func<string> action)
        {
            _resultTextBox.Text = "Running...";
            _resultTextBox.Refresh();
            _resultTextBox.Text = action();
        }
    }
}