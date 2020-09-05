using System.Drawing;
using System.Windows.Forms;

namespace MouseUnSnag.ScreenHandling
{
    /// <summary>
    /// Abstracts the Properties of a <see cref="Screen"/>
    /// </summary>
    public interface IScreen
    {
        /// <inheritdoc cref="Screen.BitsPerPixel"/>
        int BitsPerPixel { get; }

        /// <inheritdoc cref="Screen.Bounds"/>
        Rectangle Bounds { get; }

        /// <inheritdoc cref="Screen.DeviceName"/>
        string DeviceName { get; }

        /// <inheritdoc cref="Screen.Primary"/>
        bool Primary { get; }

        /// <inheritdoc cref="Screen.WorkingArea"/>
        Rectangle WorkingArea { get; }
    }
}