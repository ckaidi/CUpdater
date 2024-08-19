using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupdateInfoGenerater
{
    public class FileHash
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public string Version { get; set; }

        public FileHash(string name, string hash, string version)
        {
            Name = name;
            Hash = hash;
            Version = version;
        }
    }
}
