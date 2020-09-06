using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MouseUnSnag.ScreenHandling
{

    /// <summary>
    /// Wraps a List of Displays and provides useful methods
    /// </summary>
    public class DisplayList
    {
        /// <summary> List of All Screens </summary>
        public List<Display> All { get; }

        /// <summary> List of Screens that are considered left most </summary>
        public List<Display> LeftMost { get; }
        /// <summary> List of Screens that are considered right most </summary>
        public List<Display> RightMost { get; }
        /// <summary> List of Screens that are considered top most </summary>
        public List<Display> TopMost { get; }
        /// <summary> List of Screens that are considered bottom most </summary>
        public List<Display> BottomMost { get; }

        /// <summary> Bounding box that encompasses all screens </summary>
        public Rectangle BoundingBox { get; }

        /// <summary>
        /// Initializes a new <see cref="DisplayList"/>
        /// </summary>
        /// <param name="screens"></param>
        /// <exception cref="NullReferenceException">screens was null</exception>
        public DisplayList(IEnumerable<IScreen> screens)
        {
            if (screens == null)
                throw new ArgumentNullException(nameof(screens));

            var boundingBox = Rectangle.Empty;
            var displays = screens.Select((x, i) => new Display(x, i)).ToList();
            foreach (var display in displays)
            {
                foreach (var s in displays) 
                    display.Link(s);

                boundingBox = Rectangle.Union(boundingBox, display.Bounds);
            }

            All = displays;
            BoundingBox = boundingBox;
            LeftMost = displays.Where(x => x.ToLeft.Count == 0).ToList();
            RightMost = displays.Where(x => x.ToRight.Count == 0).ToList();
            TopMost = displays.Where(x => x.Above.Count == 0).ToList();
            BottomMost = displays.Where(x => x.Below.Count == 0).ToList();

        }

        /// <summary>
        /// Find which screen the point is on. Returns null if not on any screen
        /// </summary>
        /// <param name="P">Point to check</param>
        /// <returns>Screen on which the point is, or null otherwise</returns>
        public Display WhichScreen(Point P)
        {
            return All.FirstOrDefault(s => s.Bounds.Contains(P));
        }

        /// <summary>
        /// Find the first monitor (first one we come across in the for() loop)
        /// that is in the direction of the point.
        /// </summary>
        /// <param name="dir">Direction</param>
        /// <param name="curScreen">Current Screen</param>
        /// <returns></returns>
        public Display ScreenInDirection(Point dir, Rectangle curScreen)
        {
            // Screen must be strictly above/below/beside. For instance, for a monitor to be
            // "above", the monitor's Bottom equal to the current screen's Top ("current
            // screen" is where the Cursor (NOT the mouse!!) is currently).
            foreach (var s in All)
            {
                if (((dir.X == 1) && (curScreen.Right == s.Bounds.Left)) ||
                    ((dir.X == -1) && (curScreen.Left == s.Bounds.Right)) ||
                    ((dir.Y == 1) && (curScreen.Bottom == s.Bounds.Top)) ||
                    ((dir.Y == -1) && (curScreen.Top == s.Bounds.Bottom)))
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
        public Display WrapScreen(Point dir, Point cursor)
        {
            if (dir.X == 0)
                return All[0];

            // Find closest Left- or Right-most screen, in Y direction.
            var distClosest = int.MaxValue;
            var screensToCheck = (dir.X == 1 ? LeftMost : RightMost);
            var ws = screensToCheck.FirstOrDefault() ?? All[0]; 
            foreach (var s in screensToCheck)
            {
                var dist = Math.Abs(GeometryUtil.OutsideYDistance(s.Bounds, cursor));
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
            
            var i = 0;
            foreach (var s in All)
            {
                sb.AppendLine($"    {i}: {s.DetailledDescription()}");
                i += 1;
            }

            sb.AppendLine(
                $"Screens at the Edge: R({AsString(RightMost)}), L({AsString(LeftMost)}), T({AsString(TopMost)}), B({AsString(BottomMost)})   " +
                $"BoundingBox{BoundingBox}");

            return sb.ToString();

            static string AsString(List<Display> L) => string.Join(",", L.Select(sn => sn.ScreenNumber));
        }

    }
}
