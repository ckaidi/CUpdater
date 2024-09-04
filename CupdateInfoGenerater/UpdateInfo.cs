using System.ComponentModel;
using Xceed.Wpf.Toolkit.Attributes;

namespace CupdateInfoGenerater
{
    public class UpdateInfo
    {
        /// <summary>
        /// 是否启用更新器
        /// </summary>
        [DisplayName("是否启用更新器"), CheckBox]
        public bool IsAddUpdater { get; set; } = true;

        /// <summary>
        /// 更新Url
        /// </summary>
        [DisplayName("更新Url")]
        public string Url { get; set; } = "";
    }
}