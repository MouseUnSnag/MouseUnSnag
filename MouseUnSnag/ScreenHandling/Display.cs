using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using MouseUnSnag.Win32Interop;
using MouseUnSnag.Win32Interop.Display;

namespace MouseUnSnag.ScreenHandling
{

    /// <summary>
    /// Encapsulates a display and its relation to other displays
    /// </summary>
    public class Display
    {
        /// <summary> Corresponding <see cref="IScreen"/> </summary>
        public IScreen Screen { get; }

        /// <summary> Screen Number </summary>
        public int ScreenNumber { get; }

        /// <summary> Bounds of the Screen </summary>
        public Rectangle Bounds { get;  }

        /// <summary> Displays to the left of this one </summary>
        public List<Display> ToLeft { get; } = new List<Display>();
        /// <summary> Displays to the right of this one </summary>
        public List<Display> ToRight { get; } = new List<Display>();
        /// <summary> Displays above this one </summary>
        public List<Display> Above { get; } = new List<Display>();
        /// <summary> Displays below this one </summary>
        public List<Display> Below { get; } = new List<Display>();


        
        /// <summary>
        /// The effective DPI. This value should be used when determining the correct scale factor for scaling UI elements. This incorporates the scale factor set by the user for this specific display.
        /// </summary>
        public int EffectiveDpi { get; }

        /// <summary>
        /// The raw DPI. This value is the linear DPI of the screen as measured on the screen itself. Use this value when you want to read the pixel density and not the recommended scaling setting. This does not include the scale factor set by the user for this specific display and is not guaranteed to be a supported DPI value.
        /// </summary>
        public int RawDpi { get; }

        /// <summary>
        /// The angular DPI. This DPI ensures rendering at a compliant angular resolution on the screen. This does not include the scale factor set by the user for this specific display.
        /// </summary>
        public int AngularDpi { get; }

        /// <summary>
        /// Scale factor that was set for this display
        /// </summary>
        public int ScaleFactor => (int) Math.Round(EffectiveDpi / 96.0 * 100);


        /// <summary>
        /// Initialize a new <see cref="Display"/>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="screenNumber"></param>
        /// <exception cref="ArgumentNullException"><param name="screen"/> was null</exception>
        public Display(IScreen screen, int screenNumber)
        {
            Screen = screen ?? throw new ArgumentNullException(nameof(screen));
            Bounds = screen.Bounds;
            ScreenNumber = screenNumber;

            RawDpi = (int) Win32Display.GetDpi(screen, DpiType.Raw);
            EffectiveDpi = (int) Win32Display.GetDpi(screen, DpiType.Effective);
            AngularDpi = (int) Win32Display.GetDpi(screen, DpiType.Angular);
        }


        /// <summary>
        /// Links both <see cref="Display"/>s if they are adjacent. Does nothing otherwise.
        /// Adjacent means that both <see cref="Display"/>s share a border
        /// The <see cref="Display"/> passed in will be added to at most one List (<see cref="ToLeft"/>, <see cref="ToRight"/>, <see cref="Above"/>, <see cref="Below"/>)
        /// </summary>
        /// <param name="display"></param>
        public void Link(Display display)
        {
            if (display == null)
                throw new ArgumentNullException(nameof(display));

            if ((Bounds.Right == display.Bounds.Left) && GeometryUtil.OverlapY(Bounds, display.Bounds)) 
                ToRight.Add(display);
            else if ((Bounds.Left == display.Bounds.Right) && GeometryUtil.OverlapY(Bounds, display.Bounds)) 
                ToLeft.Add(display);
            else if ((Bounds.Top == display.Bounds.Bottom) && GeometryUtil.OverlapX(Bounds, display.Bounds)) 
                Above.Add(display);
            else if ((Bounds.Bottom == display.Bounds.Top) && GeometryUtil.OverlapX(Bounds, display.Bounds)) 
                Below.Add(display);
        }



        public string DetailledDescription()
        {
            return $"({Bounds.Left},{Bounds.Top})-({Bounds.Right},{Bounds.Bottom})   Size:({Bounds.Width},{Bounds.Height}) " +
                   $"L({AsString(ToLeft)}),R({AsString(ToRight)}),A({AsString(Above)}),B({AsString(Below)})    " +
                   $"DPI(Raw/Eff/Ang): {RawDpi}/{EffectiveDpi}/{AngularDpi}  " +
                   $"Screen Scaling: {ScaleFactor}%   , {Screen.DeviceName}";


            static string AsString(List<Display> L) => string.Join(",", L.Select(sn => sn.ScreenNumber));
        }

        /// <summary>
        /// Currently returns the screen number as a string. May change in the future
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ScreenNumber.ToString(CultureInfo.InvariantCulture);

    }
}
