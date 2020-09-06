/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using MouseUnSnag.CommandLine;
using MouseUnSnag.Configuration;
using MouseUnSnag.Win32Interop;
using MouseUnSnag.Win32Interop.Display;

namespace MouseUnSnag
{

    public static class Program
    {
        [STAThread]
        public static int Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Make sure the MouseUnSnag.exe has only one instance running at a time.
            using (new Mutex(true, "__MouseUnSnag_EXE__", out var createdNew))
            {
                if (!createdNew)
                    return ShowAlreadyRunningMessageAndQuit();

                Win32Display.SetDpiAwareness(ProcessDpiAwareness.ProcessPerMonitorDpiAware);

                // Load Config from File
                var optionsFilename = Path.Combine(GetOptionsDirectory(), "snagConfig", "snagConfig.txt");
                var writer = new OptionsWriter(optionsFilename);
                var options = writer.TryRead();

                // Override by Command line args
                options = new CommandLineParser().Decode(Environment.GetCommandLineArgs().Skip(1), options);

                // Write Config if changed
                options.ConfigChanged += (sender, args) =>
                {
                    lock (writer)
                    {
                        writer.Write(options);
                    }
                };

                var hookHandler = new MouseHookHandler(options);
                hookHandler.Run();
            }

            return 0;
        }

        private static string GetOptionsDirectory()
        {
            try
            {
                var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!string.IsNullOrEmpty(assemblyDir) && Directory.Exists(assemblyDir))
                    return assemblyDir;
            }
            catch (ArgumentException e)
            {
                Debug.WriteLine(e);
            }

            var cliDirName = Environment.GetCommandLineArgs().First();
            if (!string.IsNullOrEmpty(cliDirName) && Directory.Exists(cliDirName))
                return cliDirName;

            return Environment.CurrentDirectory;
        }

        private static int ShowAlreadyRunningMessageAndQuit()
        {
            MessageBox.Show(@"MouseUnSnag is already running!! Quitting this instance...", "MouseUnSnag", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return 2;
        }


    }
}
