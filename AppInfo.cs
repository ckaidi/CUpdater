using Newtonsoft.Json;

namespace CUpdater
{
    public class AppInfo
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("assembly_infos")]
        public AssemblyInfo[] AssemblyInfos { get; set; }
    }

    public class AssemblyInfo
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}
