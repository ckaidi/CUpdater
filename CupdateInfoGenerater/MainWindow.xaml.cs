using Newtonsoft.Json;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.CheckTreeItem;
using Binding = System.Windows.Data.Binding;
using CheckBox = Xceed.Wpf.Toolkit.CheckBox;
using TabControl = System.Windows.Controls.TabControl;

namespace CupdateInfoGenerater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Xceed.Wpf.Toolkit.Window
    {
        private AppModels _appModels = new();
        private bool[] _lastFilter = [];

        public MainWindow()
        {
            DataContext = _appModels;
            InitializeComponent();
            FolderPathTB.Text = AppDomain.CurrentDomain.BaseDirectory;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs? e)
        {
            LoadPackagePath();
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

                for (int i = FilterWrapPanel.Children.Count; i < _appModels.AllFilters.Count; i++)
                {
                    var item = _appModels.AllFilters[i];
                    var ci = new CheckItem(item)
                    {
                        Margin = new Thickness(5, 0, 5, 0)
                    };
                    var binding = new Binding(nameof(Filters.IsChecked))
                    {
                        Source = item,
                        Mode = BindingMode.TwoWay,
                    };
                    BindingOperations.SetBinding(ci.CheckBox, CheckBox.IsCheckedProperty, binding);
                    FilterWrapPanel.Children.Add(ci);
                }
            }
        }

        /// <summary>
        /// 读取打包路径
        /// </summary>
        private void LoadPackagePath()
        {
            var path = Directory.CreateDirectory(FolderPathTB.Text);
            var pf = new PackFolderModels("根目录");
            pf.AllFileCount = ListFiles(pf, path);
            _appModels.PackFolder = pf;
            LoadPackFolder(pf);
        }

        private int ListFiles(PackFolderModels rootFolder, DirectoryInfo path)
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
                var extHash = _appModels.AllFilters.Where(x => x.IsChecked).Select(x => x.Name).ToHashSet();
                foreach (var file in files)
                {
                    var extensions = Path.GetExtension(file.FullName);
                    if (!_appModels.AllFilters.Any(x => x.Name == extensions))
                        _appModels.AllFilters.Add(new Filters(extensions, false));
                    rootFolder.Files.Add(new Filters(file.Name, true));
                }
                result += files.Length;
            }
            return result;
        }

        private void CreateFilesControl(CheckTreeItem listView, PackFolderModels path)
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
                var extHash = _appModels.AllFilters.Where(x => x.IsChecked).Select(x => x.Name).ToHashSet();
                foreach (var file in files)
                {
                    var extensions = Path.GetExtension(file.Name);
                    if (!_appModels.AllFilters.Any(x => x.Name == extensions))
                        _appModels.AllFilters.Add(new Filters(extensions, false));
                    var ci = new CheckItem(file);
                    listView.Items.Add(ci);
                    var binding = new Binding(nameof(CheckBox.State))
                    {
                        Source = ci.CheckBox,
                        Mode = BindingMode.TwoWay,
                    };
                    multiBinding.Bindings.Add(binding);
                }
            }
            BindingOperations.SetBinding(listView, CheckTreeItem.StateProperty, multiBinding);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new FolderBrowserDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var folderPath = openFileDialog.SelectedPath;
                FolderPathTB.Text = folderPath;
                LoadPackagePath();
            }
        }

        private void TabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem tabItem)
            {
                if (tabItem.Header.ToString() == "文件")
                {
                    var filterChecks = _appModels.AllFilters.Select(x => x.IsChecked).ToArray();
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
                        var extExclued = _appModels.AllFilters.Where(x => x.IsChecked).Select(x => x.Name).ToHashSet();
                        foreach (var item in rootItem.Items)
                        {
                            switch (item)
                            {
                                case CheckItem ci when ci.DataContext is Filters checkItemModel:
                                    var ext = Path.GetExtension(checkItemModel.Name);
                                    if (extExclued.Contains(ext))
                                        checkItemModel.IsChecked = false;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                else if (tabItem.Header.ToString() == "过滤器")
                {
                    _lastFilter = _appModels.AllFilters.Select(x => x.IsChecked).ToArray();
                }
            }
        }

        private void GenerateButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void OpenButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "*.json|*.json",
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var json = File.ReadAllText(dialog.FileName);
                var r = JsonConvert.DeserializeObject<AppModels>(json);
                if (r == null)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(this, "文件读取错误");
                    return;
                }
                _appModels = r;
                if (_appModels.PackFolder != null)
                {
                    FilterWrapPanel.Children.Clear();
                    LoadPackFolder(_appModels.PackFolder);
                }
            }
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                FileName = "untitled.json",
                Filter = "*.json|*.json",
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var json = JsonConvert.SerializeObject(_appModels, Formatting.Indented);
                File.WriteAllText(dialog.FileName, json);
            }
        }
    }
}