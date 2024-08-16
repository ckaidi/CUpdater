using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CUpdater
{
    /// <summary>
    /// Interaction logic for CheckingForUpdatesWindow.xaml.
    /// 
    /// Window that shows while NetSparkle is checking for updates.
    /// </summary>
    public partial class CheckingForUpdatesWindow : Xceed.Wpf.Toolkit.Window
    {
        private AppModel _appModel = new AppModel();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Create the window that tells the user that SparkleUpdater is checking
        /// for updates
        /// </summary>
        public CheckingForUpdatesWindow()
        {
            InitializeComponent();
            Closing += CheckingForUpdatesWindow_Closing;

            var uri = new Uri("pack://application:,,,/icon.ico");
            var bitmapImage = new BitmapImage(uri);
            if (bitmapImage != null)
            {
                Icon = bitmapImage;
            }
            Loaded += CheckingForUpdatesWindow_Loaded;
        }

        private async void CheckingForUpdatesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CheckUpdateAsync();
        }

        public async Task CheckUpdateAsync()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(_appModel.URL + "app.json", HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token))
                    {
                        var responseText = await response.Content.ReadAsStringAsync();
                        var appInfo = JsonConvert.DeserializeObject<AppInfo>(responseText);
                        new UpdateAvailableWindow(appInfo).Show();
                        Close();
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (HttpRequestException)
            {
            }
        }

        private void CheckingForUpdatesWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= CheckingForUpdatesWindow_Closing;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
