/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace MouseUnSnag
{
    // The Program class deals with the low-level mouse events.

    public class Program
    {
        private IntPtr LLMouse_hookhand = IntPtr.Zero;
        private Point LastMouse = new Point (0, 0);
        private IntPtr ThisModHandle = IntPtr.Zero;
        public int NJumps { get; private set; } = 0;

        // Command line option flags

        public bool IsUnstickEnabled { get; set; } = true;
        public bool IsJumpEnabled { get; set; } = true;
        public bool IsScreenWrapEnabled { get; set; } = true;

        private IntPtr SetHook (int HookNum, NativeMethods.HookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess ())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                if (ThisModHandle == IntPtr.Zero)
                    ThisModHandle = NativeMethods.GetModuleHandle (curModule.ModuleName);
                return NativeMethods.SetWindowsHookEx (HookNum, proc, ThisModHandle, 0);
            }
        }

        private void UnsetHook (ref IntPtr hookHand)
        {
            if (hookHand == IntPtr.Zero)
                return;

            NativeMethods.UnhookWindowsHookEx (hookHand);
            hookHand = IntPtr.Zero;
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
        private bool CheckJumpCursor (Point mouse, Point cursor, out Point NewCursor)
        {
            NewCursor = cursor; // Default is to not move cursor.

            // Gather pertinent information about cursor, mouse, screens.
            SnagScreen cursorScreen = SnagScreen.WhichScreen (cursor);
            SnagScreen mouseScreen = SnagScreen.WhichScreen (mouse);
            bool IsStuck = (cursor != LastMouse) && (mouseScreen != cursorScreen);
            Point StuckDirection = GeometryUtil.OutsideDirection (cursorScreen.R, mouse);

            Debug.WriteLine ($" StuckDirection/Distance{StuckDirection}/{GeometryUtil.OutsideDistance(cursorScreen.R, mouse)} " +
                             $"cur_mouse:{mouse}  prev_mouse:{LastMouse} ==? cursor:{cursor} (OnMon#{cursorScreen}/{mouseScreen})  " +
                             $"#UnSnags {NJumps}   {(IsStuck ? "--STUCK--" : "         ")}   ");

            LastMouse = mouse;

            // Let caller know we did NOT jump the cursor.
            if (!IsStuck)
                return false;

            SnagScreen jumpScreen = SnagScreen.ScreenInDirection (StuckDirection, cursorScreen.R);

            // If the mouse "location" (which can take on a value beyond the current
            // cursor screen) has a value, then it is "within" another valid screen
            // bounds, so just jump to it!
            if (mouseScreen != null)
            {
                if (!IsUnstickEnabled)
                    return false;
                NewCursor = mouse;
            }
            else if (jumpScreen != null)
            {
                if (!IsJumpEnabled)
                    return false;
                NewCursor = jumpScreen.R.ClosestBoundaryPoint (cursor);
            }
            else if (StuckDirection.X != 0)
            {
                if (!IsScreenWrapEnabled)
                    return false;

                SnagScreen wrapScreen = SnagScreen.WrapScreen (StuckDirection, cursor);
                Point wrapPoint = new Point(
                    StuckDirection.X==1?wrapScreen.R.Left:wrapScreen.R.Right - 1, cursor.Y);

                // Don't wrap cursor if jumping is disabled and it would need to jump.
                if (!IsJumpEnabled && !wrapScreen.R.Contains(wrapPoint))
                    return false;

                NewCursor = wrapScreen.R.ClosestBoundaryPoint(wrapPoint);
            }
            else
            {
                return false;
            }

            ++NJumps;
            Debug.WriteLine("\n -- JUMPED!!! --");
            return true;
        }

        // Called whenever the mouse moves. This routine leans entirely on the
        // CheckJumpCursor() routine to see if there is any need to "mess with" the cursor
        // position, to make it jump from one monitor to another.
        private IntPtr LLMouseHookCallback (int nCode, uint wParam, IntPtr lParam)
        {
            if ((nCode < 0) || (wParam != NativeMethods.WM_MOUSEMOVE) || UpdatingDisplaySettings)
                goto ExitToNextHook;

            var hookStruct = (NativeMethods.MSLLHOOKSTRUCT) Marshal.PtrToStructure (lParam, typeof (NativeMethods.MSLLHOOKSTRUCT));
            Point mouse = hookStruct.pt;

            // If we jump the cursor, then we return 1 here to tell the OS that we
            // have handled the message, so it doesn't call SetCursorPos() right
            // after we do, and "undo" our call to SetCursorPos().
            if (NativeMethods.GetCursorPos(out Point cursor) && CheckJumpCursor (mouse, cursor, out Point NewCursor)) {
                NativeMethods.SetCursorPos(NewCursor);
                return (IntPtr) 1;
            }

            // Default is to let the OS handle the mouse events, when "return" does not happen in
            // if() clause above.
            ExitToNextHook:
            return NativeMethods.CallNextHookEx (LLMouse_hookhand, nCode, wParam, lParam);
        }

        private bool UpdatingDisplaySettings = false;

        private void Event_DisplaySettingsChanged (object sender, EventArgs e)
        {
            UpdatingDisplaySettings=true;
            Debug.WriteLine ("\nDisplay Settings Changed...");
            SnagScreen.Init (Screen.AllScreens);
            Debug.WriteLine (SnagScreen.GetScreenInformation ());
            UpdatingDisplaySettings=false;
        }

        // Need to explicitly keep a reference to this, so it does not get "garbage collected."
        private NativeMethods.HookProc MouseHookDelegate = null;

        private void Run (string[] args)
        {
            // DPI Awareness API is not available on older OS's, but they work in
            // physical pixels anyway, so we just ignore if the call fails.
            try
            {
                NativeMethods.SetProcessDpiAwareness (NativeMethods.PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);
            }
            catch (DllNotFoundException)
            {
                Debug.WriteLine ("No SHCore.DLL. No problem.");
            }

            // Parse command line arguments
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-s":
                        IsUnstickEnabled = false;
                        break;
                    case "+s":
                        IsUnstickEnabled = true;
                        break;
                    case "-j":
                        IsJumpEnabled = false;
                        break;
                    case "+j":
                        IsJumpEnabled = true;
                        break;
                    case "-w":
                        IsScreenWrapEnabled = false;
                        break;
                    case "+w":
                        IsScreenWrapEnabled = true;
                        break;
                    default:
                        string exeName = Environment.GetCommandLineArgs()[0];
                        Console.WriteLine($"Usage: {exeName} [options ...]");
                        Console.WriteLine("\t-s    Disables mouse un-sticking.");
                        Console.WriteLine("\t+s    Enables mouse un-sticking. Default.");
                        Console.WriteLine("\t-j    Disables mouse jumping.");
                        Console.WriteLine("\t+j    Enables mouse jumping. Default.");
                        Console.WriteLine("\t-w    Disables mouse wrapping.");
                        Console.WriteLine("\t+w    Enables mouse wrapping. Default.");
                        Environment.Exit(1);
                        break;
                }
            }

            SnagScreen.Init (Screen.AllScreens);

            Debug.WriteLine (SnagScreen.GetScreenInformation());

            // Get notified of any screen configuration changes.
            SystemEvents.DisplaySettingsChanged += Event_DisplaySettingsChanged;

            // Keep a reference to the delegate, so it does not get garbage collected.
            MouseHookDelegate = LLMouseHookCallback;
            LLMouse_hookhand = SetHook (NativeMethods.WH_MOUSE_LL, MouseHookDelegate);

            // This is the one that runs "forever" while the application is alive, and handles
            // events, etc. This application is ABSOLUTELY ENTIRELY driven by the LLMouseHook
            // and DisplaySettingsChanged events.
            Application.Run (new MyCustomApplicationContext(this));

            Debug.WriteLine ("Exiting!!!");
            UnsetHook (ref LLMouse_hookhand);
            SystemEvents.DisplaySettingsChanged -= Event_DisplaySettingsChanged;
        }

        [STAThread]
        public static void Main (string[] args)
        {
            // Make sure the MouseUnSnag.exe has only one instance running at a time.
            using (new Mutex(initiallyOwned: true, "__MouseUnSnag_EXE__", out bool createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        "MouseUnSnag is already running!! Quitting this instance...",
                        "MouseUnSnag",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(defaultValue: false);

                new Program().Run(args);
            }
        }
    }
}
