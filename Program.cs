using KWMS.Views;

namespace KWMS
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Temporary: in Stage 4 this will become the Login form
            Application.Run(new ConnectionTestForm());
        }
    }
}