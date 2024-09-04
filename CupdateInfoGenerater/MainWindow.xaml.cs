using CupdateInfoGenerater;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static Xceed.Wpf.Toolkit.ByteArrayExtensions;

namespace CupdateInfoGenerater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Xceed.Wpf.Toolkit.Window
    {
        private readonly NewProjectPage _projectPage;
        private readonly PackagePage _packagePage;
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            _projectPage = new NewProjectPage(this);
            _packagePage = new PackagePage(this);
            var iconGray = Properties.Resources.ResourceManager.GetObject("icon-png");
            if (iconGray is byte[] buffer)
                _projectPage.IconImage.Source = buffer.ByteArrayToBitmapImage();
            InitializeComponent();
            MainFrame.Navigate(_projectPage);
            Instance = this;
        }

        public void OpenFolder()
        {
            _packagePage.IsLoadPathWhenPageLoad = true;
            MainFrame.Navigate(_packagePage);
        }

        public void CloseProject()
        {
            MainFrame.Navigate(_projectPage);
        }

        /// <summary>
        /// 打开已保存的项目
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveProject(string fileName)
        {
            _packagePage.IsLoadPathWhenPageLoad = false;
            _packagePage.OpenSaveProject(fileName);
            MainFrame.Navigate(_packagePage);
        }
    }
}