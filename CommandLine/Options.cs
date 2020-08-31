using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseUnSnag.CommandLine
{
    class Options
    {
        public bool Unstick { get; set; } = true;

        public bool Jump { get; set; } = true;

        public bool Wrap { get; set; } = true;
    }
}
