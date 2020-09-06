using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using MouseUnSnag.CommandLine;
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
        private readonly MouseSlowdown _mouseSlowdown;

        private readonly Thread _capsLockThread;
        private readonly AutoResetEvent _capsLockAutoResetEvent;

        private bool _lastCapsPressed;
        private bool _capsToggledStateAtBeginning;

        public MouseHookHandler(Options options)
        {
            _capsLockAutoResetEvent = new AutoResetEvent(false);
            _capsLockThread = new Thread(CapsLockLogic, 65536);
            _capsLockThread.Start();
            
            _mouseSlowdown = new MouseSlowdown();
            _mouseLogic = new MouseLogic(options);
            _mouseLogic.LastCursorBoundsChanged += (sender, args) => _cursorScreenBounds = args.Bounds;
        }

        private void CapsLockLogic()
        {
            while (true)
            {
                _capsLockAutoResetEvent.WaitOne();

                while (true)
                {
                    _capsLockAutoResetEvent.WaitOne(250);
                    var state = Win32Keyboard.KeyStates(Keys.CapsLock);
                    if ((state & KeyPressedStates.KeyPressed) == 0)
                    {
                        Win32Keyboard.SetCapsLockState(_capsToggledStateAtBeginning);
                        break;
                    }
                }
            }
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
                Application.Run(ctx);

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

                var capsState = Win32Keyboard.KeyStates(Keys.CapsLock);
                var capsPressed = (capsState & KeyPressedStates.KeyPressed) != 0;

                if (capsPressed != _lastCapsPressed)
                {
                    // Debug.WriteLine($"{capsState}: {capsPressed}, {_lastCapsPressed}, {_capsToggledStateAtBeginning}");
                    if (capsPressed)
                        _capsToggledStateAtBeginning = (capsState & KeyPressedStates.KeyToggled) == 0;

                    _lastCapsPressed = capsPressed;
                    _capsLockAutoResetEvent.Set();
                }

                if (capsPressed && NativeMethods.GetCursorPos(out var cursor1))
                {
                    Win32Mouse.SetCursorPos(_mouseSlowdown.SlowCursor(mouse, cursor1));
                    return (IntPtr) 1;
                }

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
