using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GlobalcachingApplication.Utils
{
    public class DouglasPeucker
    {
        /// <summary>
        /// Uses the Douglas Peucker algorithm to reduce the number of points.
        /// </summary>
        /// <param name="Points">The points.</param>
        /// <param name="Tolerance">The tolerance.</param>
        /// <returns></returns>
        public static List<Framework.Data.Location> DouglasPeuckerReduction
            (List<Framework.Data.Location> Points, Double Tolerance)
        {
            if (Points == null || Points.Count < 3)
                return Points;

            Int32 firstPoint = 0;
            Int32 lastPoint = Points.Count - 1;
            List<Int32> pointIndexsToKeep = new List<Int32>();

            //Add the first and last index to the keepers
            pointIndexsToKeep.Add(firstPoint);
            pointIndexsToKeep.Add(lastPoint);

            //The first and the last point cannot be the same
            while (Points[firstPoint].Equals(Points[lastPoint]))
            {
                lastPoint--;
                pointIndexsToKeep.Add(lastPoint);
            }

            DouglasPeuckerReduction(Points, firstPoint, lastPoint,
            Tolerance, ref pointIndexsToKeep);

            List<Framework.Data.Location> returnPoints = new List<Framework.Data.Location>();
            pointIndexsToKeep.Sort();
            foreach (Int32 index in pointIndexsToKeep)
            {
                returnPoints.Add(Points[index]);
            }

            return returnPoints;
        }

        /// <summary>
        /// Douglases the peucker reduction.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="firstPoint">The first point.</param>
        /// <param name="lastPoint">The last point.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="pointIndexsToKeep">The point index to keep.</param>
        private static void DouglasPeuckerReduction(List<Framework.Data.Location>
            points, Int32 firstPoint, Int32 lastPoint, Double tolerance,
            ref List<Int32> pointIndexsToKeep)
        {
            Double maxDistance = 0;
            Int32 indexFarthest = 0;

            /* org
            for (Int32 index = firstPoint; index < lastPoint; index++)
            {
                Double distance = PerpendicularDistance
                    (points[firstPoint], points[lastPoint], points[index]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0)
            {
                //Add the largest point that exceeds the tolerance
                pointIndexsToKeep.Add(indexFarthest);

                DouglasPeuckerReduction(points, firstPoint,
                indexFarthest, tolerance, ref pointIndexsToKeep);
                DouglasPeuckerReduction(points, indexFarthest,
                lastPoint, tolerance, ref pointIndexsToKeep);
            }
             * */

            if (lastPoint - firstPoint > 1)//ADDITION: need to have more than two points in the set we are looking through
            {
                for (Int32 index = firstPoint; index < lastPoint; index++)
                {
                    Double distance = PerpendicularDistance(points[firstPoint], points[lastPoint], points[index]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        indexFarthest = index;
                    }
                }

                if (maxDistance > tolerance && indexFarthest != firstPoint)//CHANGE: condition was wrong.
                {
                    //Add the largest point that exceeds the tolerance
                    pointIndexsToKeep.Add(indexFarthest);

                    DouglasPeuckerReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexsToKeep);
                    DouglasPeuckerReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexsToKeep);
                }
            }
        }

        /// <summary>
        /// The distance of a point from a line made from point1 and point2.
        /// </summary>
        /// <param name="pt1">The PT1.</param>
        /// <param name="pt2">The PT2.</param>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        public static Double PerpendicularDistance
            (Framework.Data.Location Point1, Framework.Data.Location Point2, Framework.Data.Location Point)
        {
            //Area = |(1/2)(x1y2 + x2y3 + x3y1 - x2y1 - x3y2 - x1y3)|   *Area of triangle
            //Base = v((x1-x2)²+(x1-x2)²)                               *Base of Triangle*
            //Area = .5*Base*H                                          *Solve for height
            //Height = Area/.5/Base

            Double area = Math.Abs(.5 * (Point1.Lon * Point2.Lat + Point2.Lon *
            Point.Lat + Point.Lon * Point1.Lat - Point2.Lon * Point1.Lat - Point.Lon *
            Point2.Lat - Point1.Lon * Point.Lat));
            Double bottom = Math.Sqrt(Math.Pow(Point1.Lon - Point2.Lon, 2) +
            Math.Pow(Point1.Lat - Point2.Lat, 2));
            Double height = area / bottom * 2;

            return height;

            //Another option
            //Double A = Point.X - Point1.X;
            //Double B = Point.Y - Point1.Y;
            //Double C = Point2.X - Point1.X;
            //Double D = Point2.Y - Point1.Y;

            //Double dot = A * C + B * D;
            //Double len_sq = C * C + D * D;
            //Double param = dot / len_sq;

            //Double xx, yy;

            //if (param < 0)
            //{
            //    xx = Point1.X;
            //    yy = Point1.Y;
            //}
            //else if (param > 1)
            //{
            //    xx = Point2.X;
            //    yy = Point2.Y;
            //}
            //else
            //{
            //    xx = Point1.X + param * C;
            //    yy = Point1.Y + param * D;
            //}

            //Double d = DistanceBetweenOn2DPlane(Point, new Point(xx, yy));
        }
    }
}
