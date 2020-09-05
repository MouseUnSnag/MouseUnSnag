using System.Drawing;

namespace MouseUnSnag.ScreenHandling
{
    public interface IScreen
    {
        int BitsPerPixel { get; }
        Rectangle Bounds { get; }
        string DeviceName { get; }
        bool Primary { get; }
        Rectangle WorkingArea { get; }
    }
}