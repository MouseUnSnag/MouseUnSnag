using System.Drawing;

namespace MouseUnSnag.ScreenHandling
{
    public class VirtualScreen: IScreen
    {
        public VirtualScreen(int bitsPerPixel, Rectangle bounds, string deviceName, bool primary, Rectangle workingArea)
        {
            BitsPerPixel = bitsPerPixel;
            Bounds = bounds;
            DeviceName = deviceName;
            Primary = primary;
            WorkingArea = workingArea;
        }

        public int BitsPerPixel { get; }
        public Rectangle Bounds { get; }
        public string DeviceName { get; }
        public bool Primary { get; }
        public Rectangle WorkingArea { get; }
    }
}
