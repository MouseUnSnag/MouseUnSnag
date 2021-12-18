using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MouseUnSnag.Configuration;
using MouseUnSnag.ScreenHandling;

namespace MouseUnSnag
{
    public class MouseLogic
    {

        private int _evaluations;
        public int Jumps { get; private set; }

        private Point _lastMouse;
        private Rectangle _lastCursorScreenBounds;

        public Rectangle LastCursorScreenBounds
        {
            get => _lastCursorScreenBounds;
            set
            {
                if (value == _lastCursorScreenBounds)
                    return;
                _lastCursorScreenBounds = value;
                OnLastCursorBoundsChanged(new BoundsChangedEventArgs(_lastCursorScreenBounds));
            }
        }

        /// <summary>
        /// Indicates that screens are updating.
        /// </summary>
        public bool ScreensAreUpdating { get; private set; }

        /// <summary>
        /// Display Configuration 
        /// </summary>
        public DisplayList DisplayList { get; set; }

        /// <summary>
        /// Options
        /// </summary>
        public Options Options { get; }

        /// <summary>
        /// Initializes a new <see cref="MouseLogic"/> instance
        /// </summary>
        /// <param name="options"></param>
        public MouseLogic(Options options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            BeginScreenUpdate();
        }

        /// <summary>
        /// Indicates that the screen configuration is currently beeing updated (suppresses mouse processing temporarily)
        /// </summary>
        public void BeginScreenUpdate()
        {
            ScreensAreUpdating = true;
        }

        /// <summary>
        /// Indicates that screen updating is complete and takes over the new Screen Configuration
        /// </summary>
        /// <param name="displayList"></param>
        public void EndScreenUpdate(DisplayList displayList)
        {
            DisplayList = displayList ?? throw new ArgumentNullException(nameof(displayList));
            LastCursorScreenBounds = Rectangle.Empty;
            ScreensAreUpdating = false;
        }


        /// <summary>
        /// Mouse Processing. 
        /// </summary>
        /// <param name="mouse">Position of the mouse. Can be outside the screen. Is one step ahead of cursor</param>
        /// <param name="cursor">Position of the cursor. Always within screen bounds</param>
        /// <param name="newCursor">New position that shall be taken over</param>
        /// <returns>True if the newCursor Position shall be taken over</returns>
        public bool HandleMouse(Point mouse, Point cursor, out Point newCursor)
        {
            _evaluations += 1;
            newCursor = cursor;

            var displays = DisplayList;
            if (ScreensAreUpdating || displays == null)
            {
                Debug.WriteLine(displays == null
                    ? $"{_evaluations}: No Display Configuration given"
                    : $"{_evaluations}: Screens are updating.");
                return false;
            }

            var cursorScreen = displays.WhichScreen(cursor);
            var mouseScreen = displays.WhichScreen(mouse);
            var isStuck = (cursor != _lastMouse) && (mouseScreen != cursorScreen);
            var stuckDirection = GeometryUtil.OutsideDirection(cursorScreen.Bounds, mouse);


            Debug.WriteLine($"{_evaluations}: StuckDirection/Distance{stuckDirection}/{GeometryUtil.OutsideDistance(cursorScreen.Bounds, mouse)} cur_mouse:{mouse}  prev_mouse:{_lastMouse} ==? cursor:{cursor} (OnMon#{cursorScreen}/{mouseScreen})  #UnSnags {Jumps}   {(isStuck ? "--STUCK--" : "         ")}   ");

            LastCursorScreenBounds = cursorScreen.Bounds;
            _lastMouse = mouse;

            if (!isStuck)
            {
                return false;
            }

            var jumpScreen = displays.ScreenInDirection(stuckDirection, cursorScreen.Bounds);


            if (mouseScreen != null)
            {
                // Mouse is in valid screen --> Go to it.
                // This avoids the Windows 10 feature where two adjacent screens have a small impassable border of a few Pixels at the top
                Debug.WriteLine("  > MouseScreen");
                if (!Options.Unstick)
                    return false;

                newCursor = Options.Rescale ? RescaleY(mouse, cursorScreen.Bounds, mouseScreen.Bounds) : mouse;
            }
            else if (jumpScreen != null)
            {
                Debug.WriteLine("  > JumpScreen");
                if (!Options.Jump)
                    return false;

                newCursor = jumpScreen.Bounds.ClosestBoundaryPoint(Options.Rescale ? RescaleY(cursor, cursorScreen.Bounds, jumpScreen.Bounds) : cursor);
            }
            else if (stuckDirection.X != 0)
            {
                Debug.WriteLine("  > Wrap");
                if (!Options.Wrap)
                    return false;

                var wrapScreen = displays.WrapScreen(stuckDirection, cursor);
                var wrapPoint = new Point(stuckDirection.X == 1 ? wrapScreen.Bounds.Left : wrapScreen.Bounds.Right - 1, cursor.Y);

                // Don't wrap cursor if jumping is disabled and it would need to jump.
                if (!Options.Jump && !wrapScreen.Bounds.Contains(wrapPoint))
                {
                    Debug.WriteLine("    > No Wrap due to disabled Jump");
                    return false;
                }

                if (Options.Rescale)
                {
                    // Currently does not work properly. Wrapping from small screen bottom half to big screen appears to set the newCursor Position correctly, but the actual cursor does not move there.
                    //wrapPoint = RescaleY(wrapPoint, cursorScreen.Bounds, wrapScreen.Bounds);
                }

                newCursor = wrapScreen.Bounds.ClosestBoundaryPoint(wrapPoint);
            }
            else
            {
                Debug.WriteLine("  > None");
                return false;
            }

            Debug.WriteLine($"{cursor} -> {newCursor}");
            Jumps += 1;
            return true;
        }

        

        private static Point RescaleY(Point p, Rectangle source, Rectangle destination)
        {
            if (Math.Abs(source.Top - destination.Top) > 20)
                return p;

            return p.RescaleY(source, destination);
        }

        public event EventHandler<BoundsChangedEventArgs> LastCursorBoundsChanged;
        protected virtual void OnLastCursorBoundsChanged(BoundsChangedEventArgs e)
        {
            EventHandler<BoundsChangedEventArgs> handler = LastCursorBoundsChanged;
            handler?.Invoke(this, e);
        }
        public class BoundsChangedEventArgs : EventArgs
        {
            public Rectangle Bounds { get; }

            public BoundsChangedEventArgs(Rectangle bounds)
            {
                Bounds = bounds;
            }
        }

    }
}
