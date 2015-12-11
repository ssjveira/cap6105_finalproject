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
    public partial class MainWindow : Window
    {
        // String constants
        private const string KInfoDataStr = "See info";
        private const string KBackStr = "Back to results";

        private readonly FoodSketchRecognizer m_FoodSketchRecognizer = new FoodSketchRecognizer();

        private bool m_IsNutritionixItemLoaded = false;

        public MainWindow()
        {
            InitializeComponent();

            // Use bezier smoothing when rendering the ink strokes
            AppInkCanvas.DefaultDrawingAttributes.FitToCurve = true;

            ResultButton.Content = KInfoDataStr;
        }

        private void SetResultsView()
        {
            if (m_IsNutritionixItemLoaded || ResultListView.SelectedItem == null) return;

            var searchResult = ResultListView.SelectedItem as SearchResult;
            if(searchResult != null)
                NutrionalDataGrid.ItemsSource = new List<Item>() { NutritionHelper.RetrieveItem(searchResult.Item.Id) };

            m_IsNutritionixItemLoaded = true;
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

        /*
        private void AppInkCanvas_OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + @"\tests\", "*.png").Length > 0)
            {
                foreach (string file in System.IO.Directory.EnumerateFiles(System.IO.Directory.GetCurrentDirectory() + @"\tests\", "*.png"))
                    System.IO.File.Delete(file);
            }
            List<StrokeCollection> ReadyStrokes = RecognitionUtils.Parser(AppInkCanvas.Strokes);
            SearchText.Text = ReadyStrokes.Count.ToString();
            foreach (StrokeCollection strcol in ReadyStrokes)
                InkConversionUtils.SaveStrokesToImageFile(strcol, 256.0, System.IO.Directory.GetCurrentDirectory() + @"\tests\" + DateTime.Now.ToLongDateString().ToString() + ".png");
            //var bitmap = InkConversionUtils.StrokesToBitmap(AppInkCanvas.Strokes, 256.0);
            // var testFeatures = InkConversionUtils.featurizedBitmap(bitmap);
            //  InkConversionUtils.SaveFeatures(System.IO.Directory.GetCurrentDirectory() + @"\testFeature.xml", testFeatures);

            //  SearchText.Text = RecognitionUtils.searchResult(testFeatures, st, db);
        }
        */

        private void AppInkCanvas_OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // DEBUG
            /*
            if (System.IO.Directory.GetFiles(@"\tests\", "*.png").Length > 0)
            {
                foreach (var file in System.IO.Directory.EnumerateFiles(@"\tests\", "*.png"))
                    System.IO.File.Delete(file);
            }
            */

            var parsedStrokes = RecognitionUtils.Parser(AppInkCanvas.Strokes);

            // DEBUG
            /*
            foreach (var strokeCollection in parsedStrokes)
                InkConversionUtils.SaveStrokesToImageFile(strokeCollection, 256.0, System.IO.Directory.GetCurrentDirectory() + @"\tests\" + DateTime.Now.ToLongDateString().ToString() + ".png");
            */

            // Print out text that will be used by NutritionixHelper to search for n objects and return the results
            // (i.e. With query string = "apple+tomato", nutritional info for an apple and a tomato will be retrieved
            // from the Nutritionix database.
            SearchText.Text = string.Join("+", m_FoodSketchRecognizer.Recognize(parsedStrokes));
        }

        private void ResultListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_IsNutritionixItemLoaded = false;
        }

        private void ResultButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ResultButton.Content.Equals(KInfoDataStr))
            {
                ResultButton.Content = KBackStr;

                SetResultsView();

                ResultListView.Visibility = Visibility.Hidden;
                ResultsLabel.Visibility = Visibility.Hidden;
                NutritionResultsViewer.Visibility = Visibility.Visible;
            }
            else
            {
                ResultButton.Content = KInfoDataStr;

                ResultListView.Visibility = Visibility.Visible;
                ResultsLabel.Visibility = Visibility.Visible;
                NutritionResultsViewer.Visibility = Visibility.Hidden;
            }
        }
    }
}
