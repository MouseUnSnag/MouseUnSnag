/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Dale Roberts. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Drawing;

namespace MouseUnSnag
{
    public static class GeometryUtil
    {

        /// <summary>
        /// Return the signs of X and Y. This essentially gives us the "component direction" of
        /// the point (N.B. the vector length is not "normalized" to a length 1 "unit vector" if
        /// both the X and Y components are non-zero).
        /// </summary>
        /// <param name="p"><see cref="Point"/> of which to get the sign</param>
        /// <returns><see cref="Point"/> where X, Y have values corresponding to their sign</returns>
        public static Point Sign(Point p) => new Point(Math.Sign(p.X), Math.Sign(p.Y));

        /// <summary>
        /// Return the magnitude of Point p.
        /// </summary>
        /// <param name="p"><see cref="Point"/> of which to get the sign</param>
        /// <returns><see cref="Point"/> where X, Y have values corresponding to their sign</returns>
        public static double Magnitude(Point p) => Math.Sqrt(p.X * p.X + p.Y * p.Y);

        /// <summary>
        /// "Direction" vector from P1 to P2. X/Y of returned point will have values
        /// of -1, 0, or 1 only (vector is not normalized to length 1).
        /// </summary>
        /// <param name="p1"><see cref="Point"/></param>
        /// <param name="p2"><see cref="Point"/></param>
        /// <returns><see cref="Point"/> with X, Y having values corresponding to their sign</returns>
        public static Point Direction(Point p1, Point p2) => Sign(p2 - (Size)p1);


        /// <summary>
        /// If P is anywhere inside R, then OutsideDistance() returns (0,0).
        /// Otherwise, it returns the (x,y) delta (sign is preserved) from P to the
        /// nearest edge/corner of R. 
        /// </summary>
        /// <param name="r"><see cref="Rectangle"/></param>
        /// <param name="p"><see cref="Point"/></param>
        /// <returns>Distance</returns>
        public static int OutsideXDistance(Rectangle r, Point p) => Math.Max(Math.Min(0, p.X - r.Left), p.X - r.Right + 1); // For Right we must correct by 1, since the Rectangle Right is one larger than the largest valid pixel

        /// <summary>
        /// If P is anywhere inside R, then OutsideDistance() returns (0,0).
        /// Otherwise, it returns the (x,y) delta (sign is preserved) from P to the
        /// nearest edge/corner of R. 
        /// </summary>
        /// <param name="r"><see cref="Rectangle"/></param>
        /// <param name="p"><see cref="Point"/></param>
        /// <returns>Distance</returns>
        public static int OutsideYDistance(Rectangle r, Point p) => Math.Max(Math.Min(0, p.Y - r.Top), p.Y - r.Bottom + 1); // For Bottom we must correct by 1, since the Rectangle Bottom is one larger than the largest valid pixel

        /// <summary>
        /// If P is anywhere inside R, then OutsideDistance() returns (0,0).
        /// Otherwise, it returns the (x,y) delta (sign is preserved) from P to the
        /// nearest edge/corner of R. 
        /// </summary>
        /// <param name="r"><see cref="Rectangle"/></param>
        /// <param name="p"><see cref="Point"/></param>
        /// <returns><see cref="Point"/> where X and Y represents the distance from the rectangle</returns>
        public static Point OutsideDistance(Rectangle r, Point p) => new Point(OutsideXDistance(r, p), OutsideYDistance(r, p));

        
        /// <summary>
        /// This is sort-of the "opposite" of OutsideDistance. In a sense it "captures" the point to the
        /// boundary/inside of the rectangle, rather than "excluding" it to the exterior of the rectangle.
        /// If the point is outside the rectangle, then it returns the closest location on the
        /// rectangle boundary to the Point. If Point is inside Rectangle, then it just returns
        /// the point.
        /// </summary>
        /// <param name="r"><see cref="Rectangle"/></param>
        /// <param name="p"><see cref="Point"/></param>
        /// <returns><see cref="Point"/></returns>
        public static Point ClosestBoundaryPoint(this Rectangle r, Point p)
            => new Point(
                Math.Max(Math.Min(p.X, r.Right - 1), r.Left),
                Math.Max(Math.Min(p.Y, r.Bottom - 1), r.Top));

        /// <summary>
        /// In which direction(s) is(are) the point outside of the rectangle? If P is
        /// inside R, then this returns (0,0). Else X and/or Y can be either -1 or
        /// +1, depending on which direction P is outside R.
        /// </summary>
        /// <param name="r"><see cref="Rectangle"/></param>
        /// <param name="p"><see cref="Point"/></param>
        /// <returns><see cref="Point"/></returns>
        public static Point OutsideDirection(Rectangle r, Point p) => Sign(OutsideDistance(r, p));

        /// <summary>
        /// In which direction(s) is(are) the rectangle r2 outside of the rectangle r1? If r2
        /// overlaps r1, then this returns (0,0). Else X and/or Y can be either -1 or +1, depending
        /// on which direction r2 is outside r1. For a rectangle to be diagonal to another rectangle,
        /// it must not overlap horizontal or vertically.
        /// </summary>
        /// <param name="r"><see cref="Rectangle"/></param>
        /// <param name="p"><see cref="Point"/></param>
        /// <returns><see cref="Point"/></returns>
        public static Point OutsideDirection(Rectangle r1, Rectangle r2) => (OverlapX(r1, r2), OverlapY(r1, r2)) switch {
            (false, false) => new Point(Math.Sign(r1.Left - r2.Left), Math.Sign(r1.Top - r2.Top)),
            (false, true) => new Point(Math.Sign(r1.Left - r2.Left), 0),
            (true, false) => new Point(0, Math.Sign(r1.Top - r2.Top)),
            (true, true) => new Point(0, 0),
        };


        /// <summary>
        /// Check if both Rectangles overlap in the X Direction
        /// </summary>
        /// <param name="r1"><see cref="Rectangle"/></param>
        /// <param name="r2"><see cref="Rectangle"/></param>
        /// <returns></returns>
        public static bool OverlapX(Rectangle r1, Rectangle r2) => (r1.Left < r2.Right) && (r1.Right > r2.Left);

        /// <summary>
        /// Check if both Rectangles overlap in the Y Direction
        /// </summary>
        /// <param name="r1"><see cref="Rectangle"/></param>
        /// <param name="r2"><see cref="Rectangle"/></param>
        /// <returns></returns>
        public static bool OverlapY(Rectangle r1, Rectangle r2) => (r1.Top < r2.Bottom) && (r1.Bottom > r2.Top);


        /// <summary>
        /// Rescales the Y coordinate of a point from one rectangle to another. Reference: Top
        /// </summary>
        /// <param name="p"><see cref="Point"/> to rescale</param>
        /// <param name="source">Source <see cref="Rectangle"/></param>
        /// <param name="destination">Destination <see cref="Rectangle"/></param>
        /// <returns></returns>
        public static Point RescaleY(this Point p, Rectangle source, Rectangle destination) => new Point(p.X, ((p.Y - source.Top) * destination.Height / source.Height) + destination.Top);
        
        /// <summary>
        /// Rescales the X coordinate of a point from one rectangle to another. Reference: Left
        /// </summary>
        /// <param name="p"><see cref="Point"/> to rescale</param>
        /// <param name="source">Source <see cref="Rectangle"/></param>
        /// <param name="destination">Destination <see cref="Rectangle"/></param>
        /// <returns></returns>
        public static Point RescaleX(this Point p, Rectangle source, Rectangle destination) => new Point(((p.X - source.Left) * destination.Width / source.Width) + destination.Left, p.Y);

    }
}
