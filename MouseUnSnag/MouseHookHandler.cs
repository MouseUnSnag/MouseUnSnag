using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using MouseUnSnag.CommandLine;
using MouseUnSnag.Hooking;
using MouseUnSnag.ScreenHandling;

namespace MouseUnSnag
{
    internal class MouseHookHandler
    {
        private DisplayList _displays;

        private int _nEvaluationCount;
        private int _nJumps;
        private volatile bool _updatingDisplaySettings;

        // Need to explicitly keep a reference to this, so it does not get "garbage collected."
        private NativeMethods.HookProc _mouseHookDelegate;

        private IntPtr _llMouseHookhand = IntPtr.Zero;
        private Point _lastMouse = new Point(0, 0);
        
        private HookHandler _hookHandler;

        private Rectangle _lastScreenRect = Rectangle.Empty;

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
            UpdateScreens();

            // Get notified of any screen configuration changes.
            SystemEvents.DisplaySettingsChanged += Event_DisplaySettingsChanged;

            // Keep a reference to the delegate, so it does not get garbage collected.
            _mouseHookDelegate = LlMouseHookCallback;
            _hookHandler = new HookHandler();
            _llMouseHookhand = _hookHandler.SetHook(NativeMethods.WhMouseLl, _mouseHookDelegate);

            using (var ctx = new TrayIconApplicationContext(Options))
                Application.Run(ctx);

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
                var hookStruct = (NativeMethods.Msllhookstruct) Marshal.PtrToStructure(lParam, typeof(NativeMethods.Msllhookstruct));
                var mouse = hookStruct.pt;

                if (NativeMethods.GetCursorPos(out var cursor) && !_lastScreenRect.Contains(mouse) && CheckJumpCursor(mouse, cursor, out var newCursor))
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
            
            var lastScreen = _displays.WhichScreen(_lastMouse);
            var cursorScreen = _displays.WhichScreen(cursor);
            var mouseScreen = _displays.WhichScreen(mouse);
            var isStuck = (cursor != _lastMouse) && (mouseScreen != cursorScreen) || (mouseScreen != lastScreen);
            var stuckDirection = GeometryUtil.OutsideDirection(cursorScreen.Bounds, mouse);

            
            Debug.WriteLine($"{_nEvaluationCount} StuckDirection/Distance{stuckDirection}/{GeometryUtil.OutsideDistance(cursorScreen.Bounds, mouse)} " +
                             $"cur_mouse:{mouse}  prev_mouse:{_lastMouse} ==? cursor:{cursor} (OnMon#{cursorScreen}/{mouseScreen})  " +
                             $"#UnSnags {_nJumps}   {(isStuck ? "--STUCK--" : "         ")}   ");

            _nEvaluationCount += 1;

            _lastScreenRect = cursorScreen.Bounds;
            _lastMouse = mouse;

            // Let caller know we did NOT jump the cursor.
            if (!isStuck)
                return false;

            var jumpScreen = _displays.ScreenInDirection(stuckDirection, cursorScreen.Bounds);

            // If the mouse "location" (which can take on a value beyond the current
            // cursor screen) has a value, then it is "within" another valid screen
            // bounds, so just jump to it!
            if (mouseScreen != null)
            {
                Debug.WriteLine("MouseScreen");

                if (!Options.Unstick)
                    return false;

                if (Options.Rescale)
                {
                    // FIXME: This is a hack for mouse scaling of adjacent screens
                    var nc = mouse;
                    nc.Y = nc.Y * mouseScreen.Bounds.Height / cursorScreen.Bounds.Height;
                    Debug.WriteLine($"R: {nc}, {mouseScreen.Bounds.Height}>{cursorScreen.Bounds.Height}");
                    newCursor = nc;
                }
                else
                {
                    newCursor = mouse;
                }
            }
            else if (jumpScreen != null)
            {
                Debug.WriteLine("JumpScreen");
                if (!Options.Jump)
                    return false;

                // FIXME: This is a hack for mouse scaling of adjacent screens
                var c = cursor;
                if (Options.Rescale)
                    c.Y = c.Y * jumpScreen.Bounds.Height / cursorScreen.Bounds.Height;

                newCursor = jumpScreen.Bounds.ClosestBoundaryPoint(c);
            }
            else if (stuckDirection.X != 0)
            {
                Debug.WriteLine("Wrap");
                if (!Options.Wrap)
                    return false;

                var wrapScreen = _displays.WrapScreen(stuckDirection, cursor);
                var wrapPoint = new Point(
                    stuckDirection.X == 1 ? wrapScreen.Bounds.Left : wrapScreen.Bounds.Right - 1, cursor.Y);

                // Don't wrap cursor if jumping is disabled and it would need to jump.
                if (!Options.Jump && !wrapScreen.Bounds.Contains(wrapPoint))
                    return false;

                newCursor = wrapScreen.Bounds.ClosestBoundaryPoint(wrapPoint);
            }
            else
            {
                Debug.WriteLine("Nope");
                return false;
            }

            ++_nJumps;
            Debug.WriteLine("\n -- JUMPED!!! --");
            return true;
        }

        

        private void Event_DisplaySettingsChanged(object sender, EventArgs e)
        {
            UpdateScreens();
        }

        private void UpdateScreens()
        {
            _updatingDisplaySettings = true;
            Debug.WriteLine("\nDisplay Settings Changed...");
            var displays = new DisplayList(Screen.AllScreens.Select(x => (ScreenWrapper) x));
            Debug.WriteLine(displays.GetScreenInformation());
            _lastScreenRect = Rectangle.Empty;
            _displays = displays; // FIXME: this is not really threadsafe. 
            _updatingDisplaySettings = false;
        }
    }
}
