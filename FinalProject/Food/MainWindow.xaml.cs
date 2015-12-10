using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using System.Windows.Ink;
using System.Data.SqlClient;
using System.Windows.Navigation;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Drawing;

namespace Food
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class Fea
    {
        public string s;
        public List<double> x;
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        //private string sPath = System.IO.Path.GetDirectoryName(Application.Current.ToString());
        private string sPath = "D:\\food_samples\\";
        //string currentDirName = System.IO.Directory.GetCurrentDirectory();
        
        
        //Directory.GetDirectories(sPath);
        private void Create_Features(object sender, RoutedEventArgs e)
        {
           /// string sPath = "D:\\food_samples\\fish\\";
            List<string> imgFiles = new List<string>();
            List<List<double>> imgDT2 = new List<List<double>>();
            string[] folders = Directory.GetDirectories(sPath);
            List<string> foldersNames = new List<string>();
            foreach (string folder in folders)
            {               
                foreach (string file in Directory.EnumerateFiles(folder, "*.png"))
                {
                    imgFiles.Add(file);
                    foldersNames.Add(folder.Substring(16));
                    List<double> imgDT1 = new List<double>();
                    Bitmap bitmap1 = new Bitmap(file);
                    double dist = 0; double ftr = 0;
                    for (int n = 0; n < 256; n += 8)
                    {
                        for (int m = 0; m < 256; m += 8)
                        {
                            for (int i = n; i < n + 8; i++)
                            {
                                for (int j = m; j < m + 8; j++)
                                {
                                    ftr += bitmap1.GetPixel(i, j).R;
                                }
                            }
                            imgDT1.Add(ftr);
                            ftr = 0;
                        }
                    }
                    imgDT2.Add(imgDT1);
                }
            }

            vw.Text = imgFiles[0].ToString(); 
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = sPath;
            dlg.Filter = "XML (*.xml)|*.xml";
            dlg.DefaultExt = ".xml";
            dlg.FileName = "samplesAll.xml";
            dlg.AddExtension = true; 
            dlg.Title = "Save Sample As";
            dlg.RestoreDirectory = true;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // resample, scale, translate to origin
                // test_list.Text = dlg.FileName.ToString();
                SaveFeatures(foldersNames, imgDT2);
            }
        }
        

        public bool SaveFeatures(List<string> Labels, List<List<double>> weights)
        {
           string name = "SamplesALL";
            bool success = true;
            XmlTextWriter writer = null;
            try
            {
                writer = new XmlTextWriter(name, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument(true);
                writer.WriteStartElement("Features");
                int j = 0;
                foreach (List<double> wt in weights)
                {
                    int i = 0; 
                    writer.WriteStartElement(Labels[j]);
                    foreach (double pix in wt)
                    {
                        writer.WriteAttributeString("F"+i.ToString(), XmlConvert.ToString(pix));
                        i++;
                    }
                    writer.WriteEndElement(); // <Point />
                    j++;
                }
                writer.WriteEndElement();
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
                if (writer != null)
                    writer.Close();
            }
            return success; // Xml file successfully written (or not)
        }


        public bool uniFeatures(List<Fea> samples)
        {
            bool success = true;
            XmlTextWriter writer = null;
            try
            {
                writer = new XmlTextWriter("samplesALL", Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument(true);
                writer.WriteStartElement("Features"); int i = 0;

                    foreach (Fea f in samples)
                    {
                        writer.WriteStartElement(f.s);
                        for (int j = 0; j < 1024; j++)
                        {

                            writer.WriteAttributeString("F" + j.ToString(), XmlConvert.ToString(f.x[j]));

                        }
                        writer.WriteEndElement(); // <Point />
                    }
                writer.WriteEndElement();
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
                if (writer != null)
                    writer.Close();
            }
            return success; // Xml file successfully written (or not)
        }
        public List<Fea> foodRow = new List<Fea>();
        
        public List<Fea> ReadFeatures(string filename)
        {
            bool success = true;
            
            
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(filename);
                reader.WhitespaceHandling = WhitespaceHandling.None;
                reader.MoveToContent();
                string foodName = filename.Substring(16, filename.Length - 20); 
                int j = foodRow.Count;
                if (reader.LocalName == "Features")
                {
                    
                    reader.Read();               
                    while (reader.LocalName != "Features")
                    {
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

        private void features_Click(object sender, RoutedEventArgs e)
        {
            string[] fileEntries = Directory.GetFiles(sPath);
            foreach (string fileName in fileEntries) {
                //vw.Text = fileName.Substring(1, fileName.Length);
                ReadFeatures(fileName);
            }
           // vw.Text = foodFeatures[3][5].ToString();
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = sPath;
            dlg.Filter = "XML (*.xml)|*.xml";
            dlg.DefaultExt = ".xml";
            dlg.FileName = "All.xml";
            dlg.AddExtension = true;
            dlg.Title = "Save Sample As";
            dlg.RestoreDirectory = true;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // resample, scale, translate to origin
                // test_list.Text = dlg.FileName.ToString();
                uniFeatures(foodRow);
            }
        }

        private void save_canvas_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfg = new SaveFileDialog();
            sfg.InitialDirectory = sPath;
            sfg.Filter = "SVG Files (*.svg)|*.svg|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
            sfg.DefaultExt = ".svg";
            sfg.FileName = "Image";
            sfg.AddExtension = true;
            string f_name = "Image";
            int margin = (int)this._inkCanvas.Margin.Right;
            int width = (int)this._inkCanvas.ActualWidth - margin;
            //int width = 1111;
            int height = (int)this._inkCanvas.ActualHeight - margin;
            // int height = 1111;
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Pbgra32);
            var matrix = new Matrix();
            matrix.Scale(256, 256);
           
            rtb.Render(_inkCanvas);
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            //System.Windows.Media.Imaging.PngBitmapEncoder encoder1 = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            Nullable<bool> result = sfg.ShowDialog();
            if (result == true)
            {
                f_name = sfg.FileName;
                FileStream fs = new FileStream(f_name, FileMode.Create, FileAccess.ReadWrite);
                // _InkCanvas.Strokes.Save(fs);
                encoder.Save(fs);
                fs.Close();

            }
        }

    }
}
