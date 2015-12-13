using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NutritionalInfoApp.Utils
{
    /// <summary>
    ///  Class that performs the recognition of gestures from an ink stroke.  
    ///  First the recognizer resamples the ink stroke, finds the corners of the stroke utilizing the ShortStraw algorithm (with some minor variations
    ///  taken from the iStraw algorithm), finds any self-intersections, and then utilizes the data from the ink preprocessing to do gesture recognition.
    ///  Note: This gesture recognizer handle uni-strokes ONLY.
    /// </summary>
    public class InkGestureRecognizer : IGestureRecognizer
    {
        // Constants
        private const double KRectangleAngleThresh = Math.PI / 16.0;
        private const double KRectangleAspectThreshold = 0.75;
        private const double KAxisDifferenceThreshold = 0.60; // Percentage difference between minor and major axis lengths in ellipse to determine if the shape is a circle or ellipse
        private const double KEndpointThreshold = 15; // How close together the first and last stroke point have to be to make a possible closed polygon
        private const double KSampleFactor = 40.0;

        /* These are the color brushes used for each of the shapes:
            - Square => Brushes.Green;
            - Rectangle => Brushes.Blue;
            - Triangle => Brushes.Firebrick;
            - Ellipse => Brushes.Gray;
            - Circle => Brushes.Red;
            - Arrow => Brushes.Black;
        */

        // Last stroke collected
        private Stroke m_Stroke;

        // Last gesture recognized when the recognizer was last run (i.e. InkGestureRecognizer.Recognize())
        private GestureType m_RecognizedGesture = GestureType.None;

        // Shape representing the last gesture recognized when the recognizer was last run (i.e. InkGestureRecognizer.Recognize())
        private Shape m_RecognizedGestureShape = new Rectangle();

        public void SetStroke(Stroke stroke)
        {
            m_Stroke = stroke.Clone();
        }

        public GestureType GetRecognizedGesture()
        {
            return m_RecognizedGesture;
        }

        public Shape GetRecognizedGestureShape()
        {
            return m_RecognizedGestureShape;
        }

        /// <summary>
        /// Attempts to recognize a gesture from the stroke set earlier in SetStroke().  If a stroke
        /// has not been set, then the value from GetRecognizedGesture() will be MiniJournal.Ink.GestureType.None (Default Value)
        /// and the return value from GetRecognisedGestureShape() will be System.Windows.Shapes.Rectangle (Default Value).
        /// </summary>
        public void Recognize()
        {
            if (m_Stroke != null)
            {
                // Resample corners
                var resampledPoints = ResamplePoints(m_Stroke);

                // Find corners
                var cornerIndices = GetCorners(resampledPoints);

                // Find intersections
                var intersectionPoints = GetIntersections(resampledPoints);

                // Get recognized Gesture
                var answer = FindGesture(resampledPoints, cornerIndices, intersectionPoints);

                m_RecognizedGesture = answer.Item1;
                m_RecognizedGestureShape = answer.Item2;

                m_Stroke = null;
            }
            else
            {
                m_RecognizedGesture = GestureType.None;
                m_RecognizedGestureShape = new Rectangle();
            }
        }

        private double ComputeDistance(StylusPoint a, StylusPoint b)
        {
            var deltaX = b.X - a.X;
            var deltaY = b.Y - a.Y;

            return Math.Sqrt(Math.Pow(deltaX, 2.0) + Math.Pow(deltaY, 2.0));
        }

        private double ComputePathDistance(StylusPointCollection points, int a, int b)
        {
            var distance = 0.0;

            for (int i = a; i < b; ++i)
            {
                distance += ComputeDistance(points[i], points[i + 1]);
            }

            return distance;
        }

        private double ComputeResampleSpacing(Stroke stroke)
        {
            // Get the bounds of the stroke
            var unSampledBounds = stroke.GetBounds();

            // interspacing distance = [Bounds Diagonal Length] / kSampleFactor
            return Math.Sqrt(Math.Pow(unSampledBounds.Width, 2.0) + Math.Pow(unSampledBounds.Height, 2.0)) / KSampleFactor;
        }

        /// <summary>
        /// Computes the straw threshold as MEAN(straws) * 0.95
        /// </summary>
        /// <param name="straws"></param>
        /// <returns></returns>
        private static double ComputeStrawThreshold(ICollection<double> straws)
        {
            var threshold = 0.0;

            if (straws.Count > 0)
            {
                threshold += straws.Sum() / straws.Count;
            }

            return threshold * 0.95;
        }

        // Outputs a resampled version of the ink stroke (Taken from ShortStraw paper)
        private StylusPointCollection ResamplePoints(Stroke stroke)
        {
            var oldStroke = stroke.Clone();

            // Transform stroke to origin
            var bounds = oldStroke.GetBounds();
            var translateMatrix = new Matrix();
            translateMatrix.Translate(-bounds.Left, -bounds.Top);
            oldStroke.Transform(translateMatrix, false);

            var resampledPoints = new StylusPointCollection();

            var interspacingDistance = ComputeResampleSpacing(oldStroke);
            var distanceHolder = 0.0;

            resampledPoints.Insert(0, oldStroke.StylusPoints[0]);
            for (int i = 1; i < oldStroke.StylusPoints.Count; ++i)
            {
                var a = oldStroke.StylusPoints[i - 1];
                var b = oldStroke.StylusPoints[i];

                var distance = ComputeDistance(a, b);

                if (distanceHolder + distance >= interspacingDistance)
                {
                    var approxDistance = (interspacingDistance - distanceHolder) / distance;
                    var x = a.X + approxDistance * (b.X - a.X);
                    var y = a.Y + approxDistance * (b.Y - a.Y);
                    var stylusPoint = new StylusPoint(x, y);

                    resampledPoints.Add(stylusPoint);
                    oldStroke.StylusPoints.Insert(i, stylusPoint);

                    distanceHolder = 0;
                }
                else
                {
                    distanceHolder += distance;
                }
            }

            return resampledPoints;
        }

        /// <summary>
        /// Method that takes a collection of corners from an ink stroke and returns 
        /// the indices to the "corners" in the ink stroke.
        /// </summary>
        /// <returns></returns>
        private IList<int> GetCorners(StylusPointCollection points)
        {
            // Initialize straw collection
            var straws = new List<double>();
            for (int i = 0; i < points.Count; ++i)
            {
                straws.Add(0.0);
            }

            // Initialize corner index collection
            var cornerIndices = new List<int> { 0 };

            if (points.Count > 1)
            {
                // Create window for performing ShortStraw algorithm
                var window = 3;

                // Set straw values for each point
                for (int i = window; i < points.Count - window; ++i)
                    straws[i] = ComputeDistance(points[i - window], points[i + window]);

                var strawThreshold = ComputeStrawThreshold(straws);

                for (int i = window; i < points.Count - window; ++i)
                {
                    if (straws[i] < strawThreshold)
                    {
                        var localMin = double.PositiveInfinity;
                        var localMinIndex = i;

                        while (i < straws.Count && straws[i] < strawThreshold)
                        {
                            if (straws[i] < localMin)
                            {
                                localMin = straws[i];
                                localMinIndex = i;
                            }

                            ++i;
                        }
                        cornerIndices.Add(localMinIndex);
                    }
                }

                // Add last point index to cornerIndices list
                cornerIndices.Add(points.Count - 1);

                cornerIndices = PostProcessCorners(points, cornerIndices, straws);

                cornerIndices = CurveDetection(points, cornerIndices);
            }

            return cornerIndices;
        }

        /// <summary>
        /// Remove corners that are really points on a curve.  This was taken from the iStraw paper.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="cornerIndices"></param>
        /// <returns></returns>
        private List<int> CurveDetection(StylusPointCollection points, List<int> cornerIndices)
        {
            const int kShift = 15;

            for (int i = 1; i < cornerIndices.Count - 1; ++i)
            {
                var pointIndex = cornerIndices[i]; // Current index for a corner in the points list

                // Calculate shift value to be used to calculate alpha and beta angles
                var indexDiffBetweenCorners = Math.Min(pointIndex - cornerIndices[i - 1], (points.Count - 1) - pointIndex);

                var alphaShift = indexDiffBetweenCorners > kShift ? kShift : indexDiffBetweenCorners;
                var betaShift = alphaShift / 3;

                var alphaAngle = RadiansToDegrees(CalculateAngle(points[pointIndex], points[pointIndex - alphaShift], points[pointIndex + alphaShift]));
                var betaAngle = RadiansToDegrees(CalculateAngle(points[pointIndex], points[pointIndex - betaShift], points[pointIndex + betaShift]));

                var thresholdA = 10.0 + 800.0 / (alphaAngle + 35.0);

                if ((betaAngle - alphaAngle) >= thresholdA)
                {
                    cornerIndices.RemoveAt(i);
                    --i;
                }
            }

            return cornerIndices;
        }

        private static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        /// <summary>
        /// Calculates the angle between two vectors (a - origin) and (b - origin)
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static double CalculateAngle(StylusPoint origin, StylusPoint a, StylusPoint b)
        {
            var aVec = new Vector(a.X - origin.X, a.Y - origin.Y);
            var bVec = new Vector(b.X - origin.X, b.Y - origin.Y);

            return Math.Acos((aVec * bVec) / (aVec.Length * bVec.Length));
        }

        private List<int> PostProcessCorners(StylusPointCollection points, List<int> cornerIndices, List<double> straws)
        {
            var bContinue = false;

            while (!bContinue)
            {
                bContinue = true;

                for (var i = 1; i < cornerIndices.Count; ++i)
                {
                    var c1 = cornerIndices[i - 1];
                    var c2 = cornerIndices[i];

                    if (!IsLine(points, c1, c2))
                    {
                        var newCorner = GetHalfwayCorner(straws, c1, c2);
                        cornerIndices.Insert(i, newCorner);
                        bContinue = false;
                    }
                }
            }

            for (var i = 1; i < cornerIndices.Count - 1; ++i)
            {
                var c1 = cornerIndices[i - 1];
                var c2 = cornerIndices[i + 1];

                if (IsLine(points, c1, c2))
                {
                    cornerIndices.RemoveAt(i);
                    --i;
                }
            }

            return cornerIndices;
        }

        /// <summary>
        /// Returns the index of the point that represents a possible corner between the points a and b
        /// </summary>
        /// <param name="straws"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static int GetHalfwayCorner(IReadOnlyList<double> straws, int a, int b)
        {
            var quarter = (b - a) / 4.0;
            var minValue = double.PositiveInfinity;
            var minIndex = (int)Math.Round(a + quarter, MidpointRounding.AwayFromZero);

            for (var i = minIndex; i < (int)Math.Round(b - quarter, MidpointRounding.AwayFromZero); ++i)
            {
                if (straws[i] < minValue)
                {
                    minValue = straws[i];
                    minIndex = i;
                }
            }

            return minIndex;
        }

        /// <summary>
        /// Returns a boolean for whether or not the stroke segment between points at a and b is a line
        /// </summary>
        /// <param name="points"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool IsLine(StylusPointCollection points, int a, int b)
        {
            const double threshold = 0.95;

            var distance = ComputeDistance(points[a], points[b]);
            var pathDistance = ComputePathDistance(points, a, b);

            return (distance / pathDistance) > threshold;
        }

        private StylusPointCollection GetIntersections(StylusPointCollection resampledPoints)
        {
            var intersectionPoints = new StylusPointCollection();
            var pointsToCheck = resampledPoints.Clone();

            var numPoints = pointsToCheck.Count;
            for (var i = 0; i < numPoints - 1; ++i)
            {
                // Get first line segment and remove the first point from the collection
                var a = pointsToCheck[0];
                var b = pointsToCheck[1];
                pointsToCheck.RemoveAt(0);

                for (var j = 1; j < pointsToCheck.Count - 1; ++j)
                {
                    var c = pointsToCheck[j];
                    var d = pointsToCheck[j + 1];

                    var intersectionPoint = new StylusPoint();
                    if (ComputeLineIntersection(a, b, c, d, ref intersectionPoint))
                    {
                        intersectionPoints.Add(intersectionPoint);
                    }
                }

            }

            return intersectionPoints;
        }

        /// <summary>
        /// Calculates the intersection between lines defined by line (a,b) and line (c,d).
        /// Got equation to compute line intersection from here: http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        private static bool ComputeLineIntersection(StylusPoint a, StylusPoint b, StylusPoint c, StylusPoint d, ref StylusPoint intersection)
        {
            var denominator = (d.Y - c.Y) * (b.X - a.X) - (d.X - c.X) * (b.Y - a.Y);

            if (Math.Abs(denominator) > 0.00001f) // Not coincident or parallel lines
            {
                var unknownAb = ((d.X - c.X) * (a.Y - c.Y) - (d.Y - c.Y) * (a.X - c.X)) / denominator;
                var unknownCd = ((b.X - a.X) * (a.Y - c.Y) - (b.Y - a.Y) * (a.X - c.X)) / denominator;

                if (unknownAb >= 0.0 && unknownAb <= 1.0 && unknownCd >= 0.0 && unknownCd <= 1.0)
                {
                    intersection.X = a.X + unknownAb * (b.X - a.X);
                    intersection.Y = a.Y + unknownAb * (b.Y - a.Y);
                    return true;
                }
                else
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Determines if two lines, (ab) and (cd) are coincident with each other.
        /// Created with help from the following URLs:
        /// - http://web.archive.org/web/20060911055655/http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline2d/
        /// - http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static bool AreLinesCoincident(StylusPoint a, StylusPoint b, StylusPoint c, StylusPoint d)
        {
            var denominator = (d.Y - c.Y) * (b.X - a.X) - (d.X - c.X) * (b.Y - a.Y);
            var unknownAb = ((d.X - c.X) * (a.Y - c.Y) - (d.Y - c.Y) * (a.X - c.X));
            var unknownCd = ((b.X - a.X) * (a.Y - c.Y) - (b.Y - a.Y) * (a.X - c.X));

            if (Math.Abs(denominator) <= 0.00001 && Math.Abs(unknownAb) <= 0.00001 && Math.Abs(unknownCd) <= 0.00001)
                return true;

            return false;
        }

        /// <summary>
        /// After preprocessing the ink stroke, this method finds a gesture that best matches the stroke input from the user.
        /// </summary>
        /// <param name="resampledPoints"></param>
        /// <param name="cornerIndices"></param>
        /// <param name="intersectionPoints"></param>
        /// <returns></returns>
        private Tuple<GestureType, Shape> FindGesture(StylusPointCollection resampledPoints, IList<int> cornerIndices, StylusPointCollection intersectionPoints)
        {
            var tuple = Tuple.Create(GestureType.None, (Shape)(new Rectangle()));

            // Stroke bounds
            var pointsBounds = new Rect();
            foreach (var t in resampledPoints)
            {
                pointsBounds.Union((Point)t);
            }

            // Stroke centroid
            var centroid = new StylusPoint(pointsBounds.Left + pointsBounds.Width / 2.0, pointsBounds.Top + pointsBounds.Height / 2.0);

            if (intersectionPoints.Count == 0) // 2D Primitives
            {
                if (IsClosedShape(cornerIndices, resampledPoints)) // Square, Rectangle, Triangle, Ellipse, Circle
                {
                    if (cornerIndices.Count == 2)
                    {
                        var minorAxis = double.PositiveInfinity;
                        var majorAxis = double.NegativeInfinity;

                        foreach (var radiusLength in resampledPoints.Select(t => new Vector(centroid.X - t.X, centroid.Y - t.Y).Length))
                        {
                            if (radiusLength < minorAxis)
                                minorAxis = radiusLength;

                            if (radiusLength > majorAxis)
                                majorAxis = radiusLength;
                        }

                        if ((minorAxis / majorAxis) >= KAxisDifferenceThreshold) // Circle
                        {
                            var ellipse = new Ellipse
                            {
                                Fill = Brushes.Red,
                                StrokeThickness = 2,
                                Stroke = Brushes.Black,
                                IsHitTestVisible = true,
                                Width = majorAxis * 2.0,
                                Height = majorAxis * 2.0
                            };

                            tuple = Tuple.Create(GestureType.Circle, (Shape)ellipse);
                        }
                        else // Ellipse
                        {
                            var ellipse = new Ellipse
                            {
                                Fill = Brushes.Gray,
                                StrokeThickness = 2,
                                Stroke = Brushes.Black,
                                IsHitTestVisible = true,
                                Width = majorAxis * 2.0,
                                Height = minorAxis * 2.0
                            };

                            tuple = Tuple.Create(GestureType.Ellipse, (Shape)ellipse);
                        }
                    }
                    else if (cornerIndices.Count == 4)
                    {
                        var point1 = (Point)resampledPoints[cornerIndices[0]];
                        var point2 = (Point)resampledPoints[cornerIndices[1]];
                        var point3 = (Point)resampledPoints[cornerIndices[2]];

                        var polygon = new Polygon
                        {
                            Fill = Brushes.Firebrick,
                            StrokeThickness = 2,
                            Stroke = Brushes.Black,
                            IsHitTestVisible = true
                        };

                        var pointCollection = new PointCollection { point1, point2, point3 };
                        polygon.Points = pointCollection;

                        tuple = Tuple.Create(GestureType.Triangle, (Shape)polygon);
                    }
                    else if (cornerIndices.Count == 5)
                    {
                        var angle1 = CalculateAngle(resampledPoints[cornerIndices[0]], resampledPoints[cornerIndices[1]],
                            resampledPoints[cornerIndices[3]]);
                        var angle2 = CalculateAngle(resampledPoints[cornerIndices[1]], resampledPoints[cornerIndices[0]],
                            resampledPoints[cornerIndices[2]]);
                        var angle3 = CalculateAngle(resampledPoints[cornerIndices[2]], resampledPoints[cornerIndices[1]],
                            resampledPoints[cornerIndices[3]]);
                        var angle4 = CalculateAngle(resampledPoints[cornerIndices[3]], resampledPoints[cornerIndices[0]],
                            resampledPoints[cornerIndices[2]]);

                        if ((Math.Abs(Math.PI / 2.0 - angle1) <= KRectangleAngleThresh)
                            && (Math.Abs(Math.PI / 2.0 - angle2) <= KRectangleAngleThresh)
                            && (Math.Abs(Math.PI / 2.0 - angle3) <= KRectangleAngleThresh)
                            && (Math.Abs(Math.PI / 2.0 - angle4) <= KRectangleAngleThresh))
                        {
                            var firstSide = (Point)resampledPoints[cornerIndices[1]] -
                                            (Point)resampledPoints[cornerIndices[0]];
                            var secondSide = (Point)resampledPoints[cornerIndices[2]] -
                                             (Point)resampledPoints[cornerIndices[1]];
                            var minLength = Math.Min(firstSide.Length, secondSide.Length);
                            var maxLength = Math.Max(firstSide.Length, secondSide.Length);

                            if ((minLength / maxLength) >= KRectangleAspectThreshold)
                            // Side Aspect Ratio close to 1:1 => Square
                            {
                                var rectangle = new Rectangle
                                {
                                    Fill = Brushes.Green,
                                    StrokeThickness = 2,
                                    Stroke = Brushes.Black,
                                    IsHitTestVisible = true,
                                    Width = maxLength,
                                    Height = maxLength
                                };

                                tuple = Tuple.Create(GestureType.Square, (Shape)rectangle);
                            }
                            else // Rectangle
                            {
                                var rectangle = new Rectangle
                                {
                                    Fill = Brushes.Blue,
                                    StrokeThickness = 2,
                                    Stroke = Brushes.Black,
                                    IsHitTestVisible = true,
                                    Width = maxLength,
                                    Height = minLength
                                };

                                tuple = Tuple.Create(GestureType.Rectangle, (Shape)rectangle);
                            }
                        }
                    }
                }
                else if (cornerIndices.Count == 3) // Arrow
                {
                    var bCoincidentLines = AreLinesCoincident(resampledPoints[cornerIndices[0]],
                        resampledPoints[cornerIndices[1]],
                        resampledPoints[cornerIndices[1]],
                        resampledPoints[cornerIndices[2]]);

                    var bHeadAngleOk = CalculateAngle(resampledPoints[cornerIndices[1]], resampledPoints[cornerIndices[0]], resampledPoints[cornerIndices[2]]) <= Math.PI / 4.0;

                    if (!bCoincidentLines && bHeadAngleOk)
                    {
                        var point1 = (Point)resampledPoints[cornerIndices[0]];
                        var point2 = (Point)resampledPoints[cornerIndices[1]];
                        var point3 = (Point)resampledPoints[cornerIndices[2]];

                        var polyline = new Polyline
                        {
                            Fill = Brushes.Transparent,
                            StrokeThickness = 5,
                            Stroke = Brushes.Black,
                            IsHitTestVisible = true
                        };

                        var pointCollection = new PointCollection { point1, point2, point3 };
                        polyline.Points = pointCollection;

                        tuple = Tuple.Create(GestureType.Arrow, (Shape)polyline);
                    }
                }
            }
            else if (intersectionPoints.Count >= 3) // Scribble
            {
                var rectangle = new Rectangle
                {
                    Width = pointsBounds.Width,
                    Height = pointsBounds.Height,
                    IsHitTestVisible = true
                };

                tuple = Tuple.Create(GestureType.Erase, (Shape)rectangle);
            }

            Console.WriteLine(@"Recognized Gesture: " + tuple.Item1);

            return tuple;
        }

        /// <summary>
        /// This method detects if the resampled points passed to it are a closed shape.  It does this
        /// by detecting if the start and end points are within a threshold distance of each other.
        /// </summary>
        /// <param name="cornerIndices"></param>
        /// <param name="resampledPoints"></param>
        /// <returns></returns>
        private bool IsClosedShape(IList<int> cornerIndices, StylusPointCollection resampledPoints)
        {
            if (resampledPoints.Count < 2)
                return false;

            if (IsLine(resampledPoints, 0, resampledPoints.Count - 1))
                return false;

            // We treat each point as a circle and see if the circles intersect each other
            var centerDistance = Math.Abs(ComputeDistance(resampledPoints[cornerIndices[0]], resampledPoints[cornerIndices[cornerIndices.Count - 1]]));

            if (centerDistance > KEndpointThreshold * 2.0)
                return false;
            else
                return true;
        }
    }
}
