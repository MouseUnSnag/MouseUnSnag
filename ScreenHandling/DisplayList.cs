using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseUnSnag.ScreenHandling
{
    public class DisplayList
    {
        /// <summary> List of All Screens </summary>
        public List<SnagScreen> All { get; }

        /// <summary> List of Screens that are considered left most </summary>
        public List<SnagScreen> LeftMost { get; }
        /// <summary> List of Screens that are considered right most </summary>
        public List<SnagScreen> RightMost { get; }
        /// <summary> List of Screens that are considered top most </summary>
        public List<SnagScreen> TopMost { get; }
        /// <summary> List of Screens that are considered bottom most </summary>
        public List<SnagScreen> BottomMost { get; }

        /// <summary> Bounding box that encompasses all screens </summary>
        public Rectangle BoundingBox { get; }

        public DisplayList(IEnumerable<Screen> screens)
        {
            if (screens == null)
                throw new ArgumentNullException(nameof(screens));

            var boundingBox = Rectangle.Empty;
            var snagScreens = screens.Select((x, i) => new SnagScreen(x, i)).ToList();
            foreach (var snagScreen in snagScreens)
            {
                foreach (var s in snagScreens) 
                    snagScreen.AddDirectionTo(s);

                boundingBox = Rectangle.Union(boundingBox, snagScreen.R);
            }

            All = snagScreens;
            BoundingBox = boundingBox;
            LeftMost = snagScreens.Where(x => x.ToLeft.Count == 0).ToList();
            RightMost = snagScreens.Where(x => x.ToRight.Count == 0).ToList();
            TopMost = snagScreens.Where(x => x.Above.Count == 0).ToList();
            BottomMost = snagScreens.Where(x => x.Below.Count == 0).ToList();

        }

        /// <summary>
        /// Find which screen the point is on. Returns null if not on any screen
        /// </summary>
        /// <param name="P">Point to check</param>
        /// <returns>Screen on which the point is, or null otherwise</returns>
        public SnagScreen WhichScreen(Point P)
        {
            return All.FirstOrDefault(s => s.R.Contains(P));
        }

        /// <summary>
        /// Find the first monitor (first one we come across in the for() loop)
        /// that is in the direction of the point.
        /// </summary>
        /// <param name="dir">Direction</param>
        /// <param name="curScreen">Current Screen</param>
        /// <returns></returns>
        public SnagScreen ScreenInDirection(Point dir, Rectangle curScreen)
        {
            // Screen must be strictly above/below/beside. For instance, for a monitor to be
            // "above", the monitor's Bottom equal to the current screen's Top ("current
            // screen" is where the Cursor (NOT the mouse!!) is currently).
            foreach (var s in All)
            {
                if (((dir.X == 1) && (curScreen.Right == s.R.Left)) ||
                    ((dir.X == -1) && (curScreen.Left == s.R.Right)) ||
                    ((dir.Y == 1) && (curScreen.Bottom == s.R.Top)) ||
                    ((dir.Y == -1) && (curScreen.Top == s.R.Bottom)))
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
        /// <param name="dir">Point that represents the direction</param>
        /// <param name="cursor">Point that represents the cursor</param>
        /// <returns></returns>
        public SnagScreen WrapScreen(Point dir, Point cursor)
        {
            if (dir.X == 0)
                return All[0];

            // Find closest Left- or Right-most screen, in Y direction.
            var distClosest = int.MaxValue;
            var screensToCheck = (dir.X == 1 ? LeftMost : RightMost);
            var ws = screensToCheck.FirstOrDefault() ?? All[0]; 
            foreach (var s in screensToCheck)
            {
                var dist = Math.Abs(GeometryUtil.OutsideYDistance(s.R, cursor));
                if (dist >= distClosest)
                    continue;

                distClosest = dist;
                ws = s;
            }
            return ws;
        }


        /// <summary>
        /// Get Screen Information as Text
        /// </summary>
        /// <returns></returns>
        public string GetScreenInformation()
        {
            var sb = new StringBuilder();
            var n = All.Count;
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

    }
}
