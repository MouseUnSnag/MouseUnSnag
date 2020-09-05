/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MouseUnSnag.CommandLine;

namespace MouseUnSnag
{
    internal sealed class TrayIconApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;

        /// <summary>
        /// This is the one that runs "forever" while the application is alive, and handles
        /// events, etc. This application is ABSOLUTELY ENTIRELY driven by the LLMouseHook
        /// and DisplaySettingsChanged events.
        /// </summary>
        /// <param name="options"></param>
        public TrayIconApplicationContext(Options options)
        {
            _trayIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                ContextMenu = new ContextMenu(MakeMenuItems(options).ToArray()),
                Visible = true
            };
        }

        private IEnumerable<MenuItem> MakeMenuItems(Options options)
        {

            return new[]
            {
                new MenuItem("UnStick from corners", (sender, _) =>
                {
                    var item = (MenuItem) sender;
                    options.Unstick = !options.Unstick;
                    item.Checked = options.Unstick;
                })
                {
                    Checked = options.Unstick
                },

                new MenuItem("Jump between monitors", (sender, _) =>
                {
                    var item = (MenuItem) sender;
                    options.Jump = !options.Jump;
                    item.Checked = options.Jump;
                })
                {
                    Checked = options.Jump
                },

                new MenuItem("Wrap around monitors", (sender, _) =>
                {
                    var item = (MenuItem) sender;
                    options.Wrap = !options.Wrap;
                    item.Checked = options.Wrap;
                })
                {
                    Checked = options.Wrap
                },
                new MenuItem("Rescale Pointer between Screens", (sender, _) =>
                {
                    var item = (MenuItem) sender;
                    options.Wrap = !options.Rescale;
                    item.Checked = options.Rescale;
                })
                {
                    Checked = options.Rescale
                },

                new MenuItem("Exit", delegate
                {
                    _trayIcon.Visible = false;
                    Application.Exit();
                })
            };
        }
        

    }
}
