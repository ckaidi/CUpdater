using UserControl = System.Windows.Controls.UserControl;
using CheckBox = Xceed.Wpf.Toolkit.CheckBox;

namespace CupdateInfoGenerater
{
    /// <summary>
    /// Interaction logic for CheckItem.xaml
    /// </summary>
    public partial class CheckItem : UserControl
    {
        public CheckBox CheckBox { get; }

        public CheckItem(Filters checkItemModel)
        {
            DataContext = checkItemModel;
            InitializeComponent();
            CheckBox = _checkbox;
        }

        public CheckItem(string name)
        {
            DataContext = new Filters(name);
            InitializeComponent();
            CheckBox = _checkbox;
        }
    }
}
