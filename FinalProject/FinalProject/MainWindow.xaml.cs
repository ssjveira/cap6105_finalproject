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
using System.Windows.Ink;
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
        // String constants for search results button
        private const string KInfoDataStr = "See info";
        private const string KBackStr = "Back to results";

        // String constants for search results label
        private const string KSearchResultsStr = "Search Results";
        private const string KNutritionFactsStr = "Nutrition Facts";

        private readonly FoodSketchRecognizer m_FoodSketchRecognizer = new FoodSketchRecognizer();

        /// <summary>
        /// Object used to recognize gestures (2D primitives, scratch out, etc.) from written ink.
        /// </summary>
        private readonly IGestureRecognizer m_GestureRecognizer = new InkGestureRecognizer();

        private bool m_IsNutritionixItemLoaded = false;

        public MainWindow()
        {
            InitializeComponent();

            // Use bezier smoothing when rendering the ink strokes
            AppInkCanvas.DefaultDrawingAttributes.FitToCurve = true;

            ResultButton.Content = KInfoDataStr;
        }

        private void SetNutritionFactsView()
        {
            if (m_IsNutritionixItemLoaded || ResultListView.SelectedItem == null) return;

            var searchResult = ResultListView.SelectedItem as SearchResult;
            if (searchResult != null)
            {
                var nutritionFacts = new List<Tuple<string, decimal?>>();

                var item = NutritionHelper.RetrieveItem(searchResult.Item.Id);

                nutritionFacts.Add(Tuple.Create("Calories", item.NutritionFact_Calories));
                nutritionFacts.Add(Tuple.Create("Calories from", item.NutritionFact_CaloriesFromFat));
                nutritionFacts.Add(Tuple.Create("Total Fat", item.NutritionFact_TotalFat));
                nutritionFacts.Add(Tuple.Create("Saturated Fat", item.NutritionFact_SaturatedFat));
                nutritionFacts.Add(Tuple.Create("Trans Fat", item.NutritionFact_TransFat));
                nutritionFacts.Add(Tuple.Create("Cholesterol", item.NutritionFact_Cholesterol));
                nutritionFacts.Add(Tuple.Create("Sodium", item.NutritionFact_Sodium));
                nutritionFacts.Add(Tuple.Create("Total Carbohydrate", item.NutritionFact_TotalCarbohydrate));
                nutritionFacts.Add(Tuple.Create("Dietary Fiber", item.NutritionFact_DietaryFiber));
                nutritionFacts.Add(Tuple.Create("Sugars", item.NutritionFact_Sugar));
                nutritionFacts.Add(Tuple.Create("Protein", item.NutritionFact_Protein));
                nutritionFacts.Add(Tuple.Create("Vitamin A", item.NutritionFact_VitaminA));
                nutritionFacts.Add(Tuple.Create("Vitamin C", item.NutritionFact_VitaminC));
                nutritionFacts.Add(Tuple.Create("Calcium", item.NutritionFact_Calcium));
                nutritionFacts.Add(Tuple.Create("Iron", item.NutritionFact_Iron));

                NutrionalDataGrid.ItemsSource = nutritionFacts;
            }

            m_IsNutritionixItemLoaded = true;
        }


        private GestureType RecognizeGesture(Stroke stroke)
        {
            m_GestureRecognizer.SetStroke(stroke);
            m_GestureRecognizer.Recognize();

            return m_GestureRecognizer.GetRecognizedGesture();
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
            var recognizedGesture = RecognizeGesture(e.Stroke);

            if (recognizedGesture == GestureType.Erase)
            {
                // Bounds of the erase stroke
                var pointsBounds = e.Stroke.GetBounds();

                // Remove ink strokes that intersect with the erase stroke
                var strokesToErase = AppInkCanvas.Strokes.HitTest(pointsBounds, 10);
                AppInkCanvas.Strokes.Remove(strokesToErase);

                // Remove the stroke from the ink canvas since it's recognized as an erase gesture
                AppInkCanvas.Strokes.Remove(e.Stroke);
            }
            else
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
                ResultsLabel.Content = KNutritionFactsStr;

                SetNutritionFactsView();

                ResultListView.Visibility = Visibility.Hidden;
                NutritionResultsViewer.Visibility = Visibility.Visible;
            }
            else
            {
                ResultButton.Content = KInfoDataStr;
                ResultsLabel.Content = KSearchResultsStr;

                ResultListView.Visibility = Visibility.Visible;
                NutritionResultsViewer.Visibility = Visibility.Hidden;
            }
        }
    }
}
