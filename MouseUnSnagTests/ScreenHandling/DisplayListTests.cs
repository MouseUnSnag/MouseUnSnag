using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MouseUnSnag.ScreenHandling.Tests
{
    [TestClass()]
    public class DisplayListTests
    {
        [TestMethod()]
        public void DisplayListTest()
        {
            var displayList = new DisplayList(new List<IScreen>());

            Assert.IsNotNull(displayList.All);
            Assert.IsNotNull(displayList.LeftMost);
            Assert.IsNotNull(displayList.TopMost);
            Assert.IsNotNull(displayList.BottomMost);
            Assert.IsNotNull(displayList.RightMost);
            Assert.IsNotNull(displayList.GetScreenInformation());

            Assert.AreEqual(0, displayList.All.Count);

            var gotException = false;
            try
            {
                displayList = new DisplayList(null);
            }
            catch (ArgumentNullException)
            {
                gotException = true;
            }
            Assert.IsTrue(gotException, "Should have thrown");

        }

        [TestMethod()]
        public void GetScreenInformationTest()
        {
            var displays = new DisplayList(
                new []{ new VirtualScreen(32, new Rectangle(100, 200, 300, 400), "virtualScreen", true, new Rectangle(105, 205, 200, 200) ), }
                );

            var screenInfo = displays.GetScreenInformation();
            Assert.IsNotNull(screenInfo);
            Console.WriteLine(screenInfo);
        }

        [TestMethod()]
        public void JumpScreenTest()
        {
            // Test arragements from issue #16
            var top_left = new VirtualScreen(32, new Rectangle(0, 0, 800, 600), "top_left", true, new Rectangle(0, 0, 800, 600));
            var top_right = new VirtualScreen(32, new Rectangle(800, 0, 800, 600), "top_right", true, new Rectangle(800, 0, 800, 600));
            var bottom_left = new VirtualScreen(32, new Rectangle(0, 600, 800, 600), "bottom_left", true, new Rectangle(0, 600, 800, 600));
            var bottom_right = new VirtualScreen(32, new Rectangle(800, 600, 800, 600), "bottom_right", true, new Rectangle(800, 600, 800, 600));
            var arragement1 = new DisplayList(new []{top_left, top_right, bottom_right});
            var arragement2 = new DisplayList(new []{bottom_left, top_left, top_right});

            Assert.AreEqual(arragement1.JumpScreen(new Point(810, 0), top_left.Bounds).Screen, top_right);
            Assert.AreEqual(arragement2.JumpScreen(new Point(790, 0), top_right.Bounds).Screen, top_left);
            Assert.AreEqual(arragement2.JumpScreen(new Point(790, -10), top_right.Bounds).Screen, top_left);
        }
    }
}
