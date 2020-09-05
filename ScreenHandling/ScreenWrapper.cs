using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseUnSnag.ScreenHandling
{
    public class ScreenWrapper : IScreen
    {

        private readonly Screen _screen;

        public ScreenWrapper(Screen screen)
        {
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        }

        public int BitsPerPixel => _screen.BitsPerPixel;
        public Rectangle Bounds => _screen.Bounds;
        public string DeviceName => _screen.DeviceName;
        public bool Primary => _screen.Primary;
        public Rectangle WorkingArea => _screen.WorkingArea;


        public static implicit operator Screen(ScreenWrapper sw) => sw?._screen;
        public static implicit operator ScreenWrapper(Screen s) => new ScreenWrapper(s);
        
        public Screen ToScreen()
        {
            return _screen;
        }

        public static ScreenWrapper ToScreenWrapper(Screen s)
        {
            return new ScreenWrapper(s);
        }
    }
}
