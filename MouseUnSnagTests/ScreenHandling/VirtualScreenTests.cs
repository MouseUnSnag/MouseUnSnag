using NUnit.Framework;
using System.Drawing;

namespace MouseUnSnag.ScreenHandling.Tests
{
    [TestFixture]
    public class VirtualScreenTests
    {
        [Test]
        public void VirtualScreenTest()
        {
            var bitsPerPixel = 24;
            var bounds = new Rectangle(120, 240, 360, 480);
            var name = "I am a virtual screen";
            var primary = true;
            var workArea = new Rectangle(bounds.X + 10, bounds.Y + 20, bounds.Width - 50, bounds.Height - 90);


            var actual = new VirtualScreen(bitsPerPixel, bounds, name, primary, workArea);

            Assert.AreEqual(bitsPerPixel, actual.BitsPerPixel);
            Assert.AreEqual(bounds, actual.Bounds);
            Assert.AreEqual(name, actual.DeviceName);
            Assert.AreEqual(primary, actual.Primary);
            Assert.AreEqual(workArea, actual.WorkingArea);
        }
    }
}