using NUnit.Framework;
using MouseUnSnagTests.Configuration;

namespace MouseUnSnag.Configuration.Tests
{
    [TestFixture]
    public class OptionsSerializerTests
    {
        [Test]
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
