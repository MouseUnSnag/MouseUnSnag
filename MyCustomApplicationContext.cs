/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Windows.Forms;
using MouseUnSnag.CommandLine;
using MouseUnSnag.Properties;

namespace MouseUnSnag
{
    internal sealed class MyCustomApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;

        public MyCustomApplicationContext(Options options)
        {
            _trayIcon = new NotifyIcon
            {
                Icon = Resources.Icon,
                ContextMenu = new ContextMenu
                {
                    MenuItems =
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

                        new MenuItem("Exit", delegate
                        {
                            _trayIcon.Visible = false;
                            Application.Exit();
                        })
                    }
                },
                Visible = true
            };
        }

    }
}
