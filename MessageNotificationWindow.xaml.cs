using System.Windows;

namespace CUpdater
{
    /// <summary>
    /// Interaction logic for MessageNotificationWindow.xaml.
    /// 
    /// Window that shows a single message to the user (usually an error) regarding
    /// a software update.
    /// </summary>
    public partial class MessageNotificationWindow
    {
        /// <summary>
        /// Construct the notification window for the message notification with the default
        /// <seealso cref="MessageNotificationWindowViewModel"/>.
        /// </summary>
        public MessageNotificationWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
