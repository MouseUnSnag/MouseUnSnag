using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace MouseUnSnag.ScreenHandling.Tests
{
    [TestClass()]
    public class ScreenWrapperTests
    {
        [TestMethod()]
        public void ScreenWrapperTest()
        {
            var expected = Screen.PrimaryScreen;
            var actual = new ScreenWrapper(expected);
            Compare(expected, actual);
        }

        [TestMethod()]
        public void ScreenWrapperConvertTest()
        {
            var expected = Screen.PrimaryScreen;
            var actual = (ScreenWrapper) expected;
            Compare(expected, actual);

            var actual2 = (Screen) actual;
            Assert.AreSame(expected, actual2);
        }


        [TestMethod()]
        public void ToScreenTest()
        {
            var originalScreen = Screen.PrimaryScreen;
            var wrapper = ScreenWrapper.ToScreenWrapper(originalScreen);
            var newScreen = wrapper.ToScreen();
            Assert.AreSame(originalScreen, newScreen);
        }

        [TestMethod()]
        public void ToScreenWrapperTest()
        {
            var expected = Screen.PrimaryScreen;
            var actual = ScreenWrapper.ToScreenWrapper(expected);
            Compare(expected, actual);
        }

        private void Compare(Screen expected, ScreenWrapper actual)
        {
            Assert.AreEqual(expected.BitsPerPixel, actual.BitsPerPixel);
            Assert.AreEqual(expected.Bounds, actual.Bounds);
            Assert.AreEqual(expected.DeviceName, actual.DeviceName);
            Assert.AreEqual(expected.Primary, actual.Primary);
            Assert.AreEqual(expected.WorkingArea, actual.WorkingArea);
        }
    }
}