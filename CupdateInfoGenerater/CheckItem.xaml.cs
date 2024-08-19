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
            InitializeComponent();
            CheckBox = _checkbox;
            DataContext = checkItemModel;
        }

        public CheckItem(string name, bool isChecked)
        {
            InitializeComponent();
            CheckBox = _checkbox;
            DataContext = new Filters(name, isChecked);
        }
    }
}
