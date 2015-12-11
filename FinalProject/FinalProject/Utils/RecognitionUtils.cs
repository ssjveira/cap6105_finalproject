using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Xml;

namespace NutritionalInfoApp.Utils
{
    public static class RecognitionUtils
    {
        /// <summary>
        /// Method that parses the collection of strokes taken from the ink canvas and returns a set
        /// of strokes to pass to the recognizer.
        /// </summary>
        /// <param name="colStrokes"></param>
        /// <returns></returns>
        public static List<StrokeCollection> Parser(StrokeCollection colStrokes)
        {
            List<StrokeCollection> strList = new List<StrokeCollection>();
            StrokeCollection str = colStrokes; bool plusStatus = false;
            List<StrokeCollection> plusList = new List<StrokeCollection>();
            StrokeCollection l1 = new StrokeCollection();
            StrokeCollection l2 = new StrokeCollection();
            StrokeCollection l3 = new StrokeCollection();
            for (int i = 0; i < str.Count - 1; i++)
            {
                if (str.Count >= 3)
                {
                    StrokeCollection newStrokes = new StrokeCollection();
                    Rect rec1 = str[i].GetBounds();
                    Rect rec2 = str[i + 1].GetBounds();
                    if (Math.Abs(rec1.Size.Height - rec2.Size.Height) < 10 && Math.Abs(rec1.Size.Width - rec2.Size.Width) < 10)
                    {
                        if (Math.Abs(rec1.Location.X - rec2.Location.X) < 10 && Math.Abs(rec1.Location.Y - rec2.Location.Y) < 10)
                        {
                            if (rec1.IntersectsWith(rec2))
                            {
                                double P22X = str[i + 1].StylusPoints[str[i + 1].StylusPoints.Count - 1].X;
                                double P22Y = str[i + 1].StylusPoints[str[i + 1].StylusPoints.Count - 1].Y;
                                double P21X = str[i + 1].StylusPoints[0].X;
                                double P21Y = str[i + 1].StylusPoints[0].Y;
                                double delta2Y = Math.Abs(P22Y - P21Y);
                                double delta2X = Math.Abs(P22X - P21X);
                                double angleInDegrees2 = Math.Atan(delta2Y / delta2X) * 180 / Math.PI;
                                double P12X = str[i].StylusPoints[str[i].StylusPoints.Count - 1].X;
                                double P12Y = str[i].StylusPoints[str[i].StylusPoints.Count - 1].Y;
                                double P11X = str[i].StylusPoints[0].X;
                                double P11Y = str[i].StylusPoints[0].Y;
                                double delta1Y = Math.Abs(P12Y - P11Y);
                                double delta1X = Math.Abs(P12X - P11X);
                                double angleInDegrees1 = Math.Atan(delta1Y / delta1X) * 180 / Math.PI;
                                double deltaY = Math.Abs(P22Y - P12Y);
                                double deltaX = Math.Abs(P22X - P12X);
                                double angleInDegrees = Math.Atan(deltaY / deltaX) * 180 / Math.PI;
                                if (angleInDegrees1 < 20 && angleInDegrees2 < 20 && angleInDegrees > 70)
                                {
                                    StrokeCollection plus = new StrokeCollection();
                                    plus.Add(str[i]);
                                    plus.Add(str[i + 1]);
                                    str.Remove(plus);
                                    plusList.Add(plus);
                                    plusStatus = true;

                                }

                            }
                        }
                    }
                }
            }
            if (plusStatus)
            {
                if (plusList.Count == 1)
                {
                    Rect prec = plusList[0].GetBounds();
                    for (int j = 0; j < str.Count; j++)
                    {                       
                        Rect rec = str[j].GetBounds();
                        if (rec.X < prec.X)
                            l1.Add(str[j]);
                        else
                            l2.Add(str[j]);
                    }
                    strList.Add(l1);
                    strList.Add(l2);
                }
                else if (plusList.Count == 2)
                {
                    Rect prec1 = plusList[0].GetBounds();
                    Rect prec2 = plusList[1].GetBounds();
                    for (int j = 0; j < str.Count; j++){
                    Rect rec = str[j].GetBounds();
                    if (rec.X < prec1.X && rec.X < prec2.X)
                        l1.Add(str[j]);
                    else if (rec.X > prec1.X && rec.X < prec2.X)
                        l2.Add(str[j]);
                    else if (rec.X > prec1.X && rec.X > prec2.X)
                        l3.Add(str[j]);
                }
                    strList.Add(l1);
                    strList.Add(l2);
                    strList.Add(l3);
               }
            }
            else
            {
                strList.Add(str);
            }
            return strList;
        }
    }
} 
 