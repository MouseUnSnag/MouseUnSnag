/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
        /// <summary> List of All Screens </summary>
        private static SnagScreen[] All { get; set; }
        /// <summary> List of Screens that are considered left most </summary>
        private static List<SnagScreen> LeftMost { get; set; }
        /// <summary> List of Screens that are considered right most </summary>
        private static List<SnagScreen> RightMost { get; set; }
        /// <summary> List of Screens that are considered top most </summary>
        private static List<SnagScreen> TopMost { get; set; }
        /// <summary> List of Screens that are considered bottom most </summary>
        private static List<SnagScreen> BottomMost { get; set; }
        /// <summary> Bounding box that encompasses all screens </summary>
        private static Rectangle BoundingBox { get; set; }

        /// <summary> The associated <see cref="Screen"/> </summary>
        private Screen Screen { get; } // Points to the entry in Screen.AllScreens[].
        /// <summary> Screen Number </summary>
        private int ScreenNumber { get; }
        /// <summary> Shortcut to screen.Bounds </summary>
        public Rectangle R => Screen.Bounds;
        /// <summary> Effective DPI </summary>
        public int EffectiveDpi { get; }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString() => ScreenNumber.ToString(CultureInfo.InvariantCulture);

        /// <summary> List of Screens that are to the left </summary>
        public List<SnagScreen> ToLeft { get; }
        /// <summary> List of Screens that are to the right </summary>
        public List<SnagScreen> ToRight { get; }
        /// <summary> List of Screens that are above </summary>
        public List<SnagScreen> Above { get; }
        /// <summary> List of Screens that are below </summary>
        public List<SnagScreen> Below { get; }

        /// <summary>
        /// Initialize a new <see cref="SnagScreen"/>
        /// </summary>
        /// <param name="S"><see cref="Screen"/></param>
        /// <param name="ScreenNum">Screen Number</param>
        public SnagScreen(Screen S, int ScreenNum)
        {
            EffectiveDpi = (int)NativeMethods.GetDpi(S, NativeMethods.DpiType.Effective);
            Screen = S;
            ScreenNumber = ScreenNum;
            ToLeft = new List<SnagScreen>();
            ToRight = new List<SnagScreen>();
            Above = new List<SnagScreen>();
            Below = new List<SnagScreen>();
        }

        private bool IsLeftmost => ToLeft.Count == 0;
        private bool IsRightmost => ToRight.Count == 0;
        private bool IsTopmost => Above.Count == 0;
        private bool IsBottommost => Below.Count == 0;

        // If s is immediately adjacent to (shares a border with) us, then add it to the
        // appropriate direction list. If s is not "touching" us, then it will not get added to
        // any list. s can be added to at most one list (hence use of "else if" instead of just
        // a sequence of "if's").
        private void AddDirectionTo(SnagScreen s)
        {
            if ((R.Right == s.R.Left) && GeometryUtil.OverlapY(R, s.R)) ToRight.Add(s);
            else if ((R.Left == s.R.Right) && GeometryUtil.OverlapY(R, s.R)) ToLeft.Add(s);
            else if ((R.Top == s.R.Bottom) && GeometryUtil.OverlapX(R, s.R)) Above.Add(s);
            else if ((R.Bottom == s.R.Top) && GeometryUtil.OverlapX(R, s.R)) Below.Add(s);
        }

        // Loop through Screen.AllScreens[] array to initialize ourselves.
        public static void Init(Screen[] allScreens)
        {
            if (allScreens == null)
                throw new ArgumentNullException(nameof(allScreens));

            var n = allScreens.Length;
            TopMost = new List<SnagScreen>();
            BottomMost = new List<SnagScreen>();
            LeftMost = new List<SnagScreen>();
            RightMost = new List<SnagScreen>();

            BoundingBox = new Rectangle(0, 0, 0, 0);

            // First pass, populate our All[] array with all the screens.
            All = new SnagScreen[n];
            for (var i = 0; i < n; ++i)
                All[i] = new SnagScreen(Screen.AllScreens[i], i);

            // Now determine their geometric relationships. Yes this is O(N^2), but
            // usually N (number of monitors) is not too large. There may be more
            // efficient approaches, but this is very simple, clear, and
            // straightforward, and it is not called often (only when program
            // starts, and after any change in monitor configuration).
            foreach (var sn in All)
            {
                // Add direction from this SN screen to each of the other screens.
                foreach (var s in All)
                    sn.AddDirectionTo(s);

                // Where appropriate, add ourselves to the lists of outermost screens.
                if (sn.IsLeftmost) LeftMost.Add(sn);
                if (sn.IsRightmost) RightMost.Add(sn);
                if (sn.IsTopmost) TopMost.Add(sn);
                if (sn.IsBottommost) BottomMost.Add(sn);

                BoundingBox = Rectangle.Union(BoundingBox, sn.R);
            }
        }

        /// <summary>
        /// Get Screen Information as Text
        /// </summary>
        /// <returns></returns>
        public static string GetScreenInformation()
        {
            var sb = new StringBuilder();
            var n = All.Length;
            sb.AppendLine($"There {((n > 1) ? "are" : "is")} {n} SCREEN{((n > 1) ? "S" : "")}:");
            var i = 0;

            foreach (var s in All)
            {
                var dpiEffective = NativeMethods.GetDpi(s.Screen, NativeMethods.DpiType.Effective);
                var r = s.R;

                sb.AppendLine(
                    $"   {i}: ({r.Left},{r.Top})-({r.Right},{r.Bottom})   Size:({r.Width},{r.Height}) " +
                    $"L({AsString(s.ToLeft)}),R({AsString(s.ToRight)}),A({AsString(s.Above)}),B({AsString(s.Below)})    " +
                    $"DPI(Raw/Eff/Ang): {NativeMethods.GetDpi(s.Screen, NativeMethods.DpiType.Raw)}/{dpiEffective}/{NativeMethods.GetDpi(s.Screen, NativeMethods.DpiType.Angular)}  " +
                    $"Screen Scaling: {Math.Round(dpiEffective / 96.0 * 100)}%   \r"); //  {S.DeviceName}     \r");
                ++i;
            }

            sb.AppendLine(
                $"Rtmost({AsString(RightMost)}), Lfmost({AsString(LeftMost)}), " +
                $"Topmost({AsString(TopMost)}), Btmost({AsString(BottomMost)})   " +
                $"BoundingBox{BoundingBox}");

            return sb.ToString();

            string AsString(List<SnagScreen> L) => string.Join(",", L.Select(sn => sn.ScreenNumber));
        }

        // Find which screen the point is on. If it is not on one, return null.
        /// <summary>
        /// Find which screen the point is on. Returns null if not on any screen
        /// </summary>
        /// <param name="P">Point to check</param>
        /// <returns>Screen on which the point is, or null otherwise</returns>
        public static SnagScreen WhichScreen(Point P)
        {
            return All.FirstOrDefault(s => s.R.Contains(P));
        }

        /// <summary>
        /// Find the first monitor (first one we come across in the for() loop)
        /// that is in the direction of the point.
        /// </summary>
        /// <param name="Dir">Direction</param>
        /// <param name="CurScreen">Current Screen</param>
        /// <returns></returns>
        public static SnagScreen ScreenInDirection(Point Dir, Rectangle CurScreen)
        {
            // Screen must be strictly above/below/beside. For instance, for a monitor to be
            // "above", the monitor's Bottom equal to the current screen's Top ("current
            // screen" is where the Cursor (NOT the mouse!!) is currently).
            foreach (var s in All)
            {
                if (((Dir.X == 1) && (CurScreen.Right == s.R.Left)) ||
                    ((Dir.X == -1) && (CurScreen.Left == s.R.Right)) ||
                    ((Dir.Y == 1) && (CurScreen.Bottom == s.R.Top)) ||
                    ((Dir.Y == -1) && (CurScreen.Top == s.R.Bottom)))
                {
                    return s;
                }
            }
            return null;

            // May want to update the above routine, which arbitrarily selects the monitor that
            // happens to come first in the for() loop. We should probably do a little extra work,
            // and select the monitor that is closest to the mouse position.
        }


        /// <summary>
        /// Find the best screen to "wrap" around the cursor, either horizontally or
        /// vertically. We consider only the "OuterMost" screens. For instance, if
        /// the mouse is moving to the left, we consider only the screens in the
        /// RightMost[] array.
        /// </summary>
        /// <param name="Dir">Point that represents the direction</param>
        /// <param name="Cursor">Point that represents the cursor</param>
        /// <returns></returns>
        public static SnagScreen WrapScreen(Point Dir, Point Cursor)
        {
            if (Dir.X == 0)
                return All[0];

            // Find closest Left- or Right-most screen, in Y direction.
            var distClosest = int.MaxValue;
            var screensToCheck = (Dir.X == 1 ? LeftMost : RightMost);
            var ws = screensToCheck.FirstOrDefault() ?? All[0]; 
            foreach (var s in screensToCheck)
            {
                var dist = Math.Abs(GeometryUtil.OutsideYDistance(s.R, Cursor));
                if (dist >= distClosest)
                    continue;

                distClosest = dist;
                ws = s;
            }
            return ws;
        }
    }
}
