using System.Windows;
using System.Windows.Controls;
using SoftwareQualityPrediction.Dtos;
using SoftwareQualityPrediction.ViewModels;

namespace SoftwareQualityPrediction.Views
{
    /// <summary>
    /// Interaction logic for TrainingParametersPage.xaml
    /// </summary>
    public partial class TrainingParametersPage : Page
    {
        public TrainingParametersPage(TrainingDataModel trainingDataModel)
        {
            InitializeComponent();
            var viewModel = (TrainingParametersViewModel) DataContext;
            viewModel.Populate(trainingDataModel);
        }

        private void NavigateToCrossValidationPage(object sender, RoutedEventArgs e)
        {
            var viewModel = (TrainingParametersViewModel)DataContext;
            NavigationService?.Navigate(new CrossValildationPage(viewModel.Prepare()));
        }

        private void NavigateToNextPage(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new TestingPage());
        }
    }
}
