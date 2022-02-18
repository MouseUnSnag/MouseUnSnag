using NUnit.Framework;
using System;
using System.Drawing;

namespace MouseUnSnag.ScreenHandling.Tests
{
    [TestFixture]
    public class DisplayTests
    {
        [Test]
        public void DisplayTest()
        {
            var screen = new VirtualScreen(99,
                new Rectangle(9999, 9999, 9999, 9999),
                "Awesome Screen",
                false,
                new Rectangle(8888, 8888, 8888, 888));

            var screenNumber = 12;

            var d = new Display(screen, screenNumber);


            // Null Checks
            Assert.IsNotNull(d.Above);
            Assert.IsNotNull(d.Below);
            Assert.IsNotNull(d.ToLeft);
            Assert.IsNotNull(d.ToRight);
            Assert.IsNotNull(d.ToString());
            Assert.IsNotNull(d.DetailledDescription());

            // Screen Stuff
            Assert.AreEqual(screenNumber, d.ScreenNumber);
            Assert.AreSame(screen, d.Screen);
            Assert.AreEqual(screen.Bounds, d.Bounds);

            Console.WriteLine(d.DetailledDescription());

        }


    }
}
