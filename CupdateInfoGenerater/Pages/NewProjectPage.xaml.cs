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
        }

        /// <summary>
        /// 选取打包路径
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
    }
}
