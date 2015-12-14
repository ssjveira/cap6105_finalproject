using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Ink;
using System.Windows.Input;
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

        // Search results that contain nutritional information about an item
        private Item m_ItemResult1 = null;
        private Item m_ItemResult2 = null;
        private Item m_ItemResult3 = null;

        public MainWindow()
        {
            InitializeComponent();

            // Use bezier smoothing when rendering the ink strokes
            AppInkCanvas.DefaultDrawingAttributes.FitToCurve = true;

            ResultButton.Content = KInfoDataStr;
        }

        private List<Tuple<string, decimal?>> CreateNutritionalFacts(Item item)
        {
            var nutritionFacts = new List<Tuple<string, decimal?>>
            {
                Tuple.Create("Calories", item.NutritionFact_Calories),
                Tuple.Create("Calories from", item.NutritionFact_CaloriesFromFat),
                Tuple.Create("Total Fat", item.NutritionFact_TotalFat),
                Tuple.Create("Saturated Fat", item.NutritionFact_SaturatedFat),
                Tuple.Create("Trans Fat", item.NutritionFact_TransFat),
                Tuple.Create("Cholesterol", item.NutritionFact_Cholesterol),
                Tuple.Create("Sodium", item.NutritionFact_Sodium),
                Tuple.Create("Total Carbohydrate", item.NutritionFact_TotalCarbohydrate),
                Tuple.Create("Dietary Fiber", item.NutritionFact_DietaryFiber),
                Tuple.Create("Sugars", item.NutritionFact_Sugar),
                Tuple.Create("Protein", item.NutritionFact_Protein),
                Tuple.Create("Vitamin A", item.NutritionFact_VitaminA),
                Tuple.Create("Vitamin C", item.NutritionFact_VitaminC),
                Tuple.Create("Calcium", item.NutritionFact_Calcium),
                Tuple.Create("Iron", item.NutritionFact_Iron)
            };


            return nutritionFacts;
        }

        private List<Tuple<string, decimal?, decimal?, decimal>> CreateNutritionalFacts(Item item, Item item2)
        {
            var calorieTotal = (item.NutritionFact_Calories ?? decimal.Zero) + (item2.NutritionFact_Calories ?? decimal.Zero);
            var caloriesFromFatTotal = (item.NutritionFact_CaloriesFromFat ?? decimal.Zero) + (item2.NutritionFact_CaloriesFromFat ?? decimal.Zero);
            var totalFatTotal = (item.NutritionFact_TotalFat ?? decimal.Zero) + (item2.NutritionFact_TotalFat ?? decimal.Zero);
            var saturatedFatTotal = (item.NutritionFact_SaturatedFat ?? decimal.Zero) + (item2.NutritionFact_SaturatedFat ?? decimal.Zero);
            var transFatTotal = (item.NutritionFact_TransFat ?? decimal.Zero) + (item2.NutritionFact_TransFat ?? decimal.Zero);
            var cholesterolTotal = (item.NutritionFact_Cholesterol ?? decimal.Zero) + (item2.NutritionFact_Cholesterol ?? decimal.Zero);
            var sodiumTotal = (item.NutritionFact_Sodium ?? decimal.Zero) + (item2.NutritionFact_Sodium ?? decimal.Zero);
            var totalCarbsTotal = (item.NutritionFact_TotalCarbohydrate ?? decimal.Zero) + (item2.NutritionFact_TotalCarbohydrate ?? decimal.Zero);
            var dietaryFiberTotal = (item.NutritionFact_DietaryFiber ?? decimal.Zero) + (item2.NutritionFact_DietaryFiber ?? decimal.Zero);
            var sugarsTotal = (item.NutritionFact_Sugar ?? decimal.Zero) + (item2.NutritionFact_Sugar ?? decimal.Zero);
            var proteinTotal = (item.NutritionFact_Protein ?? decimal.Zero) + (item2.NutritionFact_Protein ?? decimal.Zero);
            var vitaminATotal = (item.NutritionFact_VitaminA ?? decimal.Zero) + (item2.NutritionFact_VitaminA ?? decimal.Zero);
            var vitaminCTotal = (item.NutritionFact_VitaminC ?? decimal.Zero) + (item2.NutritionFact_VitaminC ?? decimal.Zero);
            var calciumTotal = (item.NutritionFact_Calcium ?? decimal.Zero) + (item2.NutritionFact_Calcium ?? decimal.Zero);
            var ironTotal = (item.NutritionFact_Iron ?? decimal.Zero) + (item2.NutritionFact_Iron ?? decimal.Zero);

            var nutritionFacts = new List<Tuple<string, decimal?, decimal?, decimal>>
            {
                Tuple.Create("Calories", item.NutritionFact_Calories, item2.NutritionFact_Calories, calorieTotal),
                Tuple.Create("Calories from", item.NutritionFact_CaloriesFromFat, item2.NutritionFact_CaloriesFromFat, caloriesFromFatTotal),
                Tuple.Create("Total Fat", item.NutritionFact_TotalFat, item2.NutritionFact_TotalFat, totalFatTotal),
                Tuple.Create("Saturated Fat", item.NutritionFact_SaturatedFat, item2.NutritionFact_SaturatedFat, saturatedFatTotal),
                Tuple.Create("Trans Fat", item.NutritionFact_TransFat, item2.NutritionFact_TransFat, transFatTotal),
                Tuple.Create("Cholesterol", item.NutritionFact_Cholesterol, item2.NutritionFact_Cholesterol, cholesterolTotal),
                Tuple.Create("Sodium", item.NutritionFact_Sodium, item2.NutritionFact_Sodium, sodiumTotal),
                Tuple.Create("Total Carbohydrate", item.NutritionFact_TotalCarbohydrate, item2.NutritionFact_TotalCarbohydrate, totalCarbsTotal),
                Tuple.Create("Dietary Fiber", item.NutritionFact_DietaryFiber, item2.NutritionFact_DietaryFiber, dietaryFiberTotal),
                Tuple.Create("Sugars", item.NutritionFact_Sugar, item2.NutritionFact_Sugar, sugarsTotal),
                Tuple.Create("Protein", item.NutritionFact_Protein, item2.NutritionFact_Protein, proteinTotal),
                Tuple.Create("Vitamin A", item.NutritionFact_VitaminA, item2.NutritionFact_VitaminA, vitaminATotal),
                Tuple.Create("Vitamin C", item.NutritionFact_VitaminC, item2.NutritionFact_VitaminC, vitaminCTotal),
                Tuple.Create("Calcium", item.NutritionFact_Calcium, item2.NutritionFact_Calcium, calciumTotal),
                Tuple.Create("Iron", item.NutritionFact_Iron, item2.NutritionFact_Iron, ironTotal)
            };


            return nutritionFacts;
        }

        private List<Tuple<string, decimal?, decimal?, decimal?, decimal>> CreateNutritionalFacts(Item item, Item item2, Item item3)
        {
            var calorieTotal = (item.NutritionFact_Calories ?? decimal.Zero) + (item2.NutritionFact_Calories ?? decimal.Zero) + (item3.NutritionFact_Calories ?? decimal.Zero);
            var caloriesFromFatTotal = (item.NutritionFact_CaloriesFromFat ?? decimal.Zero) + (item2.NutritionFact_CaloriesFromFat ?? decimal.Zero) + (item3.NutritionFact_CaloriesFromFat ?? decimal.Zero);
            var totalFatTotal = (item.NutritionFact_TotalFat ?? decimal.Zero) + (item2.NutritionFact_TotalFat ?? decimal.Zero) + (item3.NutritionFact_TotalFat ?? decimal.Zero);
            var saturatedFatTotal = (item.NutritionFact_SaturatedFat ?? decimal.Zero) + (item2.NutritionFact_SaturatedFat ?? decimal.Zero) + (item3.NutritionFact_SaturatedFat ?? decimal.Zero);
            var transFatTotal = (item.NutritionFact_TransFat ?? decimal.Zero) + (item2.NutritionFact_TransFat ?? decimal.Zero) + (item3.NutritionFact_TransFat ?? decimal.Zero);
            var cholesterolTotal = (item.NutritionFact_Cholesterol ?? decimal.Zero) + (item2.NutritionFact_Cholesterol ?? decimal.Zero) + (item3.NutritionFact_Cholesterol ?? decimal.Zero);
            var sodiumTotal = (item.NutritionFact_Sodium ?? decimal.Zero) + (item2.NutritionFact_Sodium ?? decimal.Zero) + (item3.NutritionFact_Sodium ?? decimal.Zero);
            var totalCarbsTotal = (item.NutritionFact_TotalCarbohydrate ?? decimal.Zero) + (item2.NutritionFact_TotalCarbohydrate ?? decimal.Zero) + (item3.NutritionFact_TotalCarbohydrate ?? decimal.Zero);
            var dietaryFiberTotal = (item.NutritionFact_DietaryFiber ?? decimal.Zero) + (item2.NutritionFact_DietaryFiber ?? decimal.Zero) + (item3.NutritionFact_DietaryFiber ?? decimal.Zero);
            var sugarsTotal = (item.NutritionFact_Sugar ?? decimal.Zero) + (item2.NutritionFact_Sugar ?? decimal.Zero) + (item3.NutritionFact_Sugar ?? decimal.Zero);
            var proteinTotal = (item.NutritionFact_Protein ?? decimal.Zero) + (item2.NutritionFact_Protein ?? decimal.Zero) + (item3.NutritionFact_Protein ?? decimal.Zero);
            var vitaminATotal = (item.NutritionFact_VitaminA ?? decimal.Zero) + (item2.NutritionFact_VitaminA ?? decimal.Zero) + (item3.NutritionFact_VitaminA ?? decimal.Zero);
            var vitaminCTotal = (item.NutritionFact_VitaminC ?? decimal.Zero) + (item2.NutritionFact_VitaminC ?? decimal.Zero) + (item3.NutritionFact_VitaminC ?? decimal.Zero);
            var calciumTotal = (item.NutritionFact_Calcium ?? decimal.Zero) + (item2.NutritionFact_Calcium ?? decimal.Zero) + (item3.NutritionFact_Calcium ?? decimal.Zero);
            var ironTotal = (item.NutritionFact_Iron ?? decimal.Zero) + (item2.NutritionFact_Iron ?? decimal.Zero) + (item3.NutritionFact_Iron ?? decimal.Zero);

            var nutritionFacts = new List<Tuple<string, decimal?, decimal?, decimal?, decimal>>
            {
                Tuple.Create("Calories", item.NutritionFact_Calories, item2.NutritionFact_Calories, item3.NutritionFact_Calories, calorieTotal),
                Tuple.Create("Calories from", item.NutritionFact_CaloriesFromFat, item2.NutritionFact_CaloriesFromFat, item3.NutritionFact_CaloriesFromFat, caloriesFromFatTotal),
                Tuple.Create("Total Fat", item.NutritionFact_TotalFat, item2.NutritionFact_TotalFat, item3.NutritionFact_TotalFat, totalFatTotal),
                Tuple.Create("Saturated Fat", item.NutritionFact_SaturatedFat, item2.NutritionFact_SaturatedFat, item3.NutritionFact_SaturatedFat, saturatedFatTotal),
                Tuple.Create("Trans Fat", item.NutritionFact_TransFat, item2.NutritionFact_TransFat, item3.NutritionFact_TransFat, transFatTotal),
                Tuple.Create("Cholesterol", item.NutritionFact_Cholesterol, item2.NutritionFact_Cholesterol, item3.NutritionFact_Cholesterol, cholesterolTotal),
                Tuple.Create("Sodium", item.NutritionFact_Sodium, item2.NutritionFact_Sodium, item3.NutritionFact_Sodium, sodiumTotal),
                Tuple.Create("Total Carbohydrate", item.NutritionFact_TotalCarbohydrate, item2.NutritionFact_TotalCarbohydrate, item3.NutritionFact_TotalCarbohydrate, totalCarbsTotal),
                Tuple.Create("Dietary Fiber", item.NutritionFact_DietaryFiber, item2.NutritionFact_DietaryFiber, item3.NutritionFact_DietaryFiber, dietaryFiberTotal),
                Tuple.Create("Sugars", item.NutritionFact_Sugar, item2.NutritionFact_Sugar, item3.NutritionFact_Sugar, sugarsTotal),
                Tuple.Create("Protein", item.NutritionFact_Protein, item2.NutritionFact_Protein, item3.NutritionFact_Protein, proteinTotal),
                Tuple.Create("Vitamin A", item.NutritionFact_VitaminA, item2.NutritionFact_VitaminA, item3.NutritionFact_VitaminA, vitaminATotal),
                Tuple.Create("Vitamin C", item.NutritionFact_VitaminC, item2.NutritionFact_VitaminC, item3.NutritionFact_VitaminC, vitaminCTotal),
                Tuple.Create("Calcium", item.NutritionFact_Calcium, item2.NutritionFact_Calcium, item3.NutritionFact_Calcium, calciumTotal),
                Tuple.Create("Iron", item.NutritionFact_Iron, item2.NutritionFact_Iron, item3.NutritionFact_Iron, ironTotal)
            };


            return nutritionFacts;
        }

        private void SetNutritionFactsView()
        {
            // Clear label
            NutritionResultLabel.Content = "";

            // Clear the columns
            NutritionalDataGrid.Columns.Clear();

            // Clear the data grid
            NutritionalDataGrid.ItemsSource = new List<Tuple<string, decimal?>>();

            // Don't bother making a table if we have no results
            if (m_ItemResult1 == null && m_ItemResult2 == null && m_ItemResult3 == null) return;

            var items = new List<Item>();
            var itemNames = "";

            if (m_ItemResult1 != null)
            {
                items.Add(m_ItemResult1);
                itemNames += "- " + m_ItemResult1.Name + "\n";
            }

            if (m_ItemResult2 != null)
            {
                items.Add(m_ItemResult2);
                itemNames += "- " + m_ItemResult2.Name + "\n";
            }

            if (m_ItemResult3 != null)
            {
                items.Add(m_ItemResult3);
                itemNames += "- " + m_ItemResult3.Name + "\n";
            }

            NutritionResultLabel.Content = itemNames;

            // Add "Feature Name" column
            var textColumn = new DataGridTextColumn {Binding = new Binding("Item1")};
            NutritionalDataGrid.Columns.Add(textColumn);

            if (items.Count == 1)
            {
                var item = items[0];

                // Add the columns
                var textColumn1 = new DataGridTextColumn
                {
                    Header = items[0].Name,
                    Binding = new Binding("Item2")
                };
                NutritionalDataGrid.Columns.Add(textColumn1);

                var nutritionFacts = CreateNutritionalFacts(item);

                NutritionalDataGrid.ItemsSource = nutritionFacts;
            }
            else if (items.Count == 2)
            {
                var item = items[0];
                var item2 = items[1];

                // Add the columns
                var textColumn1 = new DataGridTextColumn
                {
                    Header = items[0].Name,
                    Binding = new Binding("Item2")
                };
                NutritionalDataGrid.Columns.Add(textColumn1);

                var textColumn2 = new DataGridTextColumn
                {
                    Header = items[1].Name,
                    Binding = new Binding("Item3")
                };
                NutritionalDataGrid.Columns.Add(textColumn2);

                // Total column
                var totalColumn = new DataGridTextColumn
                {
                    Header = "Total",
                    Binding = new Binding("Item4")
                };
                NutritionalDataGrid.Columns.Add(totalColumn);

                var nutritionFacts = CreateNutritionalFacts(item, item2);

                NutritionalDataGrid.ItemsSource = nutritionFacts;
            }
            else if (items.Count >= 3)
            {
                var item = items[0];
                var item2 = items[1];
                var item3 = items[2];

                // Add the columns
                var textColumn1 = new DataGridTextColumn
                {
                    Header = items[0].Name,
                    Binding = new Binding("Item2")
                };
                NutritionalDataGrid.Columns.Add(textColumn1);

                var textColumn2 = new DataGridTextColumn
                {
                    Header = items[1].Name,
                    Binding = new Binding("Item3")
                };
                NutritionalDataGrid.Columns.Add(textColumn2);

                var textColumn3 = new DataGridTextColumn
                {
                    Header = items[2].Name,
                    Binding = new Binding("Item4")
                };
                NutritionalDataGrid.Columns.Add(textColumn3);

                // Total column
                var totalColumn = new DataGridTextColumn
                {
                    Header = "Total",
                    Binding = new Binding("Item5")
                };
                NutritionalDataGrid.Columns.Add(totalColumn);

                var nutritionFacts = CreateNutritionalFacts(item, item2, item3);

                NutritionalDataGrid.ItemsSource = nutritionFacts;
            }
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
                m_FoodSketchRecognizer.SetStrokes(AppInkCanvas.Strokes);

                // Print out text that will be used by NutritionixHelper to search for n objects and return the results
                // (i.e. With query string = "apple+tomato", nutritional info for an apple and a tomato will be retrieved
                // from the Nutritionix database.
                SearchText.Text = string.Join("+", m_FoodSketchRecognizer.Recognize());
            }

        }

        private void ResultListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Retrieve item data from database when a selection happens
            if (ReferenceEquals(e.Source, ResultListView1))
            {
                var searchResult = ResultListView1.SelectedItem as SearchResult;

                m_ItemResult1 = searchResult != null ? NutritionHelper.RetrieveItem(searchResult.Item.Id) : null;
            }
            else if (ReferenceEquals(e.Source, ResultListView2))
            {
                var searchResult = ResultListView2.SelectedItem as SearchResult;

                m_ItemResult2 = searchResult != null ? NutritionHelper.RetrieveItem(searchResult.Item.Id) : null;
            }
            else if (ReferenceEquals(e.Source, ResultListView3))
            {
                var searchResult = ResultListView3.SelectedItem as SearchResult;

                m_ItemResult3 = searchResult != null ? NutritionHelper.RetrieveItem(searchResult.Item.Id) : null;
            }

            SetNutritionFactsView();
        }

        private void ToggleResultView()
        {
            if (ResultButton.Content.Equals(KInfoDataStr))
            {
                ResultButton.Content = KBackStr;
                ResultsLabel.Content = KNutritionFactsStr;

                SearchResultsViewer.Visibility = Visibility.Hidden;
                NutritionResultsViewer.Visibility = Visibility.Visible;
            }
            else
            {
                ResultButton.Content = KInfoDataStr;
                ResultsLabel.Content = KSearchResultsStr;

                SearchResultsViewer.Visibility = Visibility.Visible;
                NutritionResultsViewer.Visibility = Visibility.Hidden;
            }
        }

        private void ResultButton_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleResultView();
        }

        private void SearchText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ResultLabel1.Visibility = Visibility.Hidden;
            ResultLabel2.Visibility = Visibility.Hidden;
            ResultLabel3.Visibility = Visibility.Hidden;

            ResultListView1.Visibility = Visibility.Hidden;
            ResultListView2.Visibility = Visibility.Hidden;
            ResultListView3.Visibility = Visibility.Hidden;

            ResultListView1.ItemsSource = new List<SearchResult>();
            ResultListView2.ItemsSource = new List<SearchResult>();
            ResultListView3.ItemsSource = new List<SearchResult>();

            var searchQueries = SearchText.Text.Split('+');

            // Perform initial search query result
            for (var i = 0; i < searchQueries.Length && i < 3; ++i)
            {
                if (i == 0)
                {
                    var searchResponse = NutritionHelper.Search(searchQueries[i]);
                    ResultListView1.ItemsSource = searchResponse.Results;
                    ResultLabel1.Content = "Query results for '" + searchQueries[i] + "'";

                    ResultLabel1.Visibility = Visibility.Visible;
                    ResultListView1.Visibility = Visibility.Visible;
                }
                else if (i == 1)
                {
                    var searchResponse = NutritionHelper.Search(searchQueries[i]);
                    ResultListView2.ItemsSource = searchResponse.Results;
                    ResultLabel2.Content = "Query results for '" + searchQueries[i] + "'";

                    ResultLabel2.Visibility = Visibility.Visible;
                    ResultListView2.Visibility = Visibility.Visible;
                }
                else if (i == 2)
                {
                    var searchResponse = NutritionHelper.Search(searchQueries[i]);
                    ResultListView3.ItemsSource = searchResponse.Results;
                    ResultLabel3.Content = "Query results for '" + searchQueries[i] + "'";

                    ResultLabel3.Visibility = Visibility.Visible;
                    ResultListView3.Visibility = Visibility.Visible;
                }
            }

            // Select the first items from eqch listview
            if (ResultListView1.ItemsSource.OfType<SearchResult>().Any())
                ResultListView1.SelectedIndex = 0;

            if (ResultListView2.ItemsSource.OfType<SearchResult>().Any())
                ResultListView2.SelectedIndex = 0;

            if (ResultListView3.ItemsSource.OfType<SearchResult>().Any())
                ResultListView3.SelectedIndex = 0;

            // Switch to the nutritional facts view
            if (ResultButton.Content.Equals(KInfoDataStr))
                ToggleResultView();
        }
    }
}
