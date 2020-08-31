using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using MouseUnSnag.CommandLine;
using MouseUnSnag.Hooking;

namespace MouseUnSnag
{
    internal class MouseHookHandler
    {
        private IntPtr _llMouseHookhand = IntPtr.Zero;
        private Point _lastMouse = new Point(0, 0);
        private int _nJumps;

        private HookHandler _hookHandler;
        

        /// <summary>
        /// Command Line Options
        /// </summary>
        internal Options Options { get; }

        public MouseHookHandler(Options options)
        {
            Options = options;
        }


        public void Run()
        {
            SnagScreen.Init(Screen.AllScreens);
            Debug.WriteLine(SnagScreen.GetScreenInformation());

            // Get notified of any screen configuration changes.
            SystemEvents.DisplaySettingsChanged += Event_DisplaySettingsChanged;

            // Keep a reference to the delegate, so it does not get garbage collected.
            _mouseHookDelegate = LlMouseHookCallback;
            _hookHandler = new HookHandler();
            _llMouseHookhand = _hookHandler.SetHook(NativeMethods.WhMouseLl, _mouseHookDelegate);

            
            using (var ctx = new MyCustomApplicationContext(Options))
            {
                // This is the one that runs "forever" while the application is alive, and handles
                // events, etc. This application is ABSOLUTELY ENTIRELY driven by the LLMouseHook
                // and DisplaySettingsChanged events.
                Application.Run(ctx);
            }
            

            Debug.WriteLine("Exiting...");
            HookHandler.UnsetHook(ref _llMouseHookhand);
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
            if ((nCode == 0) && (wParam == NativeMethods.WmMousemove) && !_updatingDisplaySettings)
            {
                var hookStruct = (NativeMethods.Msllhookstruct)Marshal.PtrToStructure(lParam, typeof(NativeMethods.Msllhookstruct));
                var mouse = hookStruct.pt;

                if (NativeMethods.GetCursorPos(out var cursor) && CheckJumpCursor(mouse, cursor, out var newCursor))
                {
                    NativeMethods.SetCursorPos(newCursor);
                    return (IntPtr) 1;
                }
            }

            return NativeMethods.CallNextHookEx(_llMouseHookhand, nCode, wParam, lParam);
        }


        // CheckJumpCursor() returns TRUE, ONLY if the cursor is "stuck". By "stuck" we
        // specifically mean that the user is trying to move the mouse beyond the boundaries of
        // the screen currently containing the cursor. This is determined when the *current*
        // cursor position does not equal the *previous* mouse position. If there is another
        // adjacent screen (or a "wrap around" screen), then we can consider moving the mouse
        // onto that screen.
        //
        // Note that this is ENTIRELY a *GEOMETRIC* method. Screens are "rectangles", and the
        // cursor and mouse are "points." The mouse/cursor hardware interaction (obtaining
        // current mouse and cursor information) is handled in routines further below, and any
        // Screen changes are handled by the DisplaySettingsChanged event. There are no
        // hardware or OS/Win32 references or interactions here.
        /// <summary>
        /// Processes mouse events
        /// </summary>
        /// <param name="mouse">Position of the mouse. Can be outside the screen</param>
        /// <param name="cursor">Position of the cursor. Always within screen bounds</param>
        /// <param name="newCursor">New position that shall be taken over</param>
        /// <returns>True if the newCursor Position shall be taken over</returns>
        private bool CheckJumpCursor(Point mouse, Point cursor, out Point newCursor)
        {
            newCursor = cursor; // Default is to not move cursor.

            // Gather pertinent information about cursor, mouse, screens.
            var lastScreen = SnagScreen.WhichScreen(_lastMouse);
            var cursorScreen = SnagScreen.WhichScreen(cursor);
            var mouseScreen = SnagScreen.WhichScreen(mouse);
            var isStuck = (cursor != _lastMouse) && (mouseScreen != cursorScreen) || (mouseScreen != lastScreen);
            var stuckDirection = GeometryUtil.OutsideDirection(cursorScreen.R, mouse);

            Debug.WriteLine($" StuckDirection/Distance{stuckDirection}/{GeometryUtil.OutsideDistance(cursorScreen.R, mouse)} " +
                             $"cur_mouse:{mouse}  prev_mouse:{_lastMouse} ==? cursor:{cursor} (OnMon#{cursorScreen}/{mouseScreen})  " +
                             $"#UnSnags {_nJumps}   {(isStuck ? "--STUCK--" : "         ")}   ");

            _lastMouse = mouse;

            // Let caller know we did NOT jump the cursor.
            if (!isStuck)
                return false;

            var jumpScreen = SnagScreen.ScreenInDirection(stuckDirection, cursorScreen.R);

            // If the mouse "location" (which can take on a value beyond the current
            // cursor screen) has a value, then it is "within" another valid screen
            // bounds, so just jump to it!
            if (mouseScreen != null)
            {
                if (!Options.Unstick)
                    return false;

                if (lastScreen != mouseScreen && lastScreen != null)
                {
                    var nc = mouse;
                    nc.Y = nc.Y * mouseScreen.EffectiveDpi / lastScreen.EffectiveDpi;
                    Debug.WriteLine($"R: {nc}, {lastScreen.EffectiveDpi}>{mouseScreen.EffectiveDpi}");
                    newCursor = nc;
                }
                else
                {
                    newCursor = mouse;
                }
            }
            else if (jumpScreen != null)
            {
                if (!Options.Jump)
                    return false;
                newCursor = jumpScreen.R.ClosestBoundaryPoint(cursor);
            }
            else if (stuckDirection.X != 0)
            {
                if (!Options.Wrap)
                    return false;

                var wrapScreen = SnagScreen.WrapScreen(stuckDirection, cursor);
                var wrapPoint = new Point(
                    stuckDirection.X == 1 ? wrapScreen.R.Left : wrapScreen.R.Right - 1, cursor.Y);

                // Don't wrap cursor if jumping is disabled and it would need to jump.
                if (!Options.Jump && !wrapScreen.R.Contains(wrapPoint))
                    return false;

                newCursor = wrapScreen.R.ClosestBoundaryPoint(wrapPoint);
            }
            else
            {
                return false;
            }

            ++_nJumps;
            Debug.WriteLine("\n -- JUMPED!!! --");
            return true;
        }

        

        // Need to explicitly keep a reference to this, so it does not get "garbage collected."
        private NativeMethods.HookProc _mouseHookDelegate;

        
        private volatile bool _updatingDisplaySettings;
        private void Event_DisplaySettingsChanged(object sender, EventArgs e)
        {
            _updatingDisplaySettings = true;
            Debug.WriteLine("\nDisplay Settings Changed...");
            SnagScreen.Init(Screen.AllScreens);
            Debug.WriteLine(SnagScreen.GetScreenInformation());
            _updatingDisplaySettings = false;
        }
    }
}
