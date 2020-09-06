using System;
using MouseUnSnag.ScreenHandling;

namespace MouseUnSnag.Win32Interop.Display
{
    public static class Win32Display
    {
        public const int DefaultDpi = 96;

        /// <summary>
        /// Get Dpi of a Screen
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="dpiType"></param>
        /// <returns></returns>
        public static uint GetDpi(this IScreen screen, DpiType dpiType)
        {
            if (screen == null)
                throw new ArgumentNullException(nameof(screen));

            try
            {
                var mon = NativeMethods.MonitorFromPoint(screen.Bounds.Location, 2 /*MONITOR_DEFAULTTONEAREST*/ );
                NativeMethods.GetDpiForMonitor(mon, dpiType, out var dpiX, out var dpiY);
                return dpiX;
            }
            catch (DllNotFoundException)
            {
                // On Windows <8, just assume scaling 100%.
                return DefaultDpi; 
            }
        }

        /// <summary>
        /// Set DPI Awareness
        /// </summary>
        /// <param name="dpiAwareness"></param>
        /// <returns>true on success</returns>
        public static bool SetDpiAwareness(ProcessDpiAwareness dpiAwareness)
        {
            try
            {
                return NativeMethods.SetProcessDpiAwareness(dpiAwareness);
            }
            catch (DllNotFoundException)
            {
                // DPI Awareness API is not available on older OS's, but they work in
                // physical pixels anyway, so we just ignore if the call fails.
            }
            return false;
        }
    }
}
