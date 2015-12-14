using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;

namespace NutritionalInfoApp.Utils
{
    public static class InkHelper
    {
        #region Pen stroke utilities
        public static IList<StrokeCollection> PerformInkSegmentation(StrokeCollection strokes)
        {
            // Create a list of stroke collections
            // Note: The first StrokeCollection will consist of the first stroke
            var strokeCollectionList = new List<StrokeCollection> { new StrokeCollection { strokes[0].Clone() } };

            // Iterate through all of the strokes
            // Note: For each stroke, either add it to an existing StrokeCollection or create a new StrokeCollection
            for (var i = 1; i < strokes.Count(); ++i)
            {
                var intersectingGroups = new List<StrokeCollection>();

                foreach (var strokeCollection in strokeCollectionList)
                {
                    if ((from stroke in strokeCollection let stylusShape = new EllipseStylusShape(5.0, 5.0, 0.0) where stroke.HitTest((Point[])strokes[i].StylusPoints, stylusShape) select stroke).Any())
                    {
                        intersectingGroups.Add(strokeCollection);
                    }
                }

                // Create a new stroke collection if the stroke doesn't intersect with
                // any of the strokes in the other stroke collections created so far.
                if (intersectingGroups.Count == 0)
                    strokeCollectionList.Add(new StrokeCollection { strokes[i].Clone() });
                else
                {
                    var combinedGroup = new StrokeCollection { strokes[i].Clone() };

                    // Remove the intersecting groups from the stroke collection list
                    foreach (var intersectingGroup in intersectingGroups)
                    {
                        strokeCollectionList.Remove(intersectingGroup);
                        combinedGroup = new StrokeCollection(combinedGroup.Union(intersectingGroup));
                    }

                    strokeCollectionList.Add(combinedGroup);
                }
            }

            // Take stroke collections with one stroke out of the main list of stroke collections
            var singleStrokeCollectionList = strokeCollectionList.Where(strokeCollection => strokeCollection.Count() == 1).ToList();
            strokeCollectionList.RemoveAll(strokeCollection => strokeCollection.Count() == 1);

            // Add each single strokes to a group that is closest to it by a certain tolerance value
            // TODO: Maybe incorporate time into determination of which group a small stroke should be apart of?
            foreach (var singleStrokeCollection in singleStrokeCollectionList)
            {
                var leastDistance = double.PositiveInfinity;
                StrokeCollection closestGroup = null;

                foreach (var strokeCollection in strokeCollectionList)
                {
                    var distance = ComputeDistance(strokeCollection.GetBounds().Center(), singleStrokeCollection.GetBounds().Center());

                    if (!(distance < leastDistance)) continue;

                    leastDistance = distance;
                    closestGroup = strokeCollection;
                }

                // Add the small stroke to the closest group
                if (closestGroup != null && Math.Abs(leastDistance) < 5.0)
                {
                    closestGroup.Add(singleStrokeCollection);
                }
                else
                {
                    strokeCollectionList.Add(singleStrokeCollection);
                }
            }

            return strokeCollectionList;
        }

        /// <summary>
        /// Resamples a set of stylus points in a stroke into n stylus points.
        /// </summary>
        /// <param name="stroke"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Stroke ResampleStroke(Stroke stroke, int n)
        {
            if (n <= 0)
                return stroke;

            var stylusPoints = stroke.StylusPoints.Clone();
            var newPoints = new StylusPointCollection();

            var avgPathLength = ComputePathLength(stylusPoints) / (n - 1);

            const double tolerance = 0.00001;
            if (Math.Abs(avgPathLength) < tolerance)
                return stroke;

            var distanceHolder = 0.0;

            newPoints.Add(stylusPoints[0]);
            for (var i = 1; i < stylusPoints.Count; ++i)
            {
                var distance = ComputeDistance(stylusPoints[i - 1], stylusPoints[i]);
                if ((distanceHolder + distance) >= avgPathLength)
                {
                    var newX = stylusPoints[i - 1].X + ((avgPathLength - distanceHolder) / distance) * (stylusPoints[i].X - stylusPoints[i - 1].X);
                    var newY = stylusPoints[i - 1].Y + ((avgPathLength - distanceHolder) / distance) * (stylusPoints[i].Y - stylusPoints[i - 1].Y);
                    var newPoint = new StylusPoint(newX, newY);
                    newPoints.Add(newPoint);
                    stylusPoints.Insert(i, newPoint);
                    distanceHolder = 0.0;
                }
                else
                {
                    distanceHolder += distance;
                }
            }

            // Add last point if we fail to add it due to a rounding-error
            if (newPoints.Count == n - 1)
            {
                newPoints.Add(stylusPoints[stylusPoints.Count - 1]);
            }

            return new Stroke(newPoints);
        }
        #endregion

        #region 2D Math
        public static double ComputeDistance(StylusPoint a, StylusPoint b)
        {
            return ComputeDistance((Point)a, (Point)b);
        }

        private static double ComputeDistance(Point a, Point b)
        {
            var deltaX = b.X - a.X;
            var deltaY = b.Y - a.Y;

            return Math.Sqrt(Math.Pow(deltaX, 2.0) + Math.Pow(deltaY, 2.0));
        }

        public static double ComputePathLength(StylusPointCollection stylusPoints)
        {
            var distance = 0.0;

            for (var i = 1; i < stylusPoints.Count; ++i)
            {
                distance += ComputeDistance(stylusPoints[i - 1], stylusPoints[i]);
            }

            return distance;
        }

        /// <summary>
        /// Computes of the angle of the line between the two endpoints of a stroke.
        /// </summary>
        /// <param name="stroke"></param>
        /// <returns>Angle of the line between the stroke endpoints in radians.</returns>
        public static double ComputeStrokeAngle(Stroke stroke)
        {
            var p1 = stroke.StylusPoints[0];
            var p2 = stroke.StylusPoints[stroke.StylusPoints.Count - 1];

            var xDiff = p2.X - p1.X; 
            var yDiff = p2.Y - p1.Y; 
            
            return Math.Atan2(yDiff, xDiff);
        }

        /// <summary>
        /// Computes the angle between two strokes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Angle between the strokes in radians.</returns>
        public static double ComputeAngleBetweenStrokes(Stroke a, Stroke b)
        {
            var a1 = a.StylusPoints[0];
            var a2 = a.StylusPoints[a.StylusPoints.Count - 1];
            var b1 = b.StylusPoints[0];
            var b2 = b.StylusPoints[b.StylusPoints.Count - 1];

            var vecA = new Vector(a2.X - a1.X, a2.Y - a1.Y);
            var vecB = new Vector(b2.X - b1.X, b2.Y - b1.Y);

            return Math.Acos((vecA*vecB)/(vecA.Length*vecB.Length));
        }
        #endregion
    }
}
