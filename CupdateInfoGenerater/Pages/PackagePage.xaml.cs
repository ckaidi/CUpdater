//using CUpdater;
using Mono.Cecil;
using Newtonsoft.Json;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Resources;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.CheckTreeItem;
using Binding = System.Windows.Data.Binding;
using CheckBox = Xceed.Wpf.Toolkit.CheckBox;
using Page = Xceed.Wpf.Toolkit.Page;
using TabControl = System.Windows.Controls.TabControl;

namespace CupdateInfoGenerater
{
    /// <summary>
    /// Interaction logic for PackagePage.xaml
    /// </summary>
    public partial class PackagePage : Page
    {
        internal static AppModels AppModel = new();
        private CheckBoxState[] _lastFilter = [];
        private MainWindow _window;

        /// <summary>
        /// 是否在页面加载的时候自动加载路径
        /// </summary>
        public bool IsLoadPathWhenPageLoad = true;

        public PackagePage(MainWindow owner)
        {
            DataContext = AppModel;
            _window = owner;
            InitializeComponent();
            Loaded += MainPageLoaded;
            GuiDB.DBExtensions.UseTheScrollViewerScrolling(MainMarkdownViewer);
        }

        private void TabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem tabItem)
            {
                if (tabItem.Header.ToString() == "文件")
                {
                    var filterChecks = AppModel.AllFilters.Select(x => x.IsChecked).ToArray();
                    var isUpdate = false;
                    if (_lastFilter.Length != filterChecks.Length) isUpdate = true;
                    for (int i = 0; i < _lastFilter.Length; i++)
                    {
                        if (_lastFilter[i] != filterChecks[i])
                        {
                            isUpdate = true;
                            break;
                        }
                    }
                    if (isUpdate && FilesListView.Items.Count > 0 && FilesListView.Items[0] is CheckTreeItem rootItem)
                    {
                        var extExclued = AppModel.AllFilters.Where(x => x.IsChecked == CheckBoxState.Checked).Select(x => x.Name).ToHashSet();
                        UnCheckedFilters(extExclued, rootItem);
                    }
                }
                else if (tabItem.Header.ToString() == "过滤器") _lastFilter = AppModel.AllFilters.Select(x => x.IsChecked).ToArray();
            }
        }

        private static void UnCheckedFilters(HashSet<string> filters, CheckTreeItem rootItem)
        {
            foreach (var item in rootItem.Items)
            {
                switch (item)
                {
                    case CheckItem ci when ci.DataContext is Filters checkItemModel:
                        var ext = Path.GetExtension(checkItemModel.Name);
                        if (filters.Contains(ext)) checkItemModel.IsChecked = CheckBoxState.UnChecked;
                        break;
                    case CheckTreeItem checkTreeItem:
                        UnCheckedFilters(filters, checkTreeItem);
                        break;
                    default:
                        break;
                }
            }
        }

        private void LoadPackFolder(PackFolderModels pf)
        {
            FilesListView.Items.Clear();
            if (pf.AllFileCount == 0)
            {
                MainScrollViewer.Visibility = Visibility.Collapsed;
                TipsTB.Visibility = Visibility.Visible;
                TipsTB.Text = "所选文件夹中没有任何文件!";
            }
            else
            {
                var rootCheckTree = new CheckTreeItem()
                {
                    Header = pf.FolderName,
                    IsExpanded = true,
                };
                CreateFilesControl(rootCheckTree, pf);
                TipsTB.Visibility = Visibility.Collapsed;
                MainScrollViewer.Visibility = Visibility.Visible;
                FilesListView.Items.Add(rootCheckTree);

                for (int i = FilterWrapPanel.Children.Count; i < AppModel.AllFilters.Count; i++)
                {
                    var item = AppModel.AllFilters[i];
                    var ci = new CheckItem(item)
                    {
                        Margin = new Thickness(5, 0, 5, 0)
                    };
                    var binding = new Binding(nameof(Filters.IsChecked))
                    {
                        Source = item,
                        Mode = BindingMode.TwoWay,
                    };
                    BindingOperations.SetBinding(ci.CheckBox, CheckBox.StateProperty, binding);
                    FilterWrapPanel.Children.Add(ci);
                }
            }
        }

        /// <summary>
        /// 加载所选取的需要打包的路径
        /// </summary>
        private void LoadPackagePath()
        {
            if (string.IsNullOrEmpty(AppModel?.Path)) return;
            var path = Directory.CreateDirectory(AppModel.Path);
            var pf = new PackFolderModels("根目录");
            pf.AllFileCount = ListFiles(pf, path);
            AppModel.PackFolder = pf;
            LoadPackFolder(pf);
        }

        /// <summary>
        /// 枚举文件夹中的文件
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private static int ListFiles(PackFolderModels rootFolder, DirectoryInfo path)
        {
            var result = 0;
            if (path != null)
            {
                foreach (var folder in path.GetDirectories())
                {
                    var subListView = new PackFolderModels(folder.Name);
                    subListView.AllFileCount = ListFiles(subListView, folder);
                    result += subListView.AllFileCount;
                    rootFolder.SubFolders.Add(subListView);
                }
                var files = path.GetFiles();
                var extHash = AppModel.AllFilters.Where(x => x.IsChecked == CheckBoxState.Checked).Select(x => x.Name).ToHashSet();
                foreach (var file in files)
                {
                    var extensions = Path.GetExtension(file.FullName);
                    if (!AppModel.AllFilters.Any(x => x.Name == extensions))
                        AppModel.AllFilters.Add(new Filters(extensions, CheckBoxState.UnChecked));
                    rootFolder.Files.Add(new Filters(file.Name, CheckBoxState.Checked));
                }
                result += files.Length;
            }
            return result;
        }

        private static void CreateFilesControl(CheckTreeItem listView, PackFolderModels path, bool ignoreFilter = false)
        {
            var multiBinding = new MultiBinding()
            {
                Converter = new ChechBoxMultiBinding(),
                Mode = BindingMode.TwoWay,
            };
            if (path != null)
            {
                foreach (var folder in path.SubFolders.Where(x => x.AllFileCount != 0))
                {
                    var subListView = new CheckTreeItem()
                    {
                        Header = folder.FolderName,
                    };
                    CreateFilesControl(subListView, folder);
                    listView.Items.Add(subListView);
                    var binding = new Binding(nameof(CheckTreeItem.State))
                    {
                        Source = subListView,
                        Mode = BindingMode.TwoWay,
                    };
                    multiBinding.Bindings.Add(binding);
                }
                var files = path.Files;
                foreach (var file in files)
                {
                    var extensions = Path.GetExtension(file.Name);
                    if (!AppModel.AllFilters.Any(x => x.Name == extensions))
                        AppModel.AllFilters.Add(new Filters(extensions, CheckBoxState.UnChecked));
                    var ci = new CheckItem(file);
                    listView.Items.Add(ci);
                    var binding = new Binding(nameof(CheckBox.State))
                    {
                        Source = ci.CheckBox,
                        Mode = BindingMode.TwoWay,
                    };
                    var stateBinding = new Binding(nameof(Filters.IsChecked))
                    {
                        Source = file,
                        Mode = BindingMode.TwoWay,
                    };
                    BindingOperations.SetBinding(ci.CheckBox, CheckBox.StateProperty, stateBinding);
                    multiBinding.Bindings.Add(binding);
                }
            }
            BindingOperations.SetBinding(listView, CheckTreeItem.StateProperty, multiBinding);
        }

        private void MainPageLoaded(object? sender, RoutedEventArgs? e)
        {
            if (IsLoadPathWhenPageLoad)
                LoadPackagePath();
        }

        /// <summary>
        /// 生成打包的hash文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateButtonClick(object sender, RoutedEventArgs e)
        {
            if (!(AppModel?.Path != null && AppModel?.PackFolder != null && AppModel.VersionInfo != null))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(_window, "请选择需要打包的文件夹");
                return;
            }

            var openFileDialog = new FolderBrowserDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var folderPath = openFileDialog.SelectedPath;
                var appJsonFolder = Path.Join(folderPath, "package");

                // 复制更新包
                var directoryPath = AppDomain.CurrentDomain.BaseDirectory;
                FolderUtils.CopyDirectory(Path.Join(directoryPath, "Updater"), Path.Join(appJsonFolder, "Updater"));

                // 生成app.json
                var appJsonPath = Path.Join(AppModel.Path, "app.json");
                if (Directory.Exists(appJsonFolder)) Directory.Delete(appJsonFolder, true);
                Directory.CreateDirectory(appJsonFolder);
                var result = new PublishFileModel(new PublishVersion(AppModel.VersionInfo.Version));
                if (AppModel.VersionInfo.Description != null && File.Exists(AppModel.VersionInfo.Description))
                    result.Version.Description = File.ReadAllText(AppModel.VersionInfo.Description);

                var isSuccess = CalculateFileHash(result.Files, AppModel.PackFolder, "./");
                if (isSuccess)
                {
                    var json = JsonConvert.SerializeObject(result, Formatting.Indented);
                    File.WriteAllText(appJsonPath, json);

                    // 生成压缩包
                    ZipFolder(Path.Join(appJsonFolder, "app.zip"), AppModel.Path);

                    // 生成安装包
                    FolderUtils.CopyDirectory(Path.Join(directoryPath, "Installer"), Path.Join(appJsonFolder, "Installer"));

                    ReplaceResource(Path.Join(Path.Join(appJsonFolder, "Installer"), "CInstaller.exe"),
                        "CInstaller.Properties.Resources.resources", "app", Path.Join(appJsonFolder, "app.zip"));
                    Xceed.Wpf.Toolkit.MessageBox.Show(MainWindow.Instance, "打包完成");
                    Process.Start("appJsonFolder");
                }
            }
        }

        private static void ReplaceResource(string dllPath, string resourceFileName, string resourceName, string newResourcePath)
        {
            using var assembly = AssemblyDefinition.ReadAssembly(dllPath, new ReaderParameters { ReadWrite = true });
            var resource = assembly.MainModule.Resources.OfType<EmbeddedResource>().FirstOrDefault(r => r.Name == resourceFileName);

            if (resource == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(MainWindow.Instance, $"Resource '{resourceFileName}' not found in the assembly.");
                return;
            }

            using var resourceStream = resource.GetResourceStream();
            using var newResourceStream = new FileStream(newResourcePath, FileMode.Open, FileAccess.Read);
            using var reader = new ResourceReader(resourceStream);
            using var writerStream = new MemoryStream();
            using var writer = new ResourceWriter(writerStream);
            foreach (DictionaryEntry entry in reader)
            {
                var key = entry.Key?.ToString();
                if (key == null) continue;
                if (key == resourceName)
                {
                    // 用新的资源替换
                    writer.AddResource(key, newResourceStream);
                }
                else
                {
                    writer.AddResource(key, entry.Value);
                }
            }

            writer.Generate();
            writerStream.Seek(0, SeekOrigin.Begin);

            var newEmbeddedResource = new EmbeddedResource(resourceFileName, resource.Attributes, writerStream);
            assembly.MainModule.Resources.Remove(resource);
            assembly.MainModule.Resources.Add(newEmbeddedResource);

            assembly.Write();
        }

        /// <summary>
        /// 压缩文件夹到压缩文件
        /// </summary>
        /// <param name="zipPath">压缩包的路径</param>
        /// <param name="folderPath">要添加的文件夹路径</param>
        public static void ZipFolder(string zipPath, string folderPath)
        {
            using FileStream zipToOpen = new(zipPath, FileMode.OpenOrCreate);
            using ZipArchive archive = new(zipToOpen, ZipArchiveMode.Create);
            // 遍历文件夹中的所有文件和子文件夹
            foreach (string filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                // 计算压缩包内文件的路径
                string relativePath = Path.GetRelativePath(folderPath, filePath);
                relativePath = relativePath.Replace("\\", "/");  // 使用正斜杠作为路径分隔符

                // 创建一个新条目（代表压缩包内的文件）
                ZipArchiveEntry entry = archive.CreateEntry(relativePath);

                // 将文件内容写入压缩包内的文件
                using Stream entryStream = entry.Open();
                using FileStream fileToCompress = new(filePath, FileMode.Open, FileAccess.Read);
                fileToCompress.CopyTo(entryStream);
            }
        }

        /// <summary>
        /// 计算文件哈希值
        /// </summary>
        /// <param name="fileHashes"></param>
        /// <param name="packFolder"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        private bool CalculateFileHash(List<FileHash> fileHashes, PackFolderModels packFolder, string root)
        {
            foreach (var item in packFolder.Files)
            {
                if (item.IsChecked != CheckBoxState.Checked) continue;
                var filePath = Path.Join(AppModel.Path, Path.Join(root, item.Name));
                if (!File.Exists(filePath))
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(_window, $"{filePath}文件不存在");
                    return false;
                }
                var hash = ComputeFileHash(filePath);
                var fileInfo = new FileInfo(filePath);
                fileHashes.Add(new FileHash(Path.Join(root, item.Name), hash, fileInfo.Length / 1024));
            }

            foreach (var item in packFolder.SubFolders)
                if (!CalculateFileHash(fileHashes, item, Path.Join(root, item.FolderName)))
                    return false;
            return true;
        }

        /// <summary>
        /// 求相对路径
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        private static string CalculateRelativePath(string absolutePath, string basePath)
        {
            var baseUri = new Uri(basePath);
            var targetUri = new Uri(absolutePath);

            var relativeUri = baseUri.MakeRelativeUri(targetUri);
            return Uri.UnescapeDataString(relativeUri.ToString());
        }

        /// <summary>
        /// 计算文件hash
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ComputeFileHash(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant(); // 返回哈希值的十六进制字符串
        }

        /// <summary>
        /// 打开保存的文件
        /// </summary>
        /// <param name="fileName"></param>
        public void OpenSaveProject(string fileName)
        {
            var json = File.ReadAllText(fileName);
            var r = JsonConvert.DeserializeObject<AppModels>(json);
            if (r == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(_window, "文件读取错误");
                return;
            }
            AppModel = r;
            DataContext = AppModel;
            if (AppModel.PackFolder != null)
            {
                FilterWrapPanel.Children.Clear();
                LoadPackFolder(AppModel.PackFolder);
                _lastFilter = AppModel.AllFilters.Select(x => x.IsChecked).ToArray();
            }
        }

        /// <summary>
        /// 保存项目文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (AppModel?.VersionInfo == null)
                return;
            var dialog = new SaveFileDialog()
            {
                FileName = "untitled.json",
                Filter = "*.json|*.json",
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var json = JsonConvert.SerializeObject(AppModel, Formatting.Indented);
                File.WriteAllText(dialog.FileName, json);
            }
        }

        private void CloseProjectButtonClick(object sender, RoutedEventArgs e)
        {
            _window.CloseProject();
        }
    }
}
