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

            // 1. First-run setup: make sure an administrator exists
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

            // 2. Login -> dashboard -> (log out) -> login ... until the user closes a window with X
            while (true)
            {
                using (LoginForm login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK) break;   // login window closed: exit app
                }

                using (Form dashboard = DashboardFactory.Create(SessionManager.CurrentUser!))
                {
                    DialogResult result = dashboard.ShowDialog();
                    SessionManager.End();                                // always clear the session

                    if (result != DialogResult.Retry) break;             // closed with X: exit app
                }                                                        // Retry = logged out: back to login
            }
        }
    }
}