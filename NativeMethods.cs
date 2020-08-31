/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MouseUnSnag
{
    public static class NativeMethods
    {
        public const int WhMouseLl = 14; // Win32 low-level mouse event hook ID.
        public const int WmMousemove = 0x0200;

        public delegate IntPtr HookProc(int nCode, uint wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, uint wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        internal static extern bool SetCursorPos(int X, int Y);
        internal static bool SetCursorPos(Point p) => SetCursorPos(p.X, p.Y);

        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out Point lpPoint);

        public enum ProcessDpiAwareness
        {
            ProcessDpiUnaware = 0,
            ProcessSystemDpiAware = 1,
            ProcessPerMonitorDpiAware = 2
        }

        [DllImport("SHCore.dll", SetLastError = true)]
        internal static extern bool SetProcessDpiAwareness(ProcessDpiAwareness awareness);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Msllhookstruct
        {
            public Point pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public enum DpiType
        {
            Effective = 0,
            Angular = 1,
            Raw = 2
        }

        //https://msdn.microsoft.com/en-us/library/windows/desktop/dd145062(v=vs.85).aspx
        [DllImport("User32.dll")]
        internal static extern IntPtr MonitorFromPoint([In] Point pt, [In] uint dwFlags);

        //https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510(v=vs.85).aspx
        [DllImport("Shcore.dll")]
        internal static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

        public static uint GetDpi(Screen screen, DpiType dpiType)
        {
            try
            {
                var mon = MonitorFromPoint(screen.Bounds.Location, 2 /*MONITOR_DEFAULTTONEAREST*/ );
                GetDpiForMonitor(mon, dpiType, out var dpiX, out var dpiY);
                return dpiX;
            }
            catch (DllNotFoundException)
            {
                return 96; // On Windows <8, just assume scaling 100%.
            }
        }
    }
}