using Page = System.Windows.Controls.Page;
using System.Windows;

namespace CupdateInfoGenerater
{
    /// <summary>
    /// Interaction logic for NewProjectPage.xaml
    /// </summary>
    public partial class NewProjectPage : Page
    {
        private MainWindow _mainWindow;

        public NewProjectPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            IconImage.Source = mainWindow.ChromeIcon;
        }

        /// <summary>
        /// 打开指定文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFolderButtonClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new FolderBrowserDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var folderPath = openFileDialog.SelectedPath;
                PackagePage.AppModel.Path = folderPath;
                _mainWindow.OpenFolder();
            }
        }

        /// <summary>
        /// 打开项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenProjectButtonClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "配置文件夹|*.json",
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _mainWindow.SaveProject(openFileDialog.FileName);
            }
        }
    }
}
