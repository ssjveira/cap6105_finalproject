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
        private readonly string KInfoDataStr = "See info";
        private readonly string KBackStr = "Back to results";

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

        private void AppInkCanvas_OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            SearchText.Text = m_FoodSketchRecognizer.Recognize(AppInkCanvas.Strokes);
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
