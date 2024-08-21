using System;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.Core.Attributes;
using Xceed.Wpf.Toolkit.Core;

namespace CUpdater
{
    public class PublishVersion : ViewModelBase
    {
        private string _version = "1.0.0.0";
        private string _description;
        public DateTime PublishDate { get; } = DateTime.Now;
        [DisplayName("版本")]
        public string Version
        {
            get => _version;
            set
            {
                if (value != _version)
                {
                    _version = value;
                    OnPropertyChanged();
                }
            }
        }
        [DisplayName("描述"), FilePath("README.md")]
        public string Description
        {
            get => _description;
            set
            {
                if (value != _description)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }
        public PublishVersion(string version, string description)
        {
            Version = version;
            Description = description;
        }
        public PublishVersion(string version)
        {
            Version = version;
        }
        public PublishVersion()
        {
        }
    }
}
