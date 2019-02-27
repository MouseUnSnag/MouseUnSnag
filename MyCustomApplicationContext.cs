/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Windows.Forms;
using MouseUnSnag.Properties;

namespace MouseUnSnag
{
    internal sealed class MyCustomApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon trayIcon;

        public MyCustomApplicationContext(Program program)
        {
            trayIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                ContextMenu = new ContextMenu
                {
                    MenuItems =
                    {
                        new MenuItem("UnStick from corners", (sender, _) =>
                        {
                            var item = (MenuItem) sender;
                            program.IsUnstickEnabled = !program.IsUnstickEnabled;
                            item.Checked = program.IsUnstickEnabled;
                        })
                        {
                            Checked = program.IsUnstickEnabled
                        },

                        new MenuItem("Jump between monitors", (sender, _) =>
                        {
                            var item = (MenuItem) sender;
                            program.IsJumpEnabled = !program.IsJumpEnabled;
                            item.Checked = program.IsJumpEnabled;
                        })
                        {
                            Checked = program.IsJumpEnabled
                        },

                        new MenuItem("Wrap around monitors", (sender, _) =>
                        {
                            var item = (MenuItem) sender;
                            program.IsScreenWrapEnabled = !program.IsScreenWrapEnabled;
                            item.Checked = program.IsScreenWrapEnabled;
                        })
                        {
                            Checked = program.IsScreenWrapEnabled
                        },

                        new MenuItem("Exit", delegate
                        {
                            trayIcon.Visible = false;
                            Application.Exit();
                        })
                    }
                },
                Visible = true
            };

            // Update the number of unsnags displayed when hovering over the tray icon
            trayIcon.MouseMove += new MouseEventHandler((sender, e) => {
                trayIcon.Text = $"Unsnags: {program.NJumps}";
            });
        }
    }
}
