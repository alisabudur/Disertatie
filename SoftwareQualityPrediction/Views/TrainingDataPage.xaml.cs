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

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (TrainingDataViewModel) DataContext;
            var trainingDataDto = viewModel.PrepareDto();
            NavigationService?.Navigate(new TrainingParametersPage(trainingDataDto));
        }
    }
}
