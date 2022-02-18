using NUnit.Framework;
using MouseUnSnag.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MouseUnSnag.Configuration;

namespace MouseUnSnag.Configuration.Tests
{
    [TestFixture]
    public class OptionsTests
    {
        [Test]
        public void EventTest()
        {
            var options = new Options();
            var events = 0;

            options.ConfigChanged += (sender, args) => { events += 1; };

            options.Jump = !options.Jump;
            Assert.AreEqual(1, events);

            options.Rescale = !options.Rescale;
            Assert.AreEqual(2, events);

            options.Unstick = !options.Unstick;
            Assert.AreEqual(3, events);

            options.Wrap = !options.Wrap;
            Assert.AreEqual(4, events);

        }
    }
}