using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseUnSnag.Configuration
{
    /// <summary>
    /// Serializes Options
    /// </summary>
    public class OptionsSerializer
    {
        /// <summary>
        /// Seperator for one option
        /// </summary>
        public string OptionSeperator { get; } = Environment.NewLine;

        /// <summary>
        /// Separator between Option name and Option Value
        /// </summary>
        public string FieldSeparator { get; } = ":";

        /// <summary>
        /// Serializes the given options into text
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public string Serialize(Options options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var opts = new List<(string name, object value)>();
            opts.Add(("Rescale", options.Rescale));
            opts.Add(("Wrap   ", options.Wrap));
            opts.Add(("Jump   ", options.Jump));
            opts.Add(("Unstick", options.Unstick));

            var lines = opts.Select(x => $"{x.name} {FieldSeparator} {x.value}");
            return string.Join(OptionSeperator, lines);
        }

        public Options Deserialize(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var options = new Options();

            var lines = text.Split(new[] {OptionSeperator}, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var optionFields = line.Split(new[] {FieldSeparator}, StringSplitOptions.None);
                if (optionFields.Length != 2)
                {
                    Debug.WriteLine($"Deserialization Failure: Expected 2 Fields, got {optionFields.Length}. Line: {line}");
                    continue;
                }

                var optionName = optionFields[0].Trim();
                var optionValue = optionFields[1].Trim();

                try
                {
                    switch (optionName.ToUpperInvariant())
                    {
                        case "RESCALE":
                            options.Rescale = bool.Parse(optionValue);
                            break;
                        case "WRAP":
                            options.Wrap = bool.Parse(optionValue);
                            break;
                        case "JUMP":
                            options.Jump = bool.Parse(optionValue);
                            break;
                        case "UNSTICK":
                            options.Unstick = bool.Parse(optionValue);
                            break;
                        default:
                            Debug.WriteLine($"Could not recognize {optionName} as an option");
                            break;
                    }

                }
                catch (FormatException)
                {
                    Debug.WriteLine($"Could not decode Option value for Option: {optionName}, Value {optionValue}");
                }
            }

            return options;
        }

        

    }
}
