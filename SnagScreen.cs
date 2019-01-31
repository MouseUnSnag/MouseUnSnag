/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MouseUnSnag
{
    // SnagScreen keeps track of the physical screens/monitors attached to the system, and adds
    // some members to keep track of screen relative geometry.
    //
    // N.B. (Note Well!) There is a non-static "instance" of SnagScreen for EACH physical
    // screen/monitor in the system. The *static* members are relative to ALL the screens/monitors.

    public class SnagScreen
    {
        public static SnagScreen[] All;
        public static List<SnagScreen> LeftMost, RightMost, TopMost, BottomMost;
        public static Rectangle BoundingBox;  // Rectangle that contains all screens.

        public Screen screen; // Points to the entry in Screen.AllScreens[].
        public int Num; // Index into Screen.AllScreens[] for this SnagScreen object.
        public Rectangle R => screen.Bounds; // Shortcut to screen.Bounds.
        public override string ToString() => Num.ToString();

        public List<SnagScreen> ToLeft, ToRight, Above, Below;

        // Initialize each SnagScreen from each member of Screen.AllScreens[] array.
        public SnagScreen (Screen S, int ScreenNum)
        {
            screen = S;
            Num = ScreenNum;
            ToLeft = new List<SnagScreen> ();
            ToRight = new List<SnagScreen> ();
            Above = new List<SnagScreen> ();
            Below = new List<SnagScreen> ();
        }

        public bool IsLeftmost => ToLeft.Count == 0;
        public bool IsRightmost => ToRight.Count == 0;
        public bool IsTopmost => Above.Count == 0;
        public bool IsBottommost => Below.Count == 0;

        // If s is immediately adjacent to (shares a border with) us, then add it to the
        // appropriate direction list. If s is not "touching" us, then it will not get added to
        // any list. s can be added to at most one list (hence use of "else if" instead of just
        // a sequence of "if's").
        public void AddDirectionTo (SnagScreen s)
        {
            if      ((R.Right == s.R.Left) && GeometryUtil.OverlapY (R, s.R)) ToRight.Add (s);
            else if ((R.Left == s.R.Right) && GeometryUtil.OverlapY (R, s.R)) ToLeft.Add (s);
            else if ((R.Top == s.R.Bottom) && GeometryUtil.OverlapX (R, s.R)) Above.Add (s);
            else if ((R.Bottom == s.R.Top) && GeometryUtil.OverlapX (R, s.R)) Below.Add (s);
        }

        // Loop through Screen.AllScreens[] array to initialize ourselves.
        public static void Init (Screen[] AllScreens)
        {
            var N = AllScreens.Length;
            TopMost = new List<SnagScreen> ();
            BottomMost = new List<SnagScreen> ();
            LeftMost = new List<SnagScreen> ();
            RightMost = new List<SnagScreen> ();

            BoundingBox = new Rectangle(0,0,0,0);

            // First pass, populate our All[] array with all the screens.
            All = new SnagScreen[N];
            for (int i = 0; i < N; ++i)
                All[i]  = new SnagScreen (Screen.AllScreens[i], i);

            // Now determine their geometric relationships. Yes this is O(N^2), but
            // usually N (number of monitors) is not too large. There may be more
            // efficient approaches, but this is very simple, clear, and
            // straightforward, and it is not called often (only when program
            // starts, and after any change in monitor configuration).
            foreach (var SN in All)
            {
                // Add direction from this SN screen to each of the other screens.
                foreach (var s in All)
                    SN.AddDirectionTo (s);

                // Where appropriate, add ourselves to the lists of outermost screens.
                if (SN.IsLeftmost) LeftMost.Add (SN);
                if (SN.IsRightmost) RightMost.Add (SN);
                if (SN.IsTopmost) TopMost.Add (SN);
                if (SN.IsBottommost) BottomMost.Add (SN);

                BoundingBox = Rectangle.Union(BoundingBox, SN.R);
            }
        }

        public static string GetScreenInformation ()
        {
            var sb = new StringBuilder();
            int N = All.Length;
            sb.AppendLine ($"There {((N>1)?"are":"is")} {N} SCREEN{((N>1)?"S":"")}:");
            int i = 0;

            foreach (var S in All)
            {
                var DPIEffective = NativeMethods.GetDpi (S.screen, NativeMethods.DpiType.Effective);
                var R = S.R;

                sb.AppendLine(
                    $"   {i}: ({R.Left},{R.Top})-({R.Right},{R.Bottom})   Size:({R.Width},{R.Height}) "+
                    $"L({AsString(S.ToLeft)}),R({AsString(S.ToRight)}),A({AsString(S.Above)}),B({AsString(S.Below)})    "+
                    $"DPI(Raw/Eff/Ang): {NativeMethods.GetDpi(S.screen, NativeMethods.DpiType.Raw)}/{DPIEffective}/{NativeMethods.GetDpi(S.screen, NativeMethods.DpiType.Angular)}  "+
                    $"Screen Scaling: {Math.Round(DPIEffective/96.0*100)}%   \r"); //  {S.DeviceName}     \r");
                ++i;
            }

            sb.AppendLine(
                $"Rtmost({AsString(RightMost)}), Lfmost({AsString(LeftMost)}), "+
                $"Topmost({AsString(TopMost)}), Btmost({AsString(BottomMost)})   "+
                $"BoundingBox{BoundingBox}");

            return sb.ToString ();

            string AsString (List<SnagScreen> L) => string.Join (",", L.Select (sn => sn.Num));
        }

        // Find which screen the point is on. If it is not on one, return null.
        public static SnagScreen WhichScreen (Point P)
        {
            foreach(var S in SnagScreen.All)
                if(S.R.Contains(P))
                    return S;

            return null;
        }

        // Find the first monitor (first one we come across in the for() loop)
        // that is in the direction of the point.
        public static SnagScreen ScreenInDirection (Point Dir, Rectangle CurScreen)
        {
            // Screen must be strictly above/below/beside. For instance, for a monitor to be
            // "above", the monitor's Bottom equal to the current screen's Top ("current
            // screen" is where the Cursor (NOT the mouse!!) is currently).
            foreach(var S in SnagScreen.All)
            {
                if (((Dir.X == 1) && (CurScreen.Right == S.R.Left)) ||
                    ((Dir.X == -1) && (CurScreen.Left == S.R.Right)) ||
                    ((Dir.Y == 1) && (CurScreen.Bottom == S.R.Top)) ||
                    ((Dir.Y == -1) && (CurScreen.Top == S.R.Bottom)))

                    return S;
            }
            return null;
        }

        // May want to update the above routine, which arbitrarily selects the monitor that
        // happens to come first in the for() loop. We should probably do a little extra work,
        // and select the monitor that is closest to the mouse position.

        // Find the monitor that is closest to the point.
        //public static SnagScreen ScreenInDirection()
        //{
        //}

        // Find the best point to "wrap" around the cursor, either horizontally or
        // vertically. We consider only the "OuterMost" screens. For instance, if
        // the mouse is moving to the left, we consider only the screens in the
        // RightMost[] array.
        public static Point WrapPoint(Point Dir, Point Cursor)
        {
            int DistClosest = int.MaxValue;
            SnagScreen WS = null; // Our "wrap screen".

            if(Dir.X != 0) {
                // Find closest Left- or Right-most screen, in Y direction.
                foreach(var S in (Dir.X==1 ? LeftMost : RightMost)) {
                    int dist = Math.Abs(GeometryUtil.OutsideYDistance(S.R, Cursor));
                    if(dist < DistClosest) {
                        DistClosest = dist;
                        WS = S;
                    }
                }
                return WS.R.ClosestBoundaryPoint(new Point(Dir.X==1?WS.R.Left:WS.R.Right, Cursor.Y));
            }

            // We should never get here, but if we do, just return the current
            // Cursor location.
            return Cursor;
        }
    }
}
