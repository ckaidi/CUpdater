using System.Collections.Generic;
using Xceed.Wpf.Toolkit.Core;

namespace CupdateInfoGenerater
{
    public class PublishFileModel : ViewModelBase
    {
        public PublishVersion Version { get; set; }
        public List<FileHash> Files { get; set; } = new List<FileHash>();

        public PublishFileModel(PublishVersion version)
        {
            Version = version;
        }
    }
}
