using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MouseUnSnag.Configuration;

namespace MouseUnSnagTests.Configuration
{
    [TestFixture]
    public class OptionsHelpers
    {
        [Test]
        public void TestOptionsPermutationsTests()
        {
            Assert.IsTrue(OptionPermutations().Count() > 10);
        }

        public static void Compare(Options expected, Options actual)
        {
            Assert.AreEqual(expected.Jump, actual.Jump);
            Assert.AreEqual(expected.Wrap, actual.Wrap);
            Assert.AreEqual(expected.Unstick, actual.Unstick);
        }

        public static IEnumerable<Options> OptionPermutations()
        {
            var boolList = new List<bool>() { false, true };
            
            foreach (var jump in boolList)
                foreach (var rescale in boolList)
                    foreach (var unstick in boolList)
                        foreach (var wrap in boolList)
                            yield return new Options() { Jump = jump, Unstick = unstick, Wrap = wrap};

        }

    }
}
