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
    }
}
