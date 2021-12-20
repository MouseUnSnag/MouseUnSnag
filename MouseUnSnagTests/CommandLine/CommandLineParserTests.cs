using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using MouseUnSnagTests.Configuration;

namespace MouseUnSnag.CommandLine.Tests
{
    [TestClass()]
    public class CommandLineParserTests
    {
        [TestMethod()]
        public void DecodeTest()
        {
            var parser = new CommandLineParser();

            foreach (var options in OptionsHelpers.OptionPermutations())
            {
                var opts = new List<string>();
                opts.Add(options.Wrap ? "+w" : "-w");
                opts.Add(options.Jump ? "+j" : "-j");
                opts.Add(options.Rescale ? "+r" : "-r");
                opts.Add(options.Unstick ? "+s" : "-s");

                OptionsHelpers.Compare(options, parser.Decode(opts));
            }
        }

        [TestMethod()]
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

                    if (options.Rescale != backingOptions.Rescale) 
                        opts.Add(options.Rescale ? "+r" : "-r");

                    if (options.Unstick != backingOptions.Unstick) 
                        opts.Add(options.Unstick ? "+s" : "-s");
                    OptionsHelpers.Compare(options, parser.Decode(opts, backingOptions));
                }

            }
        }

    }
}
