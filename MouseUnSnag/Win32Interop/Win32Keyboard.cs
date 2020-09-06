using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseUnSnag.Win32Interop
{
    public static class Win32Keyboard
    {
        public const int VkCapital = 0x14;

        public static KeyPressedStates KeyStates(int key)
        {
            return (KeyPressedStates) NativeMethods.GetKeyState(key);
        }

        public static KeyPressedStates KeyStates(Keys key) => KeyStates((int) key);
        public static KeyPressedStates KeyStates(MouseButtons mouseButton) => KeyStates((int) mouseButton);

        public static bool IsKeyPressed(int key) => (KeyStates(key) & KeyPressedStates.KeyPressed) != 0;

        public static bool IsKeyPressed(Keys key) => IsKeyPressed((int) key);

        public static bool IsKeyPressed(MouseButtons mouseButton) => IsKeyPressed((int) mouseButton);
        
    }
}
