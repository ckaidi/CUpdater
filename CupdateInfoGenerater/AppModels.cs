using System.Collections.ObjectModel;
using Xceed.Wpf.Toolkit.Core;

namespace CupdateInfoGenerater
{
    internal class AppModels : ViewModelBase
    {
        public ObservableCollection<Filters> AllFilters { get; } = [];

        public PackFolderModels? PackFolder { get; }
    }

    public class PackFolderModels : ViewModelBase
    {
        PackFolderModels[]? SubFolders { get; set; }
        string[]? Files { get; set; }
    }

    public class Filters : ViewModelBase
    {
        private string _name;
        private bool _isChecked;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public Filters(string name)
        {
            _name = name;
        }
    }
}
