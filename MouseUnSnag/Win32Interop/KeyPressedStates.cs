using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseUnSnag.Win32Interop
{
    /// <summary>
    /// Key state for GetKeyState Function. See: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getkeystate
    /// </summary>
    [Flags]
    public enum KeyPressedStates: short
    {
        /// <summary>
        /// Key is toggled (for keys like CAPS Lock or Scroll Lock: current locked state)
        /// </summary>
        KeyToggled = 0x1,

        /// <summary>
        /// Key is pressed (indicates the current state of the key)
        /// </summary>
        KeyPressed = 0x80
    }
}
