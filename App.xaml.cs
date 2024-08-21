using System.Diagnostics;
using System.Windows;

namespace CUpdater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Process AppProcess;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] args = e.Args;
            if (args.Length == 1)
            {
                if (int.TryParse(args[0], out var processId))
                {
                    try
                    {
                        // 查找正在运行的进程
                        AppProcess = Process.GetProcessById(processId);
                        AppProcess.EnableRaisingEvents = true;
                    }
                    catch { }
                }
            }


            var mainWindow = new CheckingForUpdatesWindow();
            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }
    }
}
