using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace MouseUnSnag.CommandLine
{
    internal class CommandLineParser
    {
        public Options Decode(IEnumerable<string> args)
        {
            var options = new Options();

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "-s":
                        options.Unstick = false;
                        break;
                    case "+s":
                        options.Unstick = true;
                        break;
                    case "-j":
                        options.Jump = false;
                        break;
                    case "+j":
                        options.Jump = true;
                        break;
                    case "-w":
                        options.Wrap = false;
                        break;
                    case "+w":
                        options.Wrap = true;
                        break;
                    default:
                        DisplayUsageAndExit($"Invalid Argument: {arg}");
                        break;
                }
            }

            return options;
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


            // Console.WriteLine(string.Join(Environment.NewLine, lines));
            MessageBox.Show(string.Join(Environment.NewLine, lines), @"Invalid Argument", MessageBoxButtons.OK);
            Environment.Exit(1);
        }
    }
}
