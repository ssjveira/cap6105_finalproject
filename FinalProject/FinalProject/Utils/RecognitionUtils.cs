using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NutritionalInfoApp.Utils
{


    class RecognitionUtils
    {
        private static string sPath = "D:\\food_samples\\samplesALL.xml";
        public static string searchResult(List<double> test, List<string> st, List<double[]> db)
        {
            //MLApp.MLApp matlab = new MLApp.MLApp();
             

            
            //int count = 0;
            //foreach(var trainingData in train)
            //{
            //    Console.Write("Training #{0}: {1}, {2}\n", ++count, trainingData.s, trainingData.x);
            //}
            

            var testArr = test.ToArray();
            var lblArr = st.ToArray();
            double distance = 0.0; List<double> disArr = new List<double>();
            for (int i = 0; i < lblArr.Length; i++)
            {
                distance = KNNs.ManhattanDistance(testArr, db[i]);
            //  for (int j=0;j<121;j++){                
                 // distance += Math.Abs(testArr[j] - db[i][j]);               
           // }
              disArr.Add(distance);
            }
            int indx = disArr.IndexOf(disArr.Min());
            return st[indx];
        }
        public static List<Fea> foodRow = new List<Fea>();
        public static List<Fea> ReadFeatures()
        {
            bool success = true;
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(sPath);
                reader.WhitespaceHandling = WhitespaceHandling.None;
                reader.MoveToContent();
               // string foodName = filename.Substring(16, filename.Length - 20);
                int j = 0;
                if (reader.LocalName == "Features")
                {
                    reader.Read();
                    
                    while (reader.LocalName != "Features")
                    {
                        string foodName = reader.LocalName;
                        List<double> foodRowDT = new List<double>();
                        if (reader.LocalName == foodName)
                        {
                            foodRow.Add(new Fea());
                            foodRow[j].s = foodName;
                            for (int i = 0; i < 1024; i++)
                            {
                                foodRowDT.Add(XmlConvert.ToDouble(reader.GetAttribute("F" + i)));
                                //foodRow[0].x.Add(XmlConvert.ToDouble(reader.GetAttribute("F" + i)));
                            }
                            foodRow[j].x = foodRowDT;
                            j++;
                            reader.ReadStartElement(foodName);
                        }

                    }
                }
            }
            catch (XmlException xex)
            {
                Console.WriteLine(xex.Message);
                Console.Write(xex.StackTrace);
                Console.WriteLine();
                success = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Write(ex.StackTrace);
                Console.WriteLine();
                success = false;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return foodRow;
        }
    }
} 
