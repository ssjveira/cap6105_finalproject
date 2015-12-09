using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NutritionalInfoApp.Utils
{
    public class Fea
    {
        public string s;
        public List<double> x;
    }

    class RecognitionUtils
    {
        private static string sPath = "D:\\food_samples\\samplesALL";
        public static string searchResult(List<double> test)
        {
            //MLApp.MLApp matlab = new MLApp.MLApp();
            var train = ReadFeatures(); List<string> st = new List<string>(); List<double[]> db = new List<double[]>();
            foreach (Fea f in train) {
                st.Add(f.s);
                db.Add(f.x.ToArray());
            }
            var testArr = test.ToArray();
            var lblArr = st.ToArray();
            //var trainArr = db.ToArray();
            double distance = 0.0; List<double> disArr = new List<double>();
            for (int i = 0; i < lblArr.Length; i++)
            {
              for (int j=0;j<121;j++){
                  //distance = KNNs.ChebyshevDistance(testArr, db[i]);
                  distance = Math.Abs(testArr[j] - db[i][j]);
               
            }
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
                            for (int i = 0; i < 121; i++)
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
