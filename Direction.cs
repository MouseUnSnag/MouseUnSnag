using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseUnSnag
{
    [Flags]
    public enum Direction
    {
        None  = 0x0,
        Left  = 0x1,
        Right = 0x2,
        Up    = 0x4,
        Down  = 0x8,
    }
}
