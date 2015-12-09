using System.Windows;

namespace NutritionixSamples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Nutritionix.Samples.NutritionixSamples.RunAll();
        }
    }
}
