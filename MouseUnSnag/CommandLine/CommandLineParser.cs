using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using MouseUnSnag.Configuration;

namespace MouseUnSnag.CommandLine
{
    
    public class CommandLineParser
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This is not a high performance Method and we might use member variables in the future")]
        internal Options Decode(IEnumerable<string> args, Options options = null)
        {
            options ??= new Options();

            foreach (var arg in args)
            {
                var chars = arg.ToCharArray();
                if (chars.Length != 2)
                    DisplayUsageAndExit($"Invalid argument: {arg}. ");

                if (!TryDecodeFlag(chars[0], out var flagValue))
                    DisplayUsageAndExit($"Invalid argument: {arg}. ");

                switch (char.ToUpperInvariant(chars[1]))
                {
                    case 'S':
                        options.Unstick = flagValue;
                        break;
                    case 'J':
                        options.Jump = flagValue;
                        break;
                    case 'W':
                        options.Wrap = flagValue;
                        break;
                    case 'R':
                        options.Rescale = flagValue;
                        break;
                    default:
                        DisplayUsageAndExit($"Invalid Argument: {arg}");
                        break;
                }

            }

            return options;
        }

        private static bool TryDecodeFlag(char c, out bool result)
        {
            result = false;
            switch (c)
            {
                case '+':
                    result = true;
                    return true;
                case '-':
                    result = false;
                    return true;
            }
            return false;
        }

        private static void DisplayUsageAndExit(string prependText = null)
        {
            var lines = new List<string>();

            if (!string.IsNullOrEmpty(prependText))
            {
                lines.Add(prependText);
                lines.Add("------------------");
            }

            lines.Add($"Usage: {System.Reflection.Assembly.GetExecutingAssembly().Location} [options ...]");
            lines.Add("\t-s    Disables mouse un-sticking.");
            lines.Add("\t+s    Enables mouse un-sticking. Default.");
            lines.Add("\t-j    Disables mouse jumping.");
            lines.Add("\t+j    Enables mouse jumping. Default.");
            lines.Add("\t-w    Disables mouse wrapping.");
            lines.Add("\t+w    Enables mouse wrapping. Default.");
            lines.Add("\t-r    Disables mouse scaling. Default.");
            lines.Add("\t+r    Enables mouse scaling. Experimental");


            // FIXME: this is not a great way to handle errors.
            // Console.WriteLine(string.Join(Environment.NewLine, lines));
            MessageBox.Show(string.Join(Environment.NewLine, lines), @"Invalid Argument", MessageBoxButtons.OK);
            Environment.Exit(1);
        }
    }
}
