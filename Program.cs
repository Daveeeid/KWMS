using KWMS.Controllers;
using KWMS.Views;

namespace KWMS
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();


            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) =>
                MessageBox.Show("An unexpected error occurred. The action was not completed.\n\n" + e.Exception.Message,
                    "KWMS - Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            OperationResult setup = new AuthController().EnsureDefaultAdmin();

            if (!setup.Success)
            {
                MessageBox.Show(setup.Message, "KWMS - Startup problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (setup.Message.Length > 0)
            {
                MessageBox.Show(setup.Message, "KWMS - First-time setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            while (true)
            {
                using (LoginForm login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK) break;
                }

                using (Form dashboard = DashboardFactory.Create(SessionManager.CurrentUser!))
                {
                    DialogResult result = dashboard.ShowDialog();
                    SessionManager.End();

                    if (result != DialogResult.Retry) break;
                }
            }
        }
    }
}