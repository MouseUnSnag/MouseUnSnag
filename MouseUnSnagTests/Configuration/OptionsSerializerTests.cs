﻿using NUnit.Framework;
using MouseUnSnag.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
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