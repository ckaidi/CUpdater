using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CUpdater
{
    /// <summary>
    /// Interaction logic for UpdateAvailableWindow.xaml.
    /// 
    /// Window that shows the list of available updates to the user
    /// </summary>
    public partial class UpdateAvailableWindow : BaseWindow
    {
        private bool _hasFinishedNavigatingToAboutBlank = false;
        private string _notes = "";
        private bool _wasResponseSent = false;
        private PublishFileModel _currentApp;
        private PublishFileModel _lastestApp;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private List<FileHash> _updaterFile = new List<FileHash>();

        /// <summary>
        /// Initialize the available update window with no initial date context
        /// (and thus no initial information on downloadable releases to show
        /// to the user)
        /// </summary>
        public UpdateAvailableWindow(PublishFileModel currentVersion, PublishFileModel lastestVersion) : base(true)
        {
            _currentApp = currentVersion;
            _lastestApp = lastestVersion;
            DataContext = _lastestApp;
            InitializeComponent();
            TitleHeader.Text = $"检测到新版本:{_lastestApp.Version.Version}";
            GuiDB.DBExtensions.UseTheScrollViewerScrolling(MainMarkdownViewer);
        }

        /// <summary>
        /// Change the main grid's background color. Use new SolidColorBrush(Colors.Transparent) or null to clear.
        /// </summary>
        /// <param name="brush">Brush to use as the main grid's background color</param>
        public void ChangeMainGridBackgroundColor(System.Windows.Media.Brush brush)
        {
            if (MainGrid != null) MainGrid.Background = brush;
        }

        private void SkipButtonOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RemindMeLaterOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 下载最新版本的版本
        /// </summary>
        public static async Task DownloadNewVersionAppAsync(PublishFileModel currentApp, PublishFileModel lastestApp)
        {
            var updaterFolder = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var downloadPath = Path.Combine(updaterFolder.FullName, lastestApp.Version.Version);
            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);
            else
                return;
            var latestFileDict = lastestApp.Files.ToDictionary(x => x.Name);
            var newUpdateFile = new List<FileHash>();
            var installPath = updaterFolder.Parent;
            if (currentApp != null)
            {
                var currentFileDict = currentApp.Files.ToDictionary(x => x.Name);
                foreach (var filePath in latestFileDict.Keys)
                {
                    if (File.Exists(Path.Combine(installPath.FullName, filePath.Replace("./", ""))))
                        if (currentFileDict.ContainsKey(filePath))
                        {
                            var currentFile = currentFileDict[filePath];
                            var newFile = latestFileDict[filePath];
                            if (currentFile.Hash == newFile.Hash) continue;
                        }
                    newUpdateFile.Add(latestFileDict[filePath]);
                }
            }
            else newUpdateFile = latestFileDict.Values.ToList();
            var newDownloadFile = newUpdateFile.Where(x => !File.Exists(Path.Combine(downloadPath, x.Name))).ToArray();
            if (newDownloadFile.Length > 0)
            {
                // 下载所需文件
                var allFilesSizeMB = newDownloadFile.Sum(x => x.Size) / 1024d;
                double totalRead = 0;
                foreach (var fileHash in newDownloadFile)
                {
                    var startTotal = totalRead + fileHash.Size * 1024;
                    var outputPath = fileHash.FullName(downloadPath);
                    if (File.Exists(outputPath)) continue;
                    var folder = Directory.GetParent(outputPath);
                    if (!folder.Exists) folder.Create();
                    using (var httpClient = new HttpClient())
                    {
                        using (var response = await httpClient.GetAsync(AppModel.URL + fileHash.Name.Replace("./", ""), HttpCompletionOption.ResponseHeadersRead))
                        {
                            if (!response.IsSuccessStatusCode) return;
                            response.EnsureSuccessStatusCode();
                            using (var contentStream = await response.Content.ReadAsStreamAsync())
                            {
                                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                                {
                                    var buffer = new byte[8192];
                                    var isMoreToRead = true;
                                    double percentage;
                                    do
                                    {
                                        var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                                        if (read == 0) isMoreToRead = false;
                                        else
                                        {
                                            await fileStream.WriteAsync(buffer, 0, read);
                                            totalRead += read;
                                            percentage = (double)totalRead / (1024 * 1024) / allFilesSizeMB * 100;
                                            totalRead = Math.Min(totalRead, startTotal);
                                        }
                                    }
                                    while (isMoreToRead);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void DownloadInstallOnClick(object sender, RoutedEventArgs e)
        {
            if (App.AppProcess != null && !App.AppProcess.HasExited)
            {
                App.AppProcess.Kill();
                Thread.Sleep(1000);
            }
            var updaterFolder = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var tempPath = Path.GetTempPath();
            var versionHash = ComputeSha256Hash(CheckingForUpdatesWindow.PublishJsonText);
            var downloadPath = Path.Combine(tempPath, versionHash);
            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);
            var latestFileDict = _lastestApp.Files.ToDictionary(x => x.Name);
            var newUpdateFile = new List<FileHash>();
            var installPath = updaterFolder.Parent;
            if (_currentApp != null)
            {
                var currentFileDict = _currentApp.Files.ToDictionary(x => x.Name);
                foreach (var filePath in latestFileDict.Keys)
                {
                    if (File.Exists(Path.Combine(installPath.FullName, filePath.Replace("./", ""))))
                        if (currentFileDict.ContainsKey(filePath))
                        {
                            var currentFile = currentFileDict[filePath];
                            var newFile = latestFileDict[filePath];
                            if (currentFile.Hash == newFile.Hash) continue;
                        }
                    newUpdateFile.Add(latestFileDict[filePath]);
                }
            }
            else newUpdateFile = latestFileDict.Values.ToList();
            var newDownloadFile = newUpdateFile.Where(x => !File.Exists(Path.Combine(downloadPath, x.Name))).ToArray();
            if (newDownloadFile.Length > 0)
            {
                // 下载所需文件
                var allFilesSizeMB = newDownloadFile.Sum(x => x.Size) / 1024d;
                DownloadProgressBar.Visibility = Visibility.Visible;
                DownloadProgressBar.Value = 0;
                double totalRead = 0;
                foreach (var fileHash in newDownloadFile)
                {
                    var startTotal = totalRead + fileHash.Size * 1024;
                    var outputPath = fileHash.FullName(downloadPath);
                    var folder = Directory.GetParent(outputPath);
                    if (!folder.Exists) folder.Create();
                    using (var httpClient = new HttpClient())
                    {
                        using (var response = await httpClient.GetAsync(AppModel.URL + fileHash.Name.Replace("./", ""), HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token))
                        {
                            response.EnsureSuccessStatusCode();
                            using (var contentStream = await response.Content.ReadAsStreamAsync())
                            {
                                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                                {
                                    var buffer = new byte[8192];
                                    var isMoreToRead = true;
                                    double percentage;
                                    do
                                    {
                                        var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                                        if (read == 0) isMoreToRead = false;
                                        else
                                        {
                                            await fileStream.WriteAsync(buffer, 0, read);
                                            totalRead += read;
                                            percentage = (double)totalRead / (1024 * 1024) / allFilesSizeMB * 100;
                                            totalRead = Math.Min(totalRead, startTotal);
                                            Dispatcher.Invoke(() =>
                                            {
                                                DownloadProgressBar.Value = percentage;
                                                DownloadProgressTB.Text = $"正在下载  {totalRead / 1024 / 1024:f2}MB/{allFilesSizeMB:f2}MB({DownloadProgressBar.Value:f2}%)";
                                            });
                                        }
                                    }
                                    while (isMoreToRead && !_cancellationTokenSource.IsCancellationRequested);
                                }
                            }
                        }
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = 100;
                    DownloadProgressTB.Text = $"正在下载  {allFilesSizeMB:f2}MB/{allFilesSizeMB:f2}MB(100%)";
                });
            }
            _updaterFile = newUpdateFile.Where(x => x.Name.StartsWith("./Updater")).ToList();
            newUpdateFile = newUpdateFile.Where(x => !x.Name.StartsWith("./Updater")).ToList();
            foreach (var file in newUpdateFile)
            {
                var installPa = file.FullName(installPath.FullName);
                if (File.Exists(installPa))
                {
                    if (IsFileLocked(installPa))
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show(this, $"{installPa}文件被占用,请稍候再试");
                        return;
                    }
                }
            }
            Dispatcher.Invoke(() =>
            {
                DownloadProgressBar.Visibility = Visibility.Visible;
                DownloadProgressBar.Value = 0;
                DownloadProgressTB.Text = $"正在安装  {0}/{newUpdateFile.Count}(0%)";
            });
            double index = 0;
            foreach (var file in newUpdateFile)
            {
                var installPa = file.FullName(installPath.FullName);
                if (File.Exists(installPa))
                {
                    File.Delete(installPa);
                }
                var folderName = Path.GetDirectoryName(file.FullName(installPath.FullName));
                var folder = Directory.CreateDirectory(folderName);
                if (!folder.Exists)
                    folder.Create();
                File.Copy(file.FullName(downloadPath), file.FullName(installPath.FullName));
                index += 1;
                double percent = index / newUpdateFile.Count * 100;
                Dispatcher.Invoke(() =>
                {
                    DownloadProgressBar.Value = percent;
                    DownloadProgressTB.Text = $"正在安装  {index}/{newUpdateFile.Count}({percent}%)";
                });
            }
            var json = JsonConvert.SerializeObject(_lastestApp, Formatting.Indented);
            File.WriteAllText(Path.Combine(installPath.FullName, "app.json"), json);
            var p = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName);
            Process.Start(Path.Combine(p.FullName, AppModel.StartUpApp));
            Close();
        }

        /// <summary>
        /// 检查文件是否被占用
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static bool IsFileLocked(string filePath)
        {
            try
            {
                // 尝试以独占模式打开文件
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                // 如果出现IO异常，认为文件可能被占用
                return true;
            }
            // 如果没有异常，认为文件没有被占用
            return false;
        }

        // 使用 SHA256 计算字符串的哈希值
        public static string ComputeSha256Hash(string rawData)
        {
            // 创建一个 SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // 计算字符串的哈希值
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // 将字节转换为一个十六进制字符串
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
