using CupdateInfoGenerater;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CupdateInfoGenerater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Xceed.Wpf.Toolkit.Window
    {
        private readonly NewProjectPage _projectPage;
        private readonly PackagePage _packagePage;

        public MainWindow()
        {
            _projectPage = new NewProjectPage(this);
            _packagePage = new PackagePage(this);
            Icon = BitmapFrame.Create(new Uri("pack://application:,,,/icon-gray.ico"));
            InitializeComponent();
            MainFrame.Navigate(_projectPage);
        }

        public void OpenFolder()
        {
            MainFrame.Navigate(_packagePage);
        }

        public void CloseProject()
        {
            MainFrame.Navigate(_projectPage);
        }
    }
}