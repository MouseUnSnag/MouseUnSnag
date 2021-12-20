using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MouseUnSnag.Win32Interop
{
    public static class Win32Mouse
    {
        public const int WhMouseLl = 14; // Win32 low-level mouse event hook ID.
        public const int WmMouseMove = 0x0200;

        public static Point GetMouseLocation(IntPtr lParam)
        {
            var hookStruct = (NativeMethods.Msllhookstruct) Marshal.PtrToStructure(lParam, typeof(NativeMethods.Msllhookstruct));
            return hookStruct.pt;
        }

        public static bool SetCursorPos(Point p) => NativeMethods.SetCursorPos(p.X, p.Y);
    }
}
