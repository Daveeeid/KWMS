using System.Text;
using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{
    public class ComplaintDetailsForm : Form
    {
        private readonly Complaint _complaint;
        private readonly Func<int, DataResult<List<ComplaintResponse>>> _loadResponses;
        private readonly Func<int, string, OperationResult>? _respond;

        private readonly TextBox _conversationBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        private readonly TextBox _replyBox = new TextBox { Multiline = true, MaxLength = 2000, Dock = DockStyle.Fill };

        public ComplaintDetailsForm(Complaint complaint,
                                    Func<int, DataResult<List<ComplaintResponse>>> loadResponses,
                                    Func<int, string, OperationResult>? respond = null)
        {
            _complaint = complaint;
            _loadResponses = loadResponses;
            _respond = respond;

            Text = "Complaint: " + complaint.Subject;
            ClientSize = new Size(640, respond == null ? 440 : 600);
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            BackColor = Theme.Background;
            Font = Theme.BaseFont;

            Controls.Add(_conversationBox);   // Fill first

            if (respond != null)
            {
                Panel replyPanel = new Panel { Dock = DockStyle.Bottom, Height = 170, Padding = new Padding(10) };

                Button sendButton = new Button { Text = "Send response", Dock = DockStyle.Bottom, Height = 40 };
                Theme.StylePrimaryButton(sendButton);
                sendButton.Click += (s, e) => SendResponse();

                replyPanel.Controls.Add(_replyBox);
                replyPanel.Controls.Add(sendButton);
                replyPanel.Controls.Add(new Label
                {
                    Text = "Your response",
                    Dock = DockStyle.Top,
                    Height = 22,
                    ForeColor = Theme.TextMuted
                });
                Controls.Add(replyPanel);
            }

            Load += (s, e) => ShowConversation();
        }

        private void ShowConversation()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("Subject: " + _complaint.Subject);
            text.AppendLine("Company: " + _complaint.CompanyName);
            text.AppendLine($"From: {_complaint.ResidentName} on {_complaint.CreatedAt:dd MMM yyyy HH:mm}");
            text.AppendLine("Status: " + _complaint.StatusDisplay);
            text.AppendLine();
            text.AppendLine(_complaint.Description);
            text.AppendLine();
            text.AppendLine("---------- Responses ----------");

            DataResult<List<ComplaintResponse>> result = _loadResponses(_complaint.ComplaintID);
            if (!result.Success)
            {
                text.AppendLine(result.Message);
            }
            else if (result.Data.Count == 0)
            {
                text.AppendLine("No response yet.");
            }
            else
            {
                foreach (ComplaintResponse response in result.Data)
                {
                    text.AppendLine();
                    text.AppendLine($"[{response.CreatedAt:dd MMM yyyy HH:mm}] {response.ManagerName}:");
                    text.AppendLine(response.ResponseText);
                }
            }

            _conversationBox.Text = text.ToString();
            _conversationBox.SelectionStart = 0;
        }

        private void SendResponse()
        {
            if (_respond == null) return;

            OperationResult result = _respond(_complaint.ComplaintID, _replyBox.Text);
            if (!result.Success) { UiHelper.ShowWarning(result.Message); return; }

            _replyBox.Clear();
            ShowConversation();
            UiHelper.ShowSuccess(result.Message);
        }
    }
}