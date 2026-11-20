using KWMS.Controllers;
using KWMS.Models;

namespace KWMS.Views
{

    public class WorkerForm : FieldsDialog
    {
        private readonly WorkerController _controller = new WorkerController();
        private readonly Worker? _existing;

        private readonly TextBox _fullNameBox = new TextBox { MaxLength = 100 };
        private readonly TextBox _phoneBox = new TextBox { MaxLength = 20 };
        private readonly TextBox _userNameBox = new TextBox { MaxLength = 30 };
        private readonly TextBox _emailBox = new TextBox { MaxLength = 100 };
        private readonly TextBox _passwordBox = new TextBox { UseSystemPasswordChar = true, MaxLength = 100 };
        private readonly TextBox _confirmBox = new TextBox { UseSystemPasswordChar = true, MaxLength = 100 };

        public WorkerForm(Worker? existing = null)
            : base(existing == null ? "Add worker" : "Edit worker",
                   existing == null ? 490 : 230,
                   existing == null ? "Add worker" : "Save changes")
        {
            _existing = existing;

            AddRow("Full name *", _fullNameBox);
            AddRow("Phone (optional)", _phoneBox);

            if (existing != null)
            {
                _fullNameBox.Text = existing.FullName;
                _phoneBox.Text = existing.Phone;
            }
            else
            {
                AddHeading("Login account for the worker");
                AddRow("Username *", _userNameBox);
                AddRow("E-mail *", _emailBox);
                AddRow("Password *  (8+ characters, a letter and a digit)", _passwordBox);
                AddRow("Confirm password *", _confirmBox);
            }
        }

        protected override void OnSave()
        {
            Worker worker = new Worker
            {
                WorkerID = _existing?.WorkerID ?? 0,
                FullName = _fullNameBox.Text,
                Phone = _phoneBox.Text
            };

            ShowResult(_existing == null
                ? _controller.AddWorker(worker, _userNameBox.Text, _emailBox.Text,
                                        _passwordBox.Text, _confirmBox.Text)
                : _controller.UpdateWorker(worker));
        }
    }
}