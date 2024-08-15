using System.Windows;

namespace CUpdater
{
    /// <summary>
    /// Interaction logic for DownloadProgressWindow.xaml.
    /// 
    /// Window that shows while SparkleUpdater is downloading the update
    /// for the user.
    /// </summary>
    public partial class DownloadProgressWindow : BaseWindow
    {
        private bool _didCallDownloadProcessCompletedHandler = false;

        /// <summary>
        /// Base constructor for DownloadProgressWindow. Initializes the window
        /// and sets up the Closing event.
        /// </summary>
        public DownloadProgressWindow() : base(false)
        {
            InitializeComponent();
            Closing += DownloadProgressWindow_Closing;
        }

        private void DownloadProgressWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_didCallDownloadProcessCompletedHandler)
            {
                _didCallDownloadProcessCompletedHandler = true;
            }
            Closing -= DownloadProgressWindow_Closing;
            if (!_isOnMainThread && !_hasInitiatedShutdown)
            {
                _hasInitiatedShutdown = true;
                Dispatcher.InvokeShutdown();
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            _didCallDownloadProcessCompletedHandler = true;
        }
    }
}
