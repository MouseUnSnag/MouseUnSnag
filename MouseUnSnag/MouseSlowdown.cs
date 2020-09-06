using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseUnSnag
{
    public class MouseSlowdown
    {
        private int _xRemainder;
        private int _yRemainder;

        private float _xRemainder2;
        private float _yRemainder2;

        private readonly int _slowFactorMultiplier = 1000;


        public float SpeedFactor { get; set; } = 2.25f;
        public int SlowFactorPerMille { get; set; } = 4000;
        

        /// <summary>
        /// Slow the cursor by <see cref="SlowFactorPerMille"/>
        /// </summary>
        /// <param name="mouse"></param>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public Point SlowCursor(Point mouse, Point cursor)
        {
            var slowFactor = SlowFactorPerMille;

            var xMovement = Math.DivRem((mouse.X - cursor.X) * _slowFactorMultiplier + _xRemainder, slowFactor, out _xRemainder);
            var yMovement = Math.DivRem((mouse.Y - cursor.Y) * _slowFactorMultiplier + _yRemainder, slowFactor, out _yRemainder);

            return new Point(cursor.X + xMovement, cursor.Y + yMovement);
        }

        /// <summary>
        /// Slow the cursor using <see cref="SpeedFactor"/>
        /// </summary>
        /// <param name="mouse"></param>
        /// <param name="cursor"></param>
        /// <returns></returns>
        public Point SlowCursor2(Point mouse, Point cursor)
        {
            var speedFactor = SpeedFactor;
            
            var xDelta = (mouse.X - cursor.X) * speedFactor + _xRemainder2;
            var yDelta = (mouse.Y - cursor.Y) * speedFactor + _yRemainder2;

            var xDeltaInt = (int) xDelta;
            var yDeltaInt = (int) yDelta;

            _xRemainder2 = xDelta - xDeltaInt;
            _yRemainder2 = yDelta - yDeltaInt;
            
            return new Point(cursor.X + xDeltaInt, cursor.Y + yDeltaInt);
        }

    }
}
