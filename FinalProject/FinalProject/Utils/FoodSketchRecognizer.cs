using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Xml;

namespace NutritionalInfoApp.Utils
{

    /// <summary>
    /// Class used to recognize different food items from a user sketch.
    /// </summary>
    public class FoodSketchRecognizer
    {
        /// <summary>
        /// Class representing a sketch from the training dataset
        /// </summary>
        private class SketchData
        {
            public string Category = "";
            public readonly IList<double> FeatureData = new List<double>();
        }

        // Constants
        private const string KSamplePath = "samples.xml";
        private const int KImageSideLength = 256;

        private readonly IList<SketchData> m_SampleSketchFeatureList = new List<SketchData>();
        private readonly StrokeCollection m_CollectedStrokeCollection = new StrokeCollection();

        public FoodSketchRecognizer()
        {
            // Create a new samples.xml file using the training samples XML resource file
            // if samples.xml doesn't already exist in the current working directory.
            if (!File.Exists(KSamplePath))
            {
                var xmldoc = new XmlDocument();
                xmldoc.LoadXml(Properties.Resources.TrainingSamples);
                xmldoc.Save(KSamplePath);
            }

            LoadFeatures();
        }

        /// <summary>
        /// Parses the collected stroke collection and outputs a set of names of recognized sketches 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Recognize()
        {
            // DEBUG
            /*
            if (System.IO.Directory.GetFiles(@"\tests\", "*.png").Length > 0)
            {
                foreach (var file in System.IO.Directory.EnumerateFiles(@"\tests\", "*.png"))
                    System.IO.File.Delete(file);
            }
            */

            var sketches = ParseStrokes();

            // DEBUG
            /*
            foreach (var strokeCollection in parsedStrokes)
                InkConversionUtils.SaveStrokesToImageFile(strokeCollection, 256.0, System.IO.Directory.GetCurrentDirectory() + @"\tests\" + DateTime.Now.ToLongDateString().ToString() + ".png");
            */

            return sketches.Select(Recognize).ToList();
        }

        /// <summary>
        /// Overload that allows for recognizing a stroke that's in the form of a StrokeCollection.
        /// </summary>
        /// <param name="strokes"></param>
        /// <returns></returns>
        private string Recognize(StrokeCollection strokes)
        {
            // DEBUG - Save stroke as an image to a file
            //InkConversionUtils.SaveStrokesToImageFile(strokes, KImageSideLength, System.IO.Directory.GetCurrentDirectory() + @"\userStroke.png");
            
            // Convert strokes to a feature vector
            var testFeatures = InkConversionUtils.featurizedBitmap(InkConversionUtils.StrokesToBitmap(strokes, KImageSideLength));
            
            // DEBUG - Save features as XML to a file
            //InkConversionUtils.SaveFeatures(System.IO.Directory.GetCurrentDirectory() + @"\testFeature.xml", testFeatures);

            return Recognize(testFeatures);
        }

        /// <summary>
        /// Overload that allows for recognizing a stroke that's in the form of a feature vector
        /// </summary>
        /// <param name="testFeature"></param>
        /// <returns></returns>
        private string Recognize(List<double> testFeature)
        {
            var distanceArray = new List<Tuple<string, double>>();

            foreach (var sampleSketchFeature in m_SampleSketchFeatureList)
            {
                distanceArray.Add(Tuple.Create(sampleSketchFeature.Category, KNNs.ManhattanDistance(testFeature.ToArray(), sampleSketchFeature.FeatureData.ToArray())));
            }

            var result = distanceArray.OrderBy(tuple => tuple.Item2).First();

            return result != null ? result.Item1 : "Unknown";
        }

