using CommandLine;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
            Parser.Default.ParseArguments<ArgsOptionModel>(args).WithParsed(opts => RunOptionsAsync(opts));
        }

        private async void RunOptionsAsync(ArgsOptionModel opts)
        {
            if (opts.Command)
            {

            }
            else
            {
                if (opts.ProcessId != -1)
                    try
                    {
                        // 查找正在运行的进程
                        AppProcess = Process.GetProcessById(opts.ProcessId);
                        AppProcess.EnableRaisingEvents = true;
                    }
                    catch { }
                switch (opts.Mode)
                {
                    case "debug":
#if DEBUG
                        Thread.Sleep(20000);
#endif
                        break;
                    case "background":
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
                    case "auto":
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
                            if(current==null)
                            {
                                Environment.Exit(0);
                                return;
                            }
                            await UpdateAvailableWindow.DownloadNewVersionAppAsync(current, publish);
                            Environment.Exit(0);
                            return;
                        }
                    default: break;
                }

                var mainWindow = new CheckingForUpdatesWindow();
                mainWindow.Show();
            }
        }
    }
}
