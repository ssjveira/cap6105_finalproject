using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private static readonly string KSamplePath = "samples.xml";
        private const int KImageSideLength = 256;

        private readonly IList<SketchData> m_SampleSketchFeatureList = new List<SketchData>(); 

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
        /// Overload that allows for recognizing a stroke that's in the form of a StrokeCollection.
        /// </summary>
        /// <param name="strokes"></param>
        /// <returns></returns>
        public string Recognize(StrokeCollection strokes)
        {
            // DEBUG - Save stroke as an image to a file
            // TODO: Remove this eventually
            InkConversionUtils.SaveStrokesToImageFile(strokes, KImageSideLength, System.IO.Directory.GetCurrentDirectory() + @"\userStroke.png");
            
            // Convert strokes to a feature vector
            var testFeatures = InkConversionUtils.featurizedBitmap(InkConversionUtils.StrokesToBitmap(strokes, KImageSideLength));
            
            // DEBUG- Save features as XML to a file
            // TODO: Remove this eventually
            InkConversionUtils.SaveFeatures(System.IO.Directory.GetCurrentDirectory() + @"\testFeature.xml", testFeatures);

            return Recognize(testFeatures);
        }

        /// <summary>
        /// Overload that allows for recognizing a stroke that's in the form of a feature vector
        /// </summary>
        /// <param name="testFeature"></param>
        /// <returns></returns>
        public string Recognize(List<double> testFeature)
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