        /// <summary>
        /// Method that parses the collection of strokes taken from the ink canvas and returns a set
        /// of strokes to pass to the recognizer.
        /// HACK: This code assumes the user wrote the symbols in left to right order (i.e. sketch "+" sketch)
        /// </summary>
        /// <param name="strokes"></param>
        /// <returns></returns>
        private IEnumerable<StrokeCollection> ParseStrokes()
        {
            var sketchComparer = new SketchComparer();
            var strokeCollectionList = InkHelper.PerformInkSegmentation(m_CollectedStrokeCollection);

            var plusList = new List<StrokeCollection>(); // Set of sketches of the "+" symbol
            var othersList = new List<StrokeCollection>(); // Other sketches found
            var sketchesToReturn = new List<StrokeCollection>(); // Sketches to return from this function based on how many "+" symbols we find

            // Parse out "+" sketches and other sketches
            foreach (var strokeCollection in strokeCollectionList)
            {
                if (strokeCollection.Count() == 2)
                {
                    var lineAngle1 = InkHelper.ComputeStrokeAngle(strokeCollection[0]) * 180 / Math.PI;
                    var lineAngle2 = InkHelper.ComputeStrokeAngle(strokeCollection[1]) * 180 / Math.PI;
                    var angleBetweenStrokes = InkHelper.ComputeAngleBetweenStrokes(strokeCollection[0], strokeCollection[1]) * 180 / Math.PI;

                    if ((Math.Abs(lineAngle1) - 90) < 20 && Math.Abs(lineAngle2) < 20 && angleBetweenStrokes > 70)
                    {
                        plusList.Add(strokeCollection);
                    }
                    else if (Math.Abs(lineAngle1) < 20 && (Math.Abs(lineAngle2) - 90) < 20 && angleBetweenStrokes > 70)
                    {
                        plusList.Add(strokeCollection);
                    }
                    else
                    {
                        othersList.Add(strokeCollection);
                    }
                }
                else
                {
                    othersList.Add(strokeCollection);
                }
            }

            if (plusList.Count == 1)
            {
                var plusBounds = plusList[0].GetBounds();
                var leftSketch = new StrokeCollection();
                var rightSketch = new StrokeCollection();

                foreach (var sketch in othersList)
                {
                    if (sketch.GetBounds().X < plusBounds.X)
                        leftSketch.Add(sketch);
                    else
                        rightSketch.Add(sketch);
                }

                if(leftSketch.Count > 0)
                    sketchesToReturn.Add(leftSketch);
                
                if(rightSketch.Count > 0)
                    sketchesToReturn.Add(rightSketch);
            }
            else if (plusList.Count >= 2) // Handle the first two pluses, any others are ignored.
            {
                // Sort plus list
                plusList.Sort(sketchComparer);

                var plusBounds1 = plusList[0].GetBounds();
                var plusBounds2 = plusList[1].GetBounds();
                var leftSketch = new StrokeCollection();
                var middleSketch = new StrokeCollection();
                var rightSketch = new StrokeCollection();

                foreach (var sketch in othersList)
                {
                    var bounds = sketch.GetBounds();

                    if (bounds.X < plusBounds1.X && bounds.X < plusBounds2.X)
                        leftSketch.Add(sketch);
                    else if (bounds.X > plusBounds1.X && bounds.X < plusBounds2.X)
                        middleSketch.Add(sketch);
                    else if (bounds.X > plusBounds1.X && bounds.X > plusBounds2.X)
                        rightSketch.Add(sketch);
                }

                if (leftSketch.Count > 0)
                    sketchesToReturn.Add(leftSketch);

                if (middleSketch.Count > 0)
                    sketchesToReturn.Add(middleSketch);

                if (rightSketch.Count > 0)
                    sketchesToReturn.Add(rightSketch);
            }
            else // Treat all of the strokes as one sketch
            {
                sketchesToReturn.Add(m_CollectedStrokeCollection);
            }

            return sketchesToReturn;
        }

        /// <summary>
        /// Copies the passed in stroke collection and saves as a part of the state of the FoodSketchRecognizer.
        /// This stroke collection will be used to recognize sketch(es).
        /// </summary>
        /// <param name="strokeCollection"></param>
        public void SetStrokes(StrokeCollection strokeCollection)
        {
            m_CollectedStrokeCollection.Clear();
            m_CollectedStrokeCollection.Add(strokeCollection.Clone());
        }

        /// <summary>
        /// Loads the training sample features from the samples xml file, which should be located in the same directory as the application
        /// </summary>
        private void LoadFeatures()
        {
            using (var reader = XmlReader.Create(KSamplePath))
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(reader);

                foreach (XmlElement rootNode in xmlDocument.GetElementsByTagName("Features"))
                    foreach (XmlElement featureNode in rootNode.ChildNodes)
                    {
                        var sketchData = new SketchData { Category = featureNode.LocalName };

                        // TODO: Add count attribute to XML node to enforce feature count instead of hard coding it to 1024
                        for (var i = 0; i < 1024; i++)
                        {
                            var attribute = featureNode.GetAttribute("F" + i);

                            Debug.Assert(!attribute.Equals(""), "Attribute F" + i + " does not exist in the XML node!");

                            if (!attribute.Equals(""))
                                sketchData.FeatureData.Add(XmlConvert.ToDouble(attribute));
                        }

                        m_SampleSketchFeatureList.Add(sketchData);
                    }
            }
        }

    }
} 
