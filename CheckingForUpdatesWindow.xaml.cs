using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
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
                        if (!response.IsSuccessStatusCode)
                            Dispatcher.Invoke(() =>
                            {
                                Xceed.Wpf.Toolkit.MessageBox.Show(this, "网络请求错误,请稍候再试");
                                Close();
                            });
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
                            var update = new UpdateAvailableWindow(currentVersion, _publishFileModel);
                            update.Show();
                            Close();
                        });
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Dispatcher.Invoke(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(this, "网络请求错误,请稍候再试");
                    Close();
                });
            }
            catch (HttpRequestException)
            {
                Dispatcher.Invoke(() =>
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(this, "网络请求错误,请稍候再试");
                    Close();
                });
            }
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public static async Task<Tuple<PublishFileModel, PublishFileModel>> CheckUpdateBackgroundAsync(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(AppModel.URL + "app.json", HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token))
                    {
                        if (!response.IsSuccessStatusCode) return null;
                        PublishJsonText = await response.Content.ReadAsStringAsync();
                        var publishFileModel = JsonConvert.DeserializeObject<PublishFileModel>(PublishJsonText);
                        PublishFileModel currentVersion = null;
                        if (File.Exists("../app.json"))
                        {
                            var thisPublishFileModelJson = File.ReadAllText("../app.json");
                            if (thisPublishFileModelJson != null)
                            {
                                currentVersion = JsonConvert.DeserializeObject<PublishFileModel>(thisPublishFileModelJson);
                                if (currentVersion != null && VersionCompare(currentVersion.Version.Version, publishFileModel.Version.Version)) return null;
                            }
                        }
                        return new Tuple<PublishFileModel, PublishFileModel>(currentVersion, publishFileModel);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        private static bool VersionCompare(string oldVersion, string newVersion)
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
