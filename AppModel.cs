using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUpdater
{
    public static class AppModel
    {
        /// <summary>
        /// 程序名称
        /// </summary>
        public static string EN { get; set; } = "SDSyncApp";
        public static string URL { get; set; } = "http://127.0.0.1:5500/";

        /// <summary>
        /// 启动程序路径
        /// </summary>
        public static string StartUpApp { get; set; } = "D:\\codes\\csharp\\CUpdater\\bin\\x64\\SDSyncApp.exe";
    }
}
