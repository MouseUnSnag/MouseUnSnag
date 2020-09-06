using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Drawing;

namespace MouseUnSnag.Tests
{
    [TestClass()]
    public class GeometryUtilTests
    {
        [TestMethod()]
        public void SignTest()
        {
            var lst = new List<(Point toTest, Point expected)>()
            {
                (new Point(0, 0),   new Point(0,0)),
                (new Point(123, 0), new Point(1,0)),
                (new Point(-33, 0), new Point(-1,0)),
                (new Point(0, 123), new Point(0,1)),
                (new Point(0, -5),  new Point(0,-1)),
                (new Point(123, 123), new Point(1,1)),
            };

            foreach (var item in lst)
            {
                var actual = GeometryUtil.Sign(item.toTest);
                Assert.AreEqual(item.expected, actual);
            }
        }

        [TestMethod()]
        public void DirectionTest()
        {
            var lst = new List<(Point p1, Point p2, Point expected)>()
            {
                (new Point(5, 55),    new Point(5,55),  new Point(0, 0)),
                (new Point(123, 55),  new Point(20,30), new Point(-1, -1)),
                (new Point(123, 55),  new Point(125,3), new Point(1, -1)),
                (new Point(123, 55),  new Point(3,99),  new Point(-1, 1)),
                (new Point(123, 55),  new Point(999,99),new Point(1, 1)),
            };

            foreach (var item in lst)
            {
                var actual = GeometryUtil.Direction(item.p1, item.p2);
                Assert.AreEqual(item.expected, actual);
            }
        }

        [TestMethod()]
        public void OutsideXDistanceTest()
        {
            var rect = new Rectangle(100, 200, 300, 400);

            for (var x = rect.Left - 200; x < rect.Right + 200; x++)
            {
                for (var y = -1000; y < 1000; y += 100)
                {
                    var p = new Point(x, y);
                    var actual = GeometryUtil.OutsideXDistance(rect, p);
                    var expected = int.MinValue;
                    if (x < rect.Left)
                        expected = (x - rect.Left);
                    else if (x < rect.Right)
                        expected = 0;
                    else
                        expected = x - (rect.Right - 1);
                    
                    Assert.AreEqual(expected, actual, $"{x}, {y}");
                }
            }
        }

        [TestMethod()]
        public void OutsideYDistanceTest()
        {
            var rect = new Rectangle(100, 200, 300, 400);

            for (var x = -1000; x < 1000; x += 100)
            {
                for (var y = rect.Bottom - 200; y < rect.Top + 200; y++)
                {
                    var p = new Point(x, y);
                    var actual = GeometryUtil.OutsideYDistance(rect, p);
                    var expected = int.MinValue;
                    if (x < rect.Bottom)
                        expected = (x - rect.Bottom);
                    else if (x < rect.Top)
                        expected = 0;
                    else
                        expected = x - (rect.Top - 1);
                    
                    Assert.AreEqual(expected, actual, $"{x}, {y}");
                }
            }
        }

        [TestMethod()]
        public void OutsideDistanceTest()
        {
            var rect = new Rectangle(100, 200, 300, 400);

            for (var x = rect.Left - 200; x < rect.Right + 200; x++)
            {
                for (var y = rect.Bottom - 200; y < rect.Top + 200; y++)
                {
                    var p = new Point(x, y);
                    var expected = new Point(GeometryUtil.OutsideXDistance(rect, p), GeometryUtil.OutsideYDistance(rect, p));
                    var actual = GeometryUtil.OutsideDistance(rect, p);
                    
                    Assert.AreEqual(expected, actual, $"{x}, {y}");
                }
            }
        }

        [TestMethod()]
        public void OutsideDirectionTest()
        {
            var rect = new Rectangle(100, 200, 300, 400);

            for (var x = rect.Left - 200; x < rect.Right + 200; x++)
            {
                for (var y = rect.Bottom - 200; y < rect.Top + 200; y++)
                {
                    var p = new Point(x, y);
                    var expected = GeometryUtil.Sign(GeometryUtil.OutsideDistance(rect, p));
                    var actual = GeometryUtil.OutsideDirection(rect, p);
                    
                    Assert.AreEqual(expected, actual, $"{x}, {y}");
                }
            }
        }

        [TestMethod()]
        public void ClosestBoundaryPointTest()
        {
            int Constrain(int location, int lowerBound, int upperBound)
            {
                if (location < lowerBound)
                    return lowerBound;
                else if (location < upperBound)
                    return location;
                else
                    return upperBound - 1;
            }

            var rect = new Rectangle(100, 200, 300, 400);

            for (var x = rect.Left - 200; x < rect.Right + 200; x++)
            {
                var xExpected = Constrain(x, rect.Left, rect.Right);

                for (var y = rect.Bottom - 200; y < rect.Top + 200; y++)
                {
                    var p = new Point(x, y);
                    var actual = GeometryUtil.ClosestBoundaryPoint(rect, p);
                    var yExpected = Constrain(y, rect.Bottom, rect.Top);

                    Assert.AreEqual(new Point(xExpected, yExpected), actual, $"{x}, {y}");
                }
            }
            
        }



        [TestMethod()]
        public void OverlapXTest()
        {
            var r1 = new Rectangle(100, 200, 300, 400);

            for (var x = r1.Left - 200; x < r1.Right + 200; x++)
            {
                for (var y = r1.Bottom - 200; y < r1.Top + 200; y++)
                {
                    var r2 = new Rectangle(x, y, 10, 10);

                    var actual = GeometryUtil.OverlapX(r1, r2);
                    var expected = ((r2.Left > r1.Left) && (r2.Left < r1.Right));
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod()]
        public void OverlapYTest()
        {
            var r1 = new Rectangle(100, 200, 300, 400);

            for (var x = r1.Left - 200; x < r1.Right + 200; x++)
            {
                for (var y = r1.Bottom - 200; y < r1.Top + 200; y++)
                {
                    var r2 = new Rectangle(x, y, 10, 10);

                    var actual = GeometryUtil.OverlapX(r1, r2);
                    var expected = ((r2.Bottom > r1.Bottom) && (r2.Bottom < r1.Top));
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod()]
        public void RescaleXTest()
        {
            var rSource = new Rectangle(100, 200, 300, 400);
            var rTarget = new Rectangle(1000, 2000, 3000, 4000);

            var p = new Point(200, 150);

            var expected = new Point(2000, 150);
            var actual = p.RescaleX(rSource, rTarget);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RescaleYTest()
        {
            var rSource = new Rectangle(100, 200, 300, 400);
            var rTarget = new Rectangle(1000, 2000, 3000, 4000);

            var p = new Point(200, 300);

            var expected = new Point(200, 3000);
            var actual = p.RescaleY(rSource, rTarget);
            Assert.AreEqual(expected, actual);
        }
    }
}