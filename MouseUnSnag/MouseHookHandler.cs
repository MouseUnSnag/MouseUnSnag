﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using MouseUnSnag.Configuration;
using MouseUnSnag.Hooking;
using MouseUnSnag.ScreenHandling;
using MouseUnSnag.Win32Interop;

namespace MouseUnSnag
{
    internal class MouseHookHandler
    {
        // Need to explicitly keep a reference to this, so it does not get "garbage collected."
        private NativeMethods.HookProc _mouseHookDelegate;
        private IntPtr _llMouseHook = IntPtr.Zero;
        private HookHandler _hookHandler;

        private Rectangle _cursorScreenBounds;
        private readonly MouseLogic _mouseLogic;

        public MouseHookHandler(Options options)
        {
            _mouseLogic = new MouseLogic(options);
            _mouseLogic.LastCursorBoundsChanged += (sender, args) => _cursorScreenBounds = args.Bounds;
        }
        
        public void Run()
        {
            UpdateScreens();
            SystemEvents.DisplaySettingsChanged += Event_DisplaySettingsChanged;

            // Keep a reference to the delegate, so it does not get garbage collected.
            _mouseHookDelegate = LlMouseHookCallback;
            _hookHandler = new HookHandler();
            _llMouseHook = _hookHandler.SetHook(Win32Mouse.WhMouseLl, _mouseHookDelegate);

            // Run the application
            using (var ctx = new TrayIconApplicationContext(_mouseLogic.Options))
            {
                // Update the number of unsnags displayed when hovering over the tray icon
                ctx.SetToolTip(() => $"Unsnags: {_mouseLogic.Jumps}");
                Application.Run(ctx);
            }

            // Quit
            Debug.WriteLine("Exiting...");
            HookHandler.UnsetHook(ref _llMouseHook);
            SystemEvents.DisplaySettingsChanged -= Event_DisplaySettingsChanged;
        }

        /// <summary>
        /// Mouse hook that is called whenever the mouse moves.
        /// </summary>
        /// <param name="nCode">
        /// Smaller than 0: Pass to CallNextHookEx
        /// 0: HC_ACTION: wParam and lParam contain information about a mouse message
        /// 3: HC_NOREMOVE: like HC_ACTION, but the message stays in the queue
        /// </param>
        /// <param name="wParam">Mouse Message Identifier</param>
        /// <param name="lParam">MouseHookStruct structure</param>
        /// <returns></returns>
        private IntPtr LlMouseHookCallback(int nCode, uint wParam, IntPtr lParam)
        {
            if ((nCode == 0) && (wParam == Win32Mouse.WmMouseMove))
            {
                var mouse = Win32Mouse.GetMouseLocation(lParam);

                if (!_cursorScreenBounds.Contains(mouse) && NativeMethods.GetCursorPos(out var cursor) && _mouseLogic.HandleMouse(mouse, cursor, out var newCursor))
                {
                    Win32Mouse.SetCursorPos(newCursor);
                    return (IntPtr) 1;
                }
            }

            return NativeMethods.CallNextHookEx(_llMouseHook, nCode, wParam, lParam);
        }


        private void Event_DisplaySettingsChanged(object sender, EventArgs e)
        {
            UpdateScreens();
        }

        private void UpdateScreens()
        {
            Debug.WriteLine("\nDisplay Settings Changed...");
            var sw = new Stopwatch();
            sw.Start();
            _mouseLogic.BeginScreenUpdate();
            var displays = new DisplayList(Screen.AllScreens.Select(x => (ScreenWrapper) x));
            _mouseLogic.EndScreenUpdate(displays);
            sw.Stop();
            Debug.WriteLine($"Updated display configuration in: {sw.Elapsed.TotalMilliseconds:0.00} ms");
            Debug.WriteLine(displays.GetScreenInformation());
        }
    }
}
