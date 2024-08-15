using System.Linq;
using System.Windows;
using System.Windows.Navigation;

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

        /// <summary>
        /// Initialize the available update window with no initial date context
        /// (and thus no initial information on downloadable releases to show
        /// to the user)
        /// </summary>
        public UpdateAvailableWindow() : base(true)
        {
            InitializeComponent();
        }

        private void UpdateAvailableWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ReleaseNotesBrowser.Navigated -= ReleaseNotesBrowser_Navigated;
            Closing -= UpdateAvailableWindow_Closing;
        }

        /// <summary>
        /// Change the main grid's background color. Use new SolidColorBrush(Colors.Transparent) or null to clear.
        /// </summary>
        /// <param name="brush">Brush to use as the main grid's background color</param>
        public void ChangeMainGridBackgroundColor(System.Windows.Media.Brush brush)
        {
            if (MainGrid != null)
            {
                MainGrid.Background = brush;
            }
        }

        /// <summary>
        /// Show the release notes to the end user. Release notes should be in HTML.
        /// 
        /// There is some bizarre thing where the WPF browser doesn't navigate to the release notes unless you successfully navigate to
        /// about:blank first. I don't know why. I feel like this is a Terrible Bad Fix, but...it works for now...
        /// </summary>
        /// <param name="htmlNotes">The HTML notes to show to the end user</param>
        public void ShowReleaseNotes(string htmlNotes)
        {
            _notes = htmlNotes;
            ReleaseNotesBrowser.Dispatcher.Invoke(() =>
            {

                if (ReleaseNotesBrowser.IsLoaded)
                {
                    if (_hasFinishedNavigatingToAboutBlank)
                    {
                        ReleaseNotesBrowser.NavigateToString(_notes);
                    }
                    // else will catch up when navigating to about:blank is done
                }
                else
                {
                    // don't do anything until the web browser is loaded
                    ReleaseNotesBrowser.Loaded += ReleaseNotesBrowser_Loaded;
                }
            });
        }

        private void ReleaseNotesBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            // see https://stackoverflow.com/a/15209861/3938401
            ReleaseNotesBrowser.Loaded -= ReleaseNotesBrowser_Loaded;
            ReleaseNotesBrowser.Dispatcher.Invoke(() =>
            {
                ReleaseNotesBrowser.NavigateToString("about:blank");
            });
        }

        private void ReleaseNotesBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            if (!_hasFinishedNavigatingToAboutBlank)
            {
                ReleaseNotesBrowser.NavigateToString(_notes);
                _hasFinishedNavigatingToAboutBlank = true;
            }
        }
    }
}
