using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUpdater
{
    public class ArgsOptionModel
    {
        [Option('p', "processid", Required = false, HelpText = "call app  process id")]
        public int ProcessId { get; set; } = -1;

        [Option('m', "mode", Required = false, HelpText = "run mode")]
        public string Mode { get; set; }

        [Option('c', "copy", Required = false, HelpText = "copy")]
        public bool Command { get; set; }
    }
}
