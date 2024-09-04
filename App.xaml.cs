using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace CUpdater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Process AppProcess;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            string[] args = e.Args;
            if (args.Length >= 1)
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
            if (args.Length >= 2)
            {
                if (args[1] == "debug")
                {
#if DEBUG
                    Thread.Sleep(20000);
#endif
                }
                else if (args[1] == "background")
                {
                    var cancelToken = new CancellationTokenSource();
                    var tuple = await CheckingForUpdatesWindow.CheckUpdateBackgroundAsync(cancelToken);
                    if (tuple == null)
                    {
                        Environment.Exit(0);
                        return;
                    }
                    var current = tuple.Item1;
                    var publish = tuple.Item2;
                    Dispatcher.Invoke(() =>
                    {
                        var update = new UpdateAvailableWindow(current, publish);
                        update.Show();
                    });
                    return;
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
