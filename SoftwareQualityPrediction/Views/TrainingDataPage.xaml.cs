using System.Windows;
using System.Windows.Controls;
using SoftwareQualityPrediction.ViewModels;

namespace SoftwareQualityPrediction.Views
{
    /// <summary>
    /// Interaction logic for TrainingPage.xaml
    /// </summary>
    public partial class TrainingDataPage : Page
    {
        public TrainingDataPage()
        {
            InitializeComponent();
        }

        private void NavigateToNextPage(object sender, RoutedEventArgs e)
        {
            var viewModel = (TrainingDataViewModel) DataContext;
            var trainingDataDto = viewModel.Prepare();
            NavigationService?.Navigate(new TrainingParametersPage(trainingDataDto));
        }
    }
}
