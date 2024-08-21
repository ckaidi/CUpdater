using CUpdater;
using System.Collections.ObjectModel;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Core;

namespace CupdateInfoGenerater
{
    internal class AppModels : ViewModelBase
    {
        private string? _path;

        /// <summary>
        /// 打包路径
        /// </summary>
        public string? Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 过滤器
        /// </summary>
        public ObservableCollection<Filters> AllFilters { get; } = [];

        /// <summary>
        /// 文件的嵌套结构
        /// </summary>
        public PackFolderModels? PackFolder { get; set; }

        /// <summary>
        /// 版本信息
        /// </summary>
        public PublishVersion? VersionInfo { get; set; } = new();
    }

    public class PackFolderModels : ViewModelBase
    {
        public string FolderName { get; }
        public int AllFileCount { get; set; }
        public List<PackFolderModels> SubFolders { get; } = [];
        public List<Filters> Files { get; } = [];

        public PackFolderModels(string folderName)
        {
            FolderName = folderName;
        }
    }

    public class Filters : ViewModelBase
    {
        private string _name;
        private CheckBoxState _isChecked;

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

        public CheckBoxState IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    switch (value)
                    {
                        case CheckBoxState.Checked:
                            _isChecked = CheckBoxState.Checked;
                            break;
                        case CheckBoxState.Indeterminate:
                        case CheckBoxState.UnChecked:
                            _isChecked = CheckBoxState.UnChecked;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    OnPropertyChanged();
                }
            }
        }

        public Filters(string name, CheckBoxState isChecked)
        {
            _name = name;
            _isChecked = isChecked;
        }
    }
}
