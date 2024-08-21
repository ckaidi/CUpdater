using System.IO;

namespace CUpdater
{
    public class FileHash
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public long Size { get; set; }

        public FileHash(string name, string hash, long size)
        {
            Name = name;
            Hash = hash;
            Size = size;
        }

        public string FullName(string folder)
        {
            return Path.Combine(folder, Name.Replace("./", ""));
        }
    }
}
