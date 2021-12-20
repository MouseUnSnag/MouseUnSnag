using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MouseUnSnag.Configuration;

namespace MouseUnSnagTests.Configuration
{
    [TestClass()]
    public class OptionsHelpers
    {
        [TestMethod()]
        public void TestOptionsPermutationsTests()
        {
            Assert.IsTrue(OptionPermutations().Count() > 10);
        }

        public static void Compare(Options expected, Options actual)
        {
            Assert.AreEqual(expected.Jump, actual.Jump);
            Assert.AreEqual(expected.Wrap, actual.Wrap);
            Assert.AreEqual(expected.Unstick, actual.Unstick);
            Assert.AreEqual(expected.Rescale, actual.Rescale);
        }

        public static IEnumerable<Options> OptionPermutations()
        {
            var boolList = new List<bool>() { false, true };
            
            foreach (var jump in boolList)
                foreach (var rescale in boolList)
                    foreach (var unstick in boolList)
                        foreach (var wrap in boolList)
                            yield return new Options() { Jump = jump, Rescale = rescale, Unstick = unstick, Wrap = wrap};

        }

    }
}
