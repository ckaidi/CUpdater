using UserControl = System.Windows.Controls.UserControl;
using CheckBox = Xceed.Wpf.Toolkit.CheckBox;
using Xceed.Wpf.Toolkit;

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

        public CheckItem(string name, CheckBoxState isChecked)
        {
            InitializeComponent();
            CheckBox = _checkbox;
            DataContext = new Filters(name, isChecked);
        }
    }
}
