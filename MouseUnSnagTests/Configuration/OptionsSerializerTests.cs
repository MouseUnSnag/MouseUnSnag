using Microsoft.VisualStudio.TestTools.UnitTesting;
using MouseUnSnagTests.Configuration;

namespace MouseUnSnag.Configuration.Tests
{
    [TestClass()]
    public class OptionsSerializerTests
    {
        [TestMethod()]
        public void SerializeDeserializeTest()
        {
            var serializer = new OptionsSerializer();

            Options RoundTrip(Options options) => serializer.Deserialize(serializer.Serialize(options));

            foreach (var options in OptionsHelpers.OptionPermutations())
            {
                OptionsHelpers.Compare(options, RoundTrip(options));
            }
        }

    }
}
