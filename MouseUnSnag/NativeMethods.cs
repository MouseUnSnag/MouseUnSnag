/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using MouseUnSnag.ScreenHandling;

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

        /// <summary>
        /// See: https://docs.microsoft.com/en-us/windows/win32/api/shellscalingapi/ne-shellscalingapi-process_dpi_awareness
        /// </summary>
        public enum ProcessDpiAwareness
        {
            /// <summary>
            /// DPI unaware. This app does not scale for DPI changes and is always assumed to have a scale factor of 100% (96 DPI). It will be automatically scaled by the system on any other DPI setting.
            /// </summary>
            ProcessDpiUnaware = 0,

            /// <summary>
            /// System DPI aware. This app does not scale for DPI changes. It will query for the DPI once and use that value for the lifetime of the app. If the DPI changes, the app will not adjust to the new DPI value. It will be automatically scaled up or down by the system when the DPI changes from the system value.
            /// </summary>
            ProcessSystemDpiAware = 1,

            /// <summary>
            /// Per monitor DPI aware. This app checks for the DPI when it is created and adjusts the scale factor whenever the DPI changes. These applications are not automatically scaled by the system.
            /// </summary>
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

        /// <summary>
        /// See: https://docs.microsoft.com/en-us/windows/win32/api/shellscalingapi/ne-shellscalingapi-monitor_dpi_type
        /// </summary>
        public enum DpiType
        {
            /// <summary>
            /// The effective DPI. This value should be used when determining the correct scale factor for scaling UI elements. This incorporates the scale factor set by the user for this specific display.
            /// </summary>
            Effective = 0,

            /// <summary>
            /// The angular DPI. This DPI ensures rendering at a compliant angular resolution on the screen. This does not include the scale factor set by the user for this specific display.
            /// </summary>
            Angular = 1,

            /// <summary>
            /// The raw DPI. This value is the linear DPI of the screen as measured on the screen itself. Use this value when you want to read the pixel density and not the recommended scaling setting. This does not include the scale factor set by the user for this specific display and is not guaranteed to be a supported DPI value.
            /// </summary>
            Raw = 2
        }

        //https://msdn.microsoft.com/en-us/library/windows/desktop/dd145062(v=vs.85).aspx
        [DllImport("User32.dll")]
        internal static extern IntPtr MonitorFromPoint([In] Point pt, [In] uint dwFlags);

        //https://msdn.microsoft.com/en-us/library/windows/desktop/dn280510(v=vs.85).aspx
        [DllImport("Shcore.dll")]
        internal static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

        public static uint GetDpi(IScreen screen, DpiType dpiType)
        {
            if (screen == null)
                throw new ArgumentNullException(nameof(screen));

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