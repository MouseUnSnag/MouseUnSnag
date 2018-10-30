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
                Icon = Resources.Icon,
                ContextMenu = new ContextMenu
                {
                    MenuItems =
                    {
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
        }
    }
}
