using NUnit.Framework;
using System.Collections.Generic;
using MouseUnSnagTests.Configuration;

namespace MouseUnSnag.CommandLine.Tests
{
    [TestFixture]
    public class CommandLineParserTests
    {
        [Test]
        public void DecodeTest()
        {
            var parser = new CommandLineParser();

            foreach (var options in OptionsHelpers.OptionPermutations())
            {
                var opts = new List<string>();
                opts.Add(options.Wrap ? "+w" : "-w");
                opts.Add(options.Jump ? "+j" : "-j");
                opts.Add(options.Unstick ? "+s" : "-s");

                OptionsHelpers.Compare(options, parser.Decode(opts));
            }
        }

        [Test]
        public void DecodeWithBackingTest()
        {
            var parser = new CommandLineParser();

            foreach (var backingOptions in OptionsHelpers.OptionPermutations())
            {
                foreach (var options in OptionsHelpers.OptionPermutations())
                {
                    var opts = new List<string>();
                    if (options.Wrap != backingOptions.Wrap)
                        opts.Add(options.Wrap ? "+w" : "-w");

                    if (options.Jump != backingOptions.Jump)
                        opts.Add(options.Jump ? "+j" : "-j");

                    if (options.Unstick != backingOptions.Unstick)
                        opts.Add(options.Unstick ? "+s" : "-s");
                    OptionsHelpers.Compare(options, parser.Decode(opts, backingOptions));
                }

            }
        }

    }
}
