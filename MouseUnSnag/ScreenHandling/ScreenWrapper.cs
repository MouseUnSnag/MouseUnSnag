using System;
using System.Drawing;
using System.Windows.Forms;

namespace MouseUnSnag.ScreenHandling
{
    /// <summary>
    /// Implements the IScreen Interface for a <see cref="Screen"/>
    /// </summary>
    public class ScreenWrapper : IScreen
    {
        private readonly Screen _screen;

        /// <summary>
        /// Initializes a new <see cref="ScreenWrapper"/>
        /// </summary>
        /// <param name="screen">Underlying <see cref="Screen"/></param>
        /// <exception cref="ArgumentNullException">screen was null</exception>
        public ScreenWrapper(Screen screen)
        {
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        }

        /// <inheritdoc />
        public int BitsPerPixel => _screen.BitsPerPixel;
        /// <inheritdoc />
        public Rectangle Bounds => _screen.Bounds;
        /// <inheritdoc />
        public string DeviceName => _screen.DeviceName;
        /// <inheritdoc />
        public bool Primary => _screen.Primary;
        /// <inheritdoc />
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
