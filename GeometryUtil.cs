/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Drawing;

namespace MouseUnSnag
{
    internal static class GeometryUtil
    {
        // Return the signs of X and Y. This essentially gives us the "component direction" of
        // the point (N.B. the vector length is not "normalized" to a length 1 "unit vector" if
        // both the X and Y components are non-zero).
        public static Point Sign(Point p) => new Point(Math.Sign(p.X), Math.Sign(p.Y));

        // "Direction" vector from P1 to P2. X/Y of returned point will have values
        // of -1, 0, or 1 only (vector is not normalized to length 1).
        public static Point Direction(Point P1, Point P2) => Sign(P2 - (Size)P1);

        // If P is anywhere inside R, then OutsideDistance() returns (0,0).
        // Otherwise, it returns the (x,y) delta (sign is preserved) from P to the
        // nearest edge/corner of R. For Right and Bottom we must correct by 1,
        // since the Rectangle Right and Bottom are one larger than the largest
        // valid pixel.

        public static int OutsideXDistance(Rectangle R, Point P)
            => Math.Max(Math.Min(0, P.X - R.Left), P.X - R.Right + 1);

        public static int OutsideYDistance(Rectangle R, Point P)
            => Math.Max(Math.Min(0, P.Y - R.Top), P.Y - R.Bottom + 1);

        public static Point OutsideDistance(Rectangle R, Point P)
            => new Point(OutsideXDistance(R, P), OutsideYDistance(R, P));

        // This is sort-of the "opposite" of above. In a sense it "captures" the point to the
        // boundary/inside of the rectangle, rather than "excluding" it to the exterior of the rectangle.
        //
        // If the point is outside the rectangle, then it returns the closest location on the
        // rectangle boundary to the Point. If Point is inside Rectangle, then it just returns
        // the point.

        public static Point ClosestBoundaryPoint(this Rectangle R, Point P)
            => new Point(
                Math.Max(Math.Min(P.X, R.Right - 1), R.Left),
                Math.Max(Math.Min(P.Y, R.Bottom - 1), R.Top));

        // In which direction(s) is(are) the point outside of the rectangle? If P is
        // inside R, then this returns (0,0). Else X and/or Y can be either -1 or
        // +1, depending on which direction P is outside R.
        public static Point OutsideDirection(Rectangle R, Point P) => Sign(OutsideDistance(R, P));

        public static bool OverlapX(Rectangle R1, Rectangle R2) => (R1.Left < R2.Right) && (R1.Right > R2.Left);
        public static bool OverlapY(Rectangle R1, Rectangle R2) => (R1.Top < R2.Bottom) && (R1.Bottom > R2.Top);
    }
}