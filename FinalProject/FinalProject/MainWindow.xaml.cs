using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NutritionalInfoApp.Utils;
using Nutritionix;

namespace NutritionalInfoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
         public class Fea
    {
        public string s;
        public List<double> x;
    }
    public partial class MainWindow : Window
    {
        private readonly string m_InfoDataStr = "See info";
        private readonly string m_BackStr = "Back to results";

        private static List<string> st = new List<string>();
        private static List<double[]> db = new List<double[]>();
        public MainWindow()
        {
            InitializeComponent();

            var train = RecognitionUtils.ReadFeatures();
            for (int i = 0; i < train.Count; i++)
            {
                st.Add(train[i].s);
                db.Add(train[i].x.ToArray());
            }
            // Use bezier smoothing when rendering the ink strokes
            AppInkCanvas.DefaultDrawingAttributes.FitToCurve = true;

            ResultButton.Content = m_InfoDataStr;
        }

        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            AppInkCanvas.Strokes.Clear();
        }

        private void NutritionixHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            // Launches a browser that navigates to the Nutritionix API homepage
            // TODO: Make this a utility function
            try
            {
                // Changes the cursor so the user knows that the hyperlink has been clicked on
                Mouse.OverrideCursor = Cursors.AppStarting;

                // Opens up the browser
                Process.Start("http://www.nutritionix.com/api");
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                // Changes the mouse cursor back after Process.Start has finished running
                Mouse.OverrideCursor = null;
            }

        }

        private void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            var searchResponse = NutritionHelper.Search(SearchText.Text);
            ResultListView.ItemsSource = searchResponse.Results;
        }

        private void AppInkCanvas_OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            InkConversionUtils.SaveStrokesToImageFile(AppInkCanvas.Strokes, 256.0,
                System.IO.Directory.GetCurrentDirectory() + @"\userStroke.png");
            var bitmap = InkConversionUtils.StrokesToBitmap(AppInkCanvas.Strokes, 256.0);
            var testFeatures = InkConversionUtils.featurizedBitmap(bitmap);
            InkConversionUtils.SaveFeatures(System.IO.Directory.GetCurrentDirectory() + @"\testFeature.xml", testFeatures);
            
            SearchText.Text = RecognitionUtils.searchResult(testFeatures, st, db);
        }

        private void ResultListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var nutritionalInfo = new List<string>();

            if (ResultListView.SelectedItem != null)
            {
                foreach (var item in e.AddedItems)
                {
                    var searchResult = item as SearchResult;

                    if (searchResult != null)
                    {
                        var resultItem = NutritionHelper.RetrieveItem(searchResult.Item.Id);
                        nutritionalInfo.Add("Calcium: " + (resultItem.NutritionFact_Calcium != null ? resultItem.NutritionFact_Calcium.ToString() : ""));
                        nutritionalInfo.Add("Calories: " + (resultItem.NutritionFact_Calories != null ? resultItem.NutritionFact_Calories.ToString() : ""));
                    }
                }
            }

            NutrionalListView.ItemsSource = nutritionalInfo;
        }

        private void ResultButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ResultButton.Content.Equals(m_InfoDataStr))
            {
                ResultButton.Content = m_BackStr;

                ResultListView.Visibility = Visibility.Hidden;
                NutrionalListView.Visibility = Visibility.Visible;
            }
            else
            {
                ResultButton.Content = m_InfoDataStr;

                ResultListView.Visibility = Visibility.Visible;
                NutrionalListView.Visibility = Visibility.Hidden;
            }
        }
    }
}
