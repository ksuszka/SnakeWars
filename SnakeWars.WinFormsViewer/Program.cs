using System;
using System.Windows.Forms;
using SnakeWars.WinFormsViewer.Properties;

namespace SnakeWars.WinFormsViewer
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new MainForm();

            var contestConnector = new ContestConnector(Settings.Default.ContestServerHost,
                Settings.Default.ContestServerPort, mainForm);

            Application.Run(mainForm);
        }
    }
}