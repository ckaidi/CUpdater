﻿using System;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.Core.Attributes;
using Xceed.Wpf.Toolkit.Core;

namespace CUpdater
{
    public class PublishVersion : ViewModelBase
    {
        private string _version = "1.0.0.0";
        private string _description;
        private string _name;
        private string _executePath;
        public DateTime PublishDate { get; } = DateTime.Now;

        /// <summary>
        /// 执行文件路径
        /// </summary>
        [DisplayName("项目名称")]
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

        /// <summary>
        /// 执行文件路径
        /// </summary>
        [DisplayName("执行文件路径"), FilePath("可执行文件|*.exe")]
        public string ExecutePath
        {
            get => _executePath;
            set
            {
                if (_executePath != value)
                {
                    _executePath = value;
                    OnPropertyChanged();
                }
            }
        }

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

        [DisplayName("描述"), FilePath("*.md|*.md", "README.md")]
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
