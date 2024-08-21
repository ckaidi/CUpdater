using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
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
        public static string PublishJsonText;
        public PublishFileModel _publishFileModel;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Create the window that tells the user that SparkleUpdater is checking
        /// for updates
        /// </summary>
        public CheckingForUpdatesWindow()
        {
            InitializeComponent();

            var uri = new Uri("pack://application:,,,/icon.ico");
            var bitmapImage = new BitmapImage(uri);
            if (bitmapImage != null)
            {
                Icon = bitmapImage;
            }
            Loaded += CheckingForUpdatesWindow_Loaded;
        }

        private void CheckingForUpdatesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CheckUpdateAsync();
        }

        public async void CheckUpdateAsync()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(AppModel.URL + "app.json", HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token))
                    {
                        PublishJsonText = await response.Content.ReadAsStringAsync();
                        _publishFileModel = JsonConvert.DeserializeObject<PublishFileModel>(PublishJsonText);
                        PublishFileModel currentVersion = null;
                        if (File.Exists("../app.json"))
                        {
                            var thisPublishFileModelJson = File.ReadAllText("../app.json");
                            if (thisPublishFileModelJson != null)
                            {
                                currentVersion = JsonConvert.DeserializeObject<PublishFileModel>(thisPublishFileModelJson);
                                if (currentVersion != null && VersionCompare(currentVersion.Version.Version, _publishFileModel.Version.Version))
                                {
                                    Application.Current.MainWindow.Dispatcher.Invoke(() =>
                                    {
                                        Xceed.Wpf.Toolkit.MessageBox.ShowSingle(Icon, "当前已是最新版本");
                                        Close();
                                    });
                                    return;
                                }
                            }
                        }
                        Dispatcher.Invoke(() =>
                        {
                            var update = new UpdateAvailableWindow(currentVersion,_publishFileModel);
                            update.Show();
                            Close();
                        });
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (HttpRequestException)
            {
                //Xceed.Wpf.Toolkit.MessageBox.Show(this,"网络请求")
            }
        }

        private bool VersionCompare(string oldVersion, string newVersion)
        {
            var vOld = new Version(oldVersion);
            var vNew = new Version(newVersion);
            return vOld.CompareTo(vNew) >= 0;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
