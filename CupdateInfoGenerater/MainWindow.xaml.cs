using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.CheckTreeItem;
using CheckBox = Xceed.Wpf.Toolkit.CheckBox;
using Orientation = System.Windows.Controls.Orientation;

namespace CupdateInfoGenerater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Xceed.Wpf.Toolkit.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FolderPathTB.Text = AppDomain.CurrentDomain.BaseDirectory;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs? e)
        {
            FilesListView.Items.Clear();
            var path = Directory.CreateDirectory(FolderPathTB.Text);
            var rootCheckTree = new CheckTreeItem()
            {
                Header = "全选",
                IsExpanded = true,
            };
            ListFiles(rootCheckTree, path);
            FilesListView.Items.Add(rootCheckTree);
        }

        private void ListFiles(CheckTreeItem listView, DirectoryInfo path)
        {
            var multiBinding = new MultiBinding();
            if (path != null)
            {
                foreach (var folder in path.GetDirectories())
                {
                    var subListView = new CheckTreeItem()
                    {
                        Header = folder.Name,
                    };
                    ListFiles(subListView, folder);
                    listView.Items.Add(subListView);
                }

                foreach (var file in path.GetFiles())
                {
                    var stackPanel = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                    };
                    var cb = new CheckBox();
                    var tb = new TextBlock()
                    {
                        Text = file.Name,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    stackPanel.Children.Add(cb);
                    stackPanel.Children.Add(tb);
                    listView.Items.Add(stackPanel);
                }
            }
            BindingOperations.SetBinding(listView,, multiBinding);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new FolderBrowserDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var folderPath = openFileDialog.SelectedPath;
                FolderPathTB.Text = folderPath;
                MainWindow_Loaded(null, null);
            }
        }
    }
}