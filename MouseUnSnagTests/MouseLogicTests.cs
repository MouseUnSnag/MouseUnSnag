using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using MouseUnSnag.Configuration;
using MouseUnSnag.ScreenHandling;

namespace MouseUnSnag.Tests
{
    [TestFixture]
    public class MouseLogicTests
    {
        [Test]
        public void MouseLogicTest()
        {
            var opts = new Options();
            var ml = new MouseLogic(opts);

            Assert.AreSame(opts, ml.Options);
            Assert.AreEqual(Rectangle.Empty ,ml.LastCursorScreenBounds);
            Assert.AreEqual(true, ml.ScreensAreUpdating);

            var gotException = false;
            try
            {
                ml = new MouseLogic(null);
            }
            catch (ArgumentNullException)
            {
                gotException = true;
            }
            Assert.IsTrue(gotException, "Should have thrown");
        }

        [Test]
        public void BeginAndEndScreenUpdateTest()
        {
            var opts = new Options();
            var ml = new MouseLogic(opts);

            ml.BeginScreenUpdate();
            Assert.IsTrue(ml.ScreensAreUpdating);

            var dl = new DisplayList(new List<IScreen>());
            ml.EndScreenUpdate(dl);
            Assert.AreSame(dl, ml.DisplayList);
            Assert.IsFalse(ml.ScreensAreUpdating);

            ml.BeginScreenUpdate();
            Assert.IsTrue(ml.ScreensAreUpdating);

        }

    }
}
